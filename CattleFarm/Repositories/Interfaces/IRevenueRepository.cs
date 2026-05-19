using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IRevenueRepository : IRepository<Revenue>
    {
        Task<IEnumerable<Revenue>> GetByFarmIdAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<decimal>  GetTotalByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<(IEnumerable<Revenue> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, RevenueSource? source = null);
    }
}
