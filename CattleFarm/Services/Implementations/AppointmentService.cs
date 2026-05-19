using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _uow;
        public AppointmentService(IUnitOfWork uow) { _uow = uow; }

        public async Task<IEnumerable<Appointment>> GetByFarmAsync(int farmId) => await _uow.Appointments.GetByFarmIdAsync(farmId);
        public async Task<IEnumerable<Appointment>> GetUpcomingAsync(int farmId, int daysAhead = 7) => await _uow.Appointments.GetUpcomingAsync(farmId, daysAhead);
        public async Task<Appointment?> GetByIdAsync(int id) => await _uow.Appointments.GetByIdAsync(id);

        public async Task<(IEnumerable<Appointment> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, AppointmentStatus? status = null)
            => await _uow.Appointments.GetPagedAsync(page, pageSize, farmId, status);

        public async Task<Appointment> CreateAsync(AppointmentViewModel vm, int createdByUserId)
        {
            var appt = new Appointment
            {
                CattleId = vm.CattleId, DoctorId = vm.DoctorId, FarmId = vm.FarmId,
                ScheduledAt = vm.ScheduledAt, Reason = vm.Reason, Notes = vm.Notes,
                CreatedByUserId = createdByUserId
            };
            await _uow.Appointments.AddAsync(appt);
            await _uow.SaveChangesAsync();
            return appt;
        }

        public async Task<bool> UpdateStatusAsync(int id, AppointmentStatus status, string? notes)
        {
            var appt = await _uow.Appointments.GetByIdAsync(id);
            if (appt is null) return false;
            appt.Status = status;
            if (notes != null) appt.Notes = notes;
            if (status == AppointmentStatus.Completed) appt.CompletedAt = DateTime.UtcNow;
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
    }
}
