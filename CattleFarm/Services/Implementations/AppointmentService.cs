using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notifications;
        private readonly IImageService _images;

        public AppointmentService(IUnitOfWork uow, INotificationService notifications, IImageService images)
        {
            _uow = uow;
            _notifications = notifications;
            _images = images;
        }

        public async Task<IEnumerable<Appointment>> GetByFarmAsync(int farmId)
            => await _uow.Appointments.GetByFarmIdAsync(farmId);

        public async Task<IEnumerable<Appointment>> GetUpcomingAsync(int farmId, int daysAhead = 7)
            => await _uow.Appointments.GetUpcomingAsync(farmId, daysAhead);

        public async Task<Appointment?> GetByIdAsync(int id) => await _uow.Appointments.GetByIdAsync(id);

        public async Task<(IEnumerable<Appointment> Items, int Total)> GetPagedAsync(
            int page, int pageSize, int? farmId, AppointmentStatus? status,
            int? doctorId, IReadOnlyCollection<int>? ownerFarmIds)
            => await _uow.Appointments.GetPagedAsync(page, pageSize, farmId, status, doctorId, ownerFarmIds);

        public async Task<bool> CanViewAsync(int appointmentId, int userId, string? userRole)
        {
            if (userRole == AppRoles.Admin || userRole == AppRoles.Manager) return true;
            var appt = await _uow.Appointments.GetByIdAsync(appointmentId);
            if (appt is null) return false;
            if (userRole == AppRoles.Doctor)
            {
                var doctor = await _uow.Doctors.GetByUserIdAsync(userId);
                return doctor != null && appt.DoctorId == doctor.Id;
            }
            if (userRole == AppRoles.Owner)
            {
                var farm = appt.Farm ?? await _uow.Farms.GetByIdAsync(appt.FarmId);
                return farm?.OwnerId == userId;
            }
            return appt.CreatedByUserId == userId;
        }

        public async Task<Appointment> CreateAsync(AppointmentViewModel vm, int createdByUserId, string? userRole)
        {
            var farm = await _uow.Farms.GetByIdAsync(vm.FarmId)
                ?? throw new InvalidOperationException("Farm not found.");
            if (userRole == AppRoles.Owner && farm.OwnerId != createdByUserId)
                throw new UnauthorizedAccessException("You can only book for your own farms.");

            // Date validation should happen before downstream lookups so invalid requests
            // fail with a clear scheduling error.
            if (vm.ScheduledAt <= DateTime.Now)
                throw new InvalidOperationException("Appointment must be scheduled in the future.");

            var doctor = await _uow.Doctors.GetByIdAsync(vm.DoctorId)
                ?? throw new InvalidOperationException("Veterinarian not found.");
            if (doctor.ApprovalStatus != ApprovalStatus.Approved)
                throw new InvalidOperationException("This veterinarian is not available for booking.");

            // Double booking validation (within 1 hour / 59 minutes of another pending/accepted appointment)
            var existingAppointments = await _uow.Appointments.GetByDoctorIdAsync(vm.DoctorId);
            var overlap = existingAppointments.Any(a =>
                (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Accepted) &&
                a.ScheduledAt >= vm.ScheduledAt.AddMinutes(-59) &&
                a.ScheduledAt <= vm.ScheduledAt.AddMinutes(59)
            );
            if (overlap)
                throw new InvalidOperationException("This veterinarian is already booked for another appointment at or near this time slot.");

            var cattle = await _uow.Cattles.GetByIdAsync(vm.CattleId);
            if (cattle is null || cattle.FarmId != vm.FarmId)
                throw new InvalidOperationException("Cattle does not belong to the selected farm.");

            var appt = new Appointment
            {
                CattleId = vm.CattleId,
                DoctorId = vm.DoctorId,
                FarmId = vm.FarmId,
                ScheduledAt = vm.ScheduledAt,
                Reason = vm.Reason,
                Notes = vm.Notes,
                Status = AppointmentStatus.Pending,
                CreatedByUserId = createdByUserId
            };
            await _uow.Appointments.AddAsync(appt);
            await _uow.SaveChangesAsync();

            if (doctor.UserId.HasValue)
            {
                await _notifications.SendAsync(
                    doctor.UserId.Value,
                    "New appointment request",
                    $"Farm \"{farm.Name}\" requested a visit for {cattle.Name} on {vm.ScheduledAt:MMM dd, HH:mm}. Reason: {vm.Reason}",
                    NotificationType.AppointmentRequested,
                    "Appointment",
                    appt.Id);
            }

            return appt;
        }

        public async Task<bool> ApproveAsync(int id, int doctorUserId)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt is null || appt.Status != AppointmentStatus.Pending) return false;
            if (!await IsAssignedDoctorAsync(appt, doctorUserId)) return false;

            appt.Status = AppointmentStatus.Accepted;
            appt.AcceptedAt = DateTime.UtcNow;
            appt.UpdatedAt = DateTime.UtcNow;
            _uow.Appointments.Update(appt);
            await _uow.SaveChangesAsync();

            var farm = appt.Farm ?? await _uow.Farms.GetByIdAsync(appt.FarmId);
            if (farm != null)
            {
                await _notifications.SendAsync(
                    farm.OwnerId,
                    "Appointment accepted",
                    $"Dr. {appt.Doctor?.FullName ?? "Veterinarian"} accepted your visit on {appt.ScheduledAt:MMM dd, HH:mm}.",
                    NotificationType.AppointmentAccepted,
                    "Appointment",
                    appt.Id);
            }

            return true;
        }

        public async Task<bool> RejectAsync(int id, int doctorUserId, string? reason)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt is null || appt.Status != AppointmentStatus.Pending) return false;
            if (!await IsAssignedDoctorAsync(appt, doctorUserId)) return false;

            appt.Status = AppointmentStatus.Rejected;
            if (!string.IsNullOrWhiteSpace(reason))
                appt.Notes = string.IsNullOrWhiteSpace(appt.Notes) ? reason : $"{appt.Notes}\n[Rejected] {reason}";
            appt.UpdatedAt = DateTime.UtcNow;
            _uow.Appointments.Update(appt);
            await _uow.SaveChangesAsync();

            var farm = appt.Farm ?? await _uow.Farms.GetByIdAsync(appt.FarmId);
            if (farm != null)
            {
                await _notifications.SendAsync(
                    farm.OwnerId,
                    "Appointment declined",
                    $"Dr. {appt.Doctor?.FullName ?? "Veterinarian"} declined your request.{(string.IsNullOrWhiteSpace(reason) ? "" : $" Reason: {reason}")}",
                    NotificationType.AppointmentRejected,
                    "Appointment",
                    appt.Id);
            }

            return true;
        }

        public async Task<bool> CompleteAsync(CompleteAppointmentViewModel vm, int doctorUserId)
        {
            var appt = await _uow.Appointments.GetByIdAsync(vm.Id);
            if (appt is null || appt.Status != AppointmentStatus.Accepted) return false;
            if (!await IsAssignedDoctorAsync(appt, doctorUserId)) return false;

            var evidencePath = await _images.SaveUploadAsync(vm.EvidenceFile, "appointments/evidence");
            var prescriptionPath = await _images.SaveUploadAsync(vm.PrescriptionFile, "appointments/prescriptions");
            if (evidencePath is null || prescriptionPath is null) return false;

            appt.EvidenceImagePath = evidencePath;
            appt.PrescriptionPath = prescriptionPath;
            appt.CompletionNotes = vm.CompletionNotes;
            appt.Status = AppointmentStatus.Completed;
            appt.CompletedAt = DateTime.UtcNow;
            appt.UpdatedAt = DateTime.UtcNow;
            _uow.Appointments.Update(appt);
            await _uow.SaveChangesAsync();

            var farm = appt.Farm ?? await _uow.Farms.GetByIdAsync(appt.FarmId);
            if (farm != null)
            {
                await _notifications.SendAsync(
                    farm.OwnerId,
                    "Visit completed",
                    $"Dr. {appt.Doctor?.FullName ?? "Veterinarian"} completed the visit. View evidence and prescription on the appointment page.",
                    NotificationType.AppointmentCompleted,
                    "Appointment",
                    appt.Id);
            }

            return true;
        }

        public async Task<bool> CancelAsync(int id, int userId, string? userRole)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt is null) return false;
            if (appt.Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled or AppointmentStatus.Rejected)
                return false;

            if (userRole == AppRoles.Doctor)
            {
                if (!await IsAssignedDoctorAsync(appt, userId)) return false;
            }
            else if (userRole == AppRoles.Owner)
            {
                var farm = appt.Farm ?? await _uow.Farms.GetByIdAsync(appt.FarmId);
                if (farm?.OwnerId != userId) return false;
            }
            else if (userRole != AppRoles.Admin && userRole != AppRoles.Manager)
                return false;

            appt.Status = AppointmentStatus.Cancelled;
            appt.UpdatedAt = DateTime.UtcNow;
            _uow.Appointments.Update(appt);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt is null) return false;
            _uow.Appointments.Delete(appt);
            await _uow.SaveChangesAsync();
            return true;
        }

        private async Task<bool> IsAssignedDoctorAsync(Appointment appt, int doctorUserId)
        {
            var doctor = await _uow.Doctors.GetByUserIdAsync(doctorUserId);
            return doctor != null && appt.DoctorId == doctor.Id;
        }
    }
}
