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
        public DoctorService(IUnitOfWork uow, IImageService img) { _uow = uow; _img = img; }

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
                IsAvailable = vm.IsAvailable, FarmId = vm.FarmId, Notes = vm.Notes, ImagePath = imagePath
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
            d.IsAvailable = vm.IsAvailable; d.FarmId = vm.FarmId; d.Notes = vm.Notes; d.UpdatedAt = DateTime.UtcNow;
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
    }
}
