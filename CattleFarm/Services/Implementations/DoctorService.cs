using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace CattleFarm.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _img;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly INotificationService _notificationService;

        public DoctorService(
            IUnitOfWork uow,
            IImageService img,
            IWebHostEnvironment env,
            IEmailService emailService,
            IAuditService auditService,
            INotificationService notificationService)
        {
            _uow = uow;
            _img = img;
            _env = env;
            _emailService = emailService;
            _auditService = auditService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync() => await _uow.Doctors.GetAllAsync();
        public async Task<IEnumerable<Doctor>> GetAvailableAsync() => await _uow.Doctors.GetAvailableDoctorsAsync();
        public async Task<Doctor?> GetByIdAsync(int id) => await _uow.Doctors.GetByIdAsync(id);

        public async Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null)
            => await _uow.Doctors.GetPagedAsync(page, pageSize, search);

        public async Task<Doctor> CreateAsync(DoctorViewModel vm)
        {
            var imagePath = await _img.SaveImageAsync(vm.ImageFile, "doctors");
            var doctor = new Doctor
            {
                FullName = vm.FullName, Specialization = vm.Specialization, Phone = vm.Phone,
                Email = vm.Email, LicenseNumber = vm.LicenseNumber, ConsultationFee = vm.ConsultationFee,
                IsAvailable = vm.IsAvailable, Notes = vm.Notes, ImagePath = imagePath
            };
            await _uow.Doctors.AddAsync(doctor);
            await _uow.SaveChangesAsync();
            return doctor;
        }

        public async Task<bool> UpdateAsync(int id, DoctorViewModel vm)
        {
            var d = await _uow.Doctors.GetByIdAsync(id);
            if (d is null) return false;
            d.FullName = vm.FullName; d.Specialization = vm.Specialization; d.Phone = vm.Phone;
            d.Email = vm.Email; d.LicenseNumber = vm.LicenseNumber; d.ConsultationFee = vm.ConsultationFee;
            d.IsAvailable = vm.IsAvailable; d.Notes = vm.Notes; d.UpdatedAt = DateTime.UtcNow;
            if (vm.ImageFile != null) { _img.DeleteImage(d.ImagePath); d.ImagePath = await _img.SaveImageAsync(vm.ImageFile, "doctors"); }
            _uow.Doctors.Update(d);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var d = await _uow.Doctors.GetByIdAsync(id);
            if (d is null) return false;
            d.IsDeleted = true; d.DeletedAt = DateTime.UtcNow;
            _uow.Doctors.Update(d);
            await _uow.SaveChangesAsync();
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
                var photoPath = await _img.SaveImageAsync(vm.ProfilePhoto, "doctors")
                    ?? throw new InvalidOperationException("Failed to save profile photo.");

                var user = new User
                {
                    Username = vm.Email.Trim(),
                    FullName = vm.FullName.Trim(),
                    Email = vm.Email.Trim(),
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
                    Email = vm.Email.Trim(),
                    LicenseNumber = string.IsNullOrWhiteSpace(vm.LicenseNumber) ? null : vm.LicenseNumber.Trim(),
                    ConsultationFee = vm.ConsultationFee,
                    AvailableTimeSlot = vm.AvailableTimeSlot.Trim(),
                    YearsOfExperience = vm.YearsOfExperience,
                    ImagePath = photoPath,
                    IsAvailable = true,
                    IsActive = true,
                    IsVerified = false,
                    UserId = user.Id,
                    Notes = "Self-registered veterinarian.",
                    CreatedAt = DateTime.UtcNow
                };
                await _uow.Doctors.AddAsync(doctor);
                await _uow.SaveChangesAsync();
                await tx.CommitAsync();

                await _auditService.LogActivityAsync(user.Id, "Self-registered as veterinarian.", "Doctor", doctor.Id);
                return (user, doctor);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<Doctor> CompleteProfileAsync(CompleteDoctorProfileVM vm, DoctorInvitation? invitation = null)
        {
            var email = vm.Email.Trim();
            using var tx = await _uow.BeginTransactionAsync();
            try
            {
                var deletedUser = await _uow.Users.FindByEmailAsync(email, includeDeleted: true);
                if (deletedUser?.IsDeleted == true)
                    throw new InvalidOperationException("This email belongs to a deleted account. Please use a different email or contact support.");

                var user = await _uow.Users.GetByEmailAsync(email);
                if (user == null)
                {
                    user = new User
                    {
                        Username = email,
                        FullName = vm.FullName.Trim(),
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password, workFactor: 12),
                        Role = AppRoles.Doctor,
                        PhoneNumber = vm.PhoneNumber.Trim(),
                        IsEmailVerified = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _uow.Users.AddAsync(user);
                    await _uow.SaveChangesAsync();
                }
                else
                {
                    user.FullName = vm.FullName.Trim();
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password, workFactor: 12);
                    user.Role = AppRoles.Doctor;
                    user.PhoneNumber = vm.PhoneNumber.Trim();
                    user.IsActive = true;
                    user.UpdatedAt = DateTime.UtcNow;
                    _uow.Users.Update(user);
                    await _uow.SaveChangesAsync();
                }

                var photoPath = vm.ProfilePhoto != null
                    ? await _img.SaveImageAsync(vm.ProfilePhoto, "doctors")
                    : user.ProfileImagePath;

                if (!string.IsNullOrEmpty(photoPath))
                {
                    user.ProfileImagePath = photoPath;
                    user.UpdatedAt = DateTime.UtcNow;
                    _uow.Users.Update(user);
                    await _uow.SaveChangesAsync();
                }

                var licensePath = vm.LicenseDocument != null
                    ? await SaveDocumentAsync(vm.LicenseDocument, "licenses")
                    : null;

                var availableTimeSlot = BuildAvailableTimeSlot(vm);

                var doctor = await _uow.Doctors.GetByUserIdAsync(user.Id)
                          ?? await _uow.Doctors.GetByEmailAsync(email);

                if (doctor == null)
                {
                    doctor = new Doctor
                    {
                        FullName = vm.FullName.Trim(),
                        Specialization = vm.Specialization.Trim(),
                        Phone = vm.PhoneNumber.Trim(),
                        Email = email,
                        LicenseNumber = vm.LicenseNumber.Trim(),
                        ConsultationFee = vm.ConsultationFee,
                        ImagePath = photoPath,
                        IsAvailable = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Notes = invitation?.Notes,
                        DateOfBirth = vm.DateOfBirth,
                        Gender = vm.Gender,
                        Address = SafeTrim(vm.Address),
                        Qualification = SafeTrim(vm.Qualification),
                        University = SafeTrim(vm.University),
                        YearsOfExperience = vm.YearsOfExperience,
                        AvailableDays = vm.AvailableDays?.Count > 0 ? string.Join(",", vm.AvailableDays) : null,
                        AvailableTimeFrom = vm.AvailableTimeFrom,
                        AvailableTimeTo = vm.AvailableTimeTo,
                        AvailableTimeSlot = availableTimeSlot,
                        EmergencyAvailable = vm.EmergencyAvailable,
                        LicenseDocumentPath = licensePath,
                        IsVerified = false,
                        InvitationId = invitation?.Id,
                        UserId = user.Id
                    };
                    await _uow.Doctors.AddAsync(doctor);
                }
                else
                {
                    doctor.FullName = vm.FullName.Trim();
                    doctor.Specialization = vm.Specialization.Trim();
                    doctor.Phone = vm.PhoneNumber.Trim();
                    doctor.Email = email;
                    doctor.LicenseNumber = vm.LicenseNumber.Trim();
                    doctor.ConsultationFee = vm.ConsultationFee;
                    if (photoPath != null) doctor.ImagePath = photoPath;
                    doctor.IsActive = true;
                    doctor.IsAvailable = true;
                    if (string.IsNullOrWhiteSpace(doctor.Notes) && invitation != null)
                        doctor.Notes = invitation.Notes;
                    doctor.DateOfBirth = vm.DateOfBirth;
                    doctor.Gender = vm.Gender;
                    doctor.Address = SafeTrim(vm.Address);
                    doctor.Qualification = SafeTrim(vm.Qualification);
                    doctor.University = SafeTrim(vm.University);
                    doctor.YearsOfExperience = vm.YearsOfExperience;
                    doctor.AvailableDays = vm.AvailableDays?.Count > 0 ? string.Join(",", vm.AvailableDays) : doctor.AvailableDays;
                    doctor.AvailableTimeFrom = vm.AvailableTimeFrom;
                    doctor.AvailableTimeTo = vm.AvailableTimeTo;
                    doctor.AvailableTimeSlot = availableTimeSlot;
                    doctor.EmergencyAvailable = vm.EmergencyAvailable;
                    if (licensePath != null) doctor.LicenseDocumentPath = licensePath;
                    if (invitation != null) doctor.InvitationId = invitation.Id;
                    doctor.UserId = user.Id;
                    doctor.UpdatedAt = DateTime.UtcNow;
                    _uow.Doctors.Update(doctor);
                }

                if (invitation != null)
                {
                    invitation.IsUsed = true;
                    invitation.UsedAt = DateTime.UtcNow;
                    invitation.InvitationStatus = InvitationStatus.Accepted;
                    _uow.DoctorInvitations.Update(invitation);
                }

                await _uow.SaveChangesAsync();
                await tx.CommitAsync();

                await _auditService.LogActivityAsync(user.Id, "Completed doctor profile registration.", "Doctor", doctor.Id);

                if (invitation != null)
                {
                    await _notificationService.SendAsync(
                        invitation.CreatedByUserId,
                        "Doctor Registered",
                        $"Dr. {doctor.FullName} has completed their registration.",
                        NotificationType.DoctorRegistered, "Doctor", doctor.Id);
                }

                try
                {
                    await _emailService.SendDoctorWelcomeAsync(user.Email, doctor.FullName, "https://localhost:7170/Account/Login");
                }
                catch { /* email failure must not abort the registration */ }

                return doctor;
            }
            catch (DbUpdateException ex) when (IsDuplicateKey(ex))
            {
                await tx.RollbackAsync();
                throw new InvalidOperationException("An account with this email already exists. Please log in with your password.");
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static bool IsDuplicateKey(DbUpdateException ex)
        {
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                if (inner is SqlException sql && (sql.Number == 2601 || sql.Number == 2627))
                    return true;
            }
            return false;
        }

        private static string? SafeTrim(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? BuildAvailableTimeSlot(CompleteDoctorProfileVM vm)
        {
            var days = vm.AvailableDays?.Count > 0 ? string.Join(", ", vm.AvailableDays) : null;
            var hours = (!string.IsNullOrWhiteSpace(vm.AvailableTimeFrom) && !string.IsNullOrWhiteSpace(vm.AvailableTimeTo))
                ? $"{vm.AvailableTimeFrom} - {vm.AvailableTimeTo}"
                : null;
            if (days == null && hours == null) return null;
            if (days == null) return hours;
            if (hours == null) return days;
            return $"{days} ({hours})";
        }

        private async Task<string?> SaveDocumentAsync(IFormFile? file, string folder)
        {
            if (file is null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (_img.IsValidImage(file))
                return await _img.SaveImageAsync(file, folder);

            var allowedExts = new[] { ".pdf", ".docx", ".doc" };
            if (!allowedExts.Contains(ext))
                throw new InvalidOperationException("Invalid document format. Allowed formats: PDF, DOCX, or Images.");

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            return $"/uploads/{folder}/{fileName}";
        }
    }
}
