using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _img;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public DoctorService(
            IUnitOfWork uow,
            IImageService img,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _uow = uow;
            _img = img;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync() => await _uow.Doctors.GetAllAsync();
        public async Task<IEnumerable<Doctor>> GetAvailableAsync() => await _uow.Doctors.GetAvailableDoctorsAsync();
        public async Task<Doctor?> GetByIdAsync(int id) => await _uow.Doctors.GetByIdAsync(id);
        public async Task<Doctor?> GetByUserIdAsync(int userId) => await _uow.Doctors.GetByUserIdAsync(userId);
        public async Task<IEnumerable<Doctor>> GetPendingApprovalAsync() => await _uow.Doctors.GetPendingApprovalAsync();

        public async Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null)
            => await _uow.Doctors.GetPagedAsync(page, pageSize, search);

        public async Task<bool> UpdateAsync(int id, DoctorViewModel vm)
        {
            var d = await _uow.Doctors.GetByIdAsync(id);
            if (d is null) return false;
            d.FullName = vm.FullName;
            d.Specialization = vm.Specialization;
            d.Phone = vm.Phone;
            d.Email = vm.Email;
            d.LicenseNumber = vm.LicenseNumber;
            d.ConsultationFee = vm.ConsultationFee;
            d.IsAvailable = vm.IsAvailable;
            d.Notes = vm.Notes;
            d.UpdatedAt = DateTime.UtcNow;
            if (vm.ImageFile != null)
            {
                _img.DeleteImage(d.ImagePath);
                d.ImagePath = await _img.SaveImageAsync(vm.ImageFile, "doctors");
            }
            _uow.Doctors.Update(d);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveAsync(int doctorId, int adminUserId)
        {
            var doctor = await _uow.Doctors.GetByIdAsync(doctorId);
            if (doctor is null || doctor.ApprovalStatus != ApprovalStatus.Pending)
                return false;

            doctor.ApprovalStatus = ApprovalStatus.Approved;
            doctor.IsActive = true;
            doctor.IsAvailable = true;
            doctor.IsVerified = true;
            doctor.UpdatedAt = DateTime.UtcNow;
            _uow.Doctors.Update(doctor);
            await _uow.SaveChangesAsync();

            await _auditService.LogActivityAsync(adminUserId, $"Approved veterinarian Dr. {doctor.FullName}", "Doctor", doctor.Id);

            if (doctor.UserId.HasValue)
            {
                await _notificationService.SendAsync(
                    doctor.UserId.Value,
                    "Veterinarian profile approved",
                    "Your veterinarian registration has been approved. You are now listed on the platform and can receive appointments.",
                    NotificationType.DoctorApproved,
                    "Doctor",
                    doctor.Id);
            }

            return true;
        }

        public async Task<bool> RejectAsync(int doctorId, int adminUserId)
        {
            var doctor = await _uow.Doctors.GetByIdAsync(doctorId);
            if (doctor is null || doctor.ApprovalStatus != ApprovalStatus.Pending)
                return false;

            doctor.ApprovalStatus = ApprovalStatus.Rejected;
            doctor.IsAvailable = false;
            doctor.UpdatedAt = DateTime.UtcNow;
            _uow.Doctors.Update(doctor);
            await _uow.SaveChangesAsync();

            await _auditService.LogActivityAsync(adminUserId, $"Rejected veterinarian application: Dr. {doctor.FullName}", "Doctor", doctor.Id);

            if (doctor.UserId.HasValue)
            {
                await _notificationService.SendAsync(
                    doctor.UserId.Value,
                    "Veterinarian registration not approved",
                    "Your veterinarian registration was not approved. Please contact support if you have questions.",
                    NotificationType.Warning,
                    "Doctor",
                    doctor.Id);
            }

            return true;
        }

        public async Task<(User User, Doctor Doctor)> SelfRegisterAsync(DoctorSelfRegisterVM vm)
        {
            var email = vm.Email.Trim();
            if (await _uow.Users.EmailExistsIncludingDeletedAsync(email))
                throw new InvalidOperationException("An account with this email already exists. Please log in instead.");

            using var tx = await _uow.BeginTransactionAsync();
            try
            {
                var photoPath = await _img.SaveImageAsync(vm.ProfilePhoto, "doctors");
                if (photoPath is null)
                    throw new InvalidOperationException("Profile photo must be a valid image file (JPG, PNG, WEBP, or GIF).");

                var user = new User
                {
                    Username = email,
                    FullName = vm.FullName.Trim(),
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password, workFactor: 12),
                    Role = AppRoles.Doctor,
                    PhoneNumber = vm.PhoneNumber.Trim(),
                    ProfileImagePath = photoPath,
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Users.AddAsync(user);
                await _uow.SaveChangesAsync();

                var doctor = new Doctor
                {
                    FullName = vm.FullName.Trim(),
                    Specialization = vm.Specialization.Trim(),
                    Phone = vm.PhoneNumber.Trim(),
                    Email = email,
                    LicenseNumber = string.IsNullOrWhiteSpace(vm.LicenseNumber) ? null : vm.LicenseNumber.Trim(),
                    ConsultationFee = vm.ConsultationFee,
                    AvailableTimeSlot = vm.AvailableTimeSlot.Trim(),
                    YearsOfExperience = vm.YearsOfExperience,
                    ImagePath = photoPath,
                    IsAvailable = false,
                    IsActive = true,
                    IsVerified = false,
                    ApprovalStatus = ApprovalStatus.Pending,
                    UserId = user.Id,
                    Notes = "Awaiting admin approval.",
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Doctors.AddAsync(doctor);
                await _uow.SaveChangesAsync();
                await tx.CommitAsync();

                await _auditService.LogActivityAsync(user.Id, "Registered as veterinarian (pending admin approval).", "Doctor", doctor.Id);

                await _notificationService.SendToRoleAsync(
                    AppRoles.Admin,
                    "New veterinarian registration",
                    $"Dr. {doctor.FullName} ({doctor.Email}) has registered and is waiting for your approval.",
                    NotificationType.DoctorPendingApproval);

                return (user, doctor);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
