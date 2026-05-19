using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface ICattleRepository : IRepository<Cattle>
    {
        Task<IEnumerable<Cattle>> SearchAsync(string keyword);
        Task<(IEnumerable<Cattle> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, int? farmId = null, CattleStatus? status = null);
        Task<IEnumerable<Cattle>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Cattle>> GetByStatusAsync(CattleStatus status);
        Task<IEnumerable<Cattle>> GetListedForSaleAsync();
        Task<Cattle?>             GetWithDetailsAsync(int id);
        Task<int>                 CountAsync();
        Task<int>                 CountByFarmAsync(int farmId);
    }
}
