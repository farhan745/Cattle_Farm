using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();
        Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null);
        Task<int> CountAsync();
        Task<Doctor?> GetByUserIdAsync(int userId);
        Task<Doctor?> GetByEmailAsync(string email);
        Task<IEnumerable<Doctor>> GetPendingApprovalAsync();
    }
}
