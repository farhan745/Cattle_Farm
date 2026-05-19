using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IFarmRepository : IRepository<Farm>
    {
        Task<IEnumerable<Farm>> GetByOwnerIdAsync(int ownerId);
        Task<IEnumerable<Farm>> GetApprovedFarmsAsync();
        Task<IEnumerable<Farm>> SearchAsync(string keyword);
        Task<Farm?>             GetWithDetailsAsync(int id);
        Task<int>               CountAsync();
        Task<IEnumerable<Farm>> GetPagedAsync(int page, int pageSize, string? search = null);
    }
}
