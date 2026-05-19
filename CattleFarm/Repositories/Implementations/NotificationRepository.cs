using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false)
        {
            var q = _dbSet.Where(n => n.UserId == userId).AsQueryable();
            if (unreadOnly) q = q.Where(n => !n.IsRead);
            return await q.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
            => await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task MarkAllReadAsync(int userId)
        {
            var unread = await _dbSet.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
            foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.UtcNow; }
        }
    }
}
