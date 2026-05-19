using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<Subscription?> GetActiveByUserIdAsync(int userId);
        Task<IEnumerable<Subscription>> GetExpiringAsync(int withinDays = 7);
        Task<(IEnumerable<Subscription> Items, int Total)> GetPagedAsync(int page, int pageSize);
    }
}
