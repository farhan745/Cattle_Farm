using CattleFarm.Models;

namespace CattleFarm.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<Subscription?> GetActiveAsync(int userId);
        Task<Subscription>  CreateAsync(int userId, SubscriptionPlan plan, decimal pricePaid, string? txRef = null);
        Task<bool>          CancelAsync(int userId);
        Task<bool>          HasActivePlanAsync(int userId);
        Task<bool>          HasPlanOrHigherAsync(int userId, SubscriptionPlan minimumPlan);
        Task<(IEnumerable<Subscription> Items, int Total)> GetPagedAsync(int page, int pageSize);
        Task<bool> RevokeAsync(int subscriptionId);
    }
}
