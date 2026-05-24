using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Services.Implementations
{
    /// <summary>
    /// Feed management service — farm-scoped.
    /// Owner sees only their own farm(s) feed records.
    /// Admin/Manager sees all farms.
    /// </summary>
    public class FeedService : IFeedService
    {
        private readonly CattleFarmDbContext _context;

        public FeedService(CattleFarmDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<FeedRecord> Items, int Total)> GetPagedAsync(
            int page, int pageSize, int? farmId, FeedType? feedType,
            IEnumerable<int>? allowedFarmIds = null)
        {
            var query = _context.FeedRecords
                .Include(f => f.Farm)
                .Include(f => f.Cattle)
                .AsQueryable();

            // Farm-scoped filter for Owner role
            if (allowedFarmIds != null)
            {
                var ids = allowedFarmIds.ToHashSet();
                query = query.Where(f => ids.Contains(f.FarmId));
            }

            if (farmId.HasValue && farmId.Value > 0)
                query = query.Where(f => f.FarmId == farmId.Value);

            if (feedType.HasValue)
                query = query.Where(f => f.FeedType == feedType.Value);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<FeedRecord?> GetByIdAsync(int id)
            => await _context.FeedRecords
                .Include(f => f.Farm)
                .Include(f => f.Cattle)
                .FirstOrDefaultAsync(f => f.Id == id);

        public async Task AddAsync(FeedRecord record)
        {
            await _context.FeedRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(FeedRecord record)
        {
            _context.FeedRecords.Update(record);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var record = await _context.FeedRecords.FindAsync(id);
            if (record != null)
            {
                _context.FeedRecords.Remove(record);
                await _context.SaveChangesAsync();
            }
        }
    }
}
