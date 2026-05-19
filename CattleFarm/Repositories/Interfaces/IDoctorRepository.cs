using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();
        Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null);
        Task<int> CountAsync();
    }
}
