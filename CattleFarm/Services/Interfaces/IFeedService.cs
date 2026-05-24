using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IFeedService
    {
        Task<(IEnumerable<FeedRecord> Items, int Total)> GetPagedAsync(
            int page, int pageSize, int? farmId, FeedType? feedType,
            IEnumerable<int>? allowedFarmIds = null);

        Task<FeedRecord?> GetByIdAsync(int id);
        Task AddAsync(FeedRecord record);
        Task UpdateAsync(FeedRecord record);
        Task DeleteAsync(int id);
    }
}
