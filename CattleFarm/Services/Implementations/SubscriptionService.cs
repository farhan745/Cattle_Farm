using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;

namespace CattleFarm.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUnitOfWork _uow;
        public SubscriptionService(IUnitOfWork uow) { _uow = uow; }

        public async Task<Subscription?> GetActiveAsync(int userId) => await _uow.Subscriptions.GetActiveByUserIdAsync(userId);

        public async Task<bool> HasActivePlanAsync(int userId)
        {
            var sub = await GetActiveAsync(userId);
            return sub is not null && sub.IsActive && sub.ExpiryDate >= DateTime.UtcNow;
        }

        public async Task<bool> HasPlanOrHigherAsync(int userId, SubscriptionPlan minimumPlan)
        {
            var sub = await GetActiveAsync(userId);
            if (sub is null) return minimumPlan == SubscriptionPlan.Free;
            return sub.Plan >= minimumPlan;
        }

        public async Task<Subscription> CreateAsync(int userId, SubscriptionPlan plan, decimal pricePaid, string? txRef = null)
        {
            var existing = await GetActiveAsync(userId);
            if (existing != null) { existing.IsActive = false; _uow.Subscriptions.Update(existing); }

            var sub = new Subscription
            {
                UserId = userId, Plan = plan, PricePaid = pricePaid, TransactionRef = txRef,
                StartDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(plan == SubscriptionPlan.Enterprise ? 12 : 1),
                IsActive = true
            };
            await _uow.Subscriptions.AddAsync(sub);
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user != null) { user.SubscriptionType = plan.ToString(); user.SubscriptionExpiry = sub.ExpiryDate; user.UpdatedAt = DateTime.UtcNow; _uow.Users.Update(user); }
            await _uow.SaveChangesAsync();
            return sub;
        }

        public async Task<bool> CancelAsync(int userId)
        {
            var sub = await GetActiveAsync(userId);
            if (sub is null) return false;
            sub.IsActive = false; sub.UpdatedAt = DateTime.UtcNow;
            _uow.Subscriptions.Update(sub);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<(IEnumerable<Subscription> Items, int Total)> GetPagedAsync(int page, int pageSize)
            => await _uow.Subscriptions.GetPagedAsync(page, pageSize);

        public async Task<bool> RevokeAsync(int subscriptionId)
        {
            var sub = await _uow.Subscriptions.GetByIdAsync(subscriptionId);
            if (sub is null) return false;
            sub.IsActive = false; sub.UpdatedAt = DateTime.UtcNow;
            _uow.Subscriptions.Update(sub);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
