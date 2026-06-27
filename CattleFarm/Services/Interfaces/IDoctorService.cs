using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<Doctor>> GetAllAsync();
        Task<IEnumerable<Doctor>> GetAvailableAsync();
        Task<Doctor?> GetByIdAsync(int id);
        Task<Doctor?> GetByUserIdAsync(int userId);
        Task<bool> UpdateAsync(int id, DoctorViewModel vm);
        Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null);
        Task<IEnumerable<Doctor>> GetPendingApprovalAsync();
        Task<bool> ApproveAsync(int doctorId, int adminUserId);
        Task<bool> RejectAsync(int doctorId, int adminUserId);
        Task<(User User, Doctor Doctor)> SelfRegisterAsync(DoctorSelfRegisterVM vm);
    }
}
