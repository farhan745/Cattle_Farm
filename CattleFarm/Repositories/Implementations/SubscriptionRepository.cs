using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<Subscription?> GetActiveByUserIdAsync(int userId)
            => await _dbSet.Where(s => s.UserId == userId && s.IsActive && s.ExpiryDate >= DateTime.UtcNow)
                           .OrderByDescending(s => s.ExpiryDate).FirstOrDefaultAsync();

        public async Task<IEnumerable<Subscription>> GetExpiringAsync(int withinDays = 7)
        {
            var cutoff = DateTime.UtcNow.AddDays(withinDays);
            return await _dbSet.Include(s => s.User)
                               .Where(s => s.IsActive && s.ExpiryDate <= cutoff && s.ExpiryDate >= DateTime.UtcNow)
                               .OrderBy(s => s.ExpiryDate).ToListAsync();
        }

        public async Task<(IEnumerable<Subscription> Items, int Total)> GetPagedAsync(int page, int pageSize)
        {
            int total = await _dbSet.CountAsync();
            var items = await _dbSet.Include(s => s.User).OrderByDescending(s => s.CreatedAt)
                                    .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
