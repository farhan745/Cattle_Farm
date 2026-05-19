using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IWorkerService
    {
        Task<IEnumerable<Worker>> GetByFarmIdAsync(int farmId);
        Task<Worker?> GetByIdAsync(int id);
        Task<Worker>  CreateAsync(WorkerViewModel vm);
        Task<bool>    UpdateAsync(int id, WorkerViewModel vm);
        Task<bool>    DeleteAsync(int id);
        Task<(IEnumerable<Worker> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null);
        Task<bool>    RecordAttendanceAsync(int workerId, AttendanceStatus status, DateTime? checkIn, DateTime? checkOut);
    }
}
