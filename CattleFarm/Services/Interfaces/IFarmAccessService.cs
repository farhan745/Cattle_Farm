using CattleFarm.Models;

namespace CattleFarm.Services.Interfaces
{
    public interface IFarmAccessService
    {
        Task<bool> IsFarmOwnerAsync(int farmId, int userId);
        Task<bool> IsAssignedManagerAsync(int farmId, int userId);
        Task<bool> CanOperateFarmAsync(int farmId, int userId, string? role);
        Task<bool> CanOwnFarmEntityAsync(int farmId, int userId, string? role);
        Task<int?> GetActiveManagerFarmIdAsync(int userId);
        Task<IReadOnlyList<int>> GetAccessibleFarmIdsAsync(int userId, string? role);
        Task<IEnumerable<Farm>> GetAccessibleFarmsAsync(int userId, string? role);
    }
}
