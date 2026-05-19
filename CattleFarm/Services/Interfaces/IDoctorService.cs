using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<IEnumerable<Doctor>> GetAvailableAsync();
        Task<Doctor?> GetByIdAsync(int id);
        Task<Doctor>  CreateAsync(DoctorViewModel vm);
        Task<bool>    UpdateAsync(int id, DoctorViewModel vm);
        Task<bool>    DeleteAsync(int id);
        Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null);
    }
}
