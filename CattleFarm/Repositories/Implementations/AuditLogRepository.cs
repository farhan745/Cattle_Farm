using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, int entityId)
            => await _dbSet.Where(a => a.EntityName == entityName && a.EntityId == entityId)
                           .Include(a => a.User).OrderByDescending(a => a.Timestamp).ToListAsync();

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId)
            => await _dbSet.Where(a => a.UserId == userId).OrderByDescending(a => a.Timestamp).ToListAsync();

        public async Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(int page, int pageSize, string? entity = null, int? userId = null)
        {
            var q = _dbSet.Include(a => a.User).AsQueryable();
            if (!string.IsNullOrWhiteSpace(entity)) q = q.Where(a => a.EntityName == entity);
            if (userId.HasValue) q = q.Where(a => a.UserId == userId.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(a => a.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
