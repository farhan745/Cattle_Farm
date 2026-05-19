using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class WorkerService : IWorkerService
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _img;
        public WorkerService(IUnitOfWork uow, IImageService img) { _uow = uow; _img = img; }

        public async Task<IEnumerable<Worker>> GetByFarmIdAsync(int farmId) => await _uow.Workers.GetByFarmIdAsync(farmId);
        public async Task<Worker?> GetByIdAsync(int id) => await _uow.Workers.GetByIdAsync(id);

        public async Task<(IEnumerable<Worker> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null)
            => await _uow.Workers.GetPagedAsync(page, pageSize, farmId, search);

        public async Task<Worker> CreateAsync(WorkerViewModel vm)
        {
            var imagePath = await _img.SaveImageAsync(vm.ImageFile, "workers");
            var worker = new Worker
            {
                FullName = vm.FullName, Role = vm.Role, Phone = vm.Phone, Email = vm.Email,
                Skills = vm.Skills, ExperienceYears = vm.ExperienceYears, Salary = vm.Salary,
                IsAvailable = vm.IsAvailable, FarmId = vm.FarmId, Notes = vm.Notes, ImagePath = imagePath
            };
            await _uow.Workers.AddAsync(worker);
            await _uow.SaveChangesAsync();
            return worker;
        }

        public async Task<bool> UpdateAsync(int id, WorkerViewModel vm)
        {
            var w = await _uow.Workers.GetByIdAsync(id);
            if (w is null) return false;
            w.FullName = vm.FullName; w.Role = vm.Role; w.Phone = vm.Phone; w.Email = vm.Email;
            w.Skills = vm.Skills; w.ExperienceYears = vm.ExperienceYears; w.Salary = vm.Salary;
            w.IsAvailable = vm.IsAvailable; w.Notes = vm.Notes; w.UpdatedAt = DateTime.UtcNow;
            if (vm.ImageFile != null) { _img.DeleteImage(w.ImagePath); w.ImagePath = await _img.SaveImageAsync(vm.ImageFile, "workers"); }
            _uow.Workers.Update(w);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var w = await _uow.Workers.GetByIdAsync(id);
            if (w is null) return false;
            w.IsDeleted = true; w.DeletedAt = DateTime.UtcNow;
            _uow.Workers.Update(w);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RecordAttendanceAsync(int workerId, AttendanceStatus status, DateTime? checkIn, DateTime? checkOut)
        {
            var attendance = new WorkerAttendance
            {
                WorkerId = workerId, Date = DateTime.UtcNow.Date, Status = status,
                CheckIn = checkIn, CheckOut = checkOut,
                HoursWorked = (checkIn.HasValue && checkOut.HasValue)
                    ? (checkOut.Value - checkIn.Value).TotalHours : null
            };
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
