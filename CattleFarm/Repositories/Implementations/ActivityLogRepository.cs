using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class ActivityLogRepository : Repository<ActivityLog>, IActivityLogRepository
    {
        public ActivityLogRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<ActivityLog>> GetRecentAsync(int count = 20)
            => await _dbSet.Include(a => a.User).OrderByDescending(a => a.Timestamp).Take(count).ToListAsync();

        public async Task<IEnumerable<ActivityLog>> GetByUserIdAsync(int userId, int count = 50)
            => await _dbSet.Where(a => a.UserId == userId).OrderByDescending(a => a.Timestamp).Take(count).ToListAsync();
    }
}
