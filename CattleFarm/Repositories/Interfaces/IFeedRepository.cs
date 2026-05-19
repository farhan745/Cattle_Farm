using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IFeedRepository : IRepository<FeedRecord>
    {
        Task<IEnumerable<FeedRecord>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<FeedRecord>> GetByCattleIdAsync(int cattleId);
        Task<decimal> GetTotalCostAsync(int farmId, DateTime from, DateTime to);
        Task<(IEnumerable<FeedRecord> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, FeedType? feedType = null);
    }
}
