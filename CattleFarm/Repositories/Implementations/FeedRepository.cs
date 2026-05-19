using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class FeedRepository : Repository<FeedRecord>, IFeedRepository
    {
        private readonly CattleFarmDbContext _db;
        public FeedRepository(CattleFarmDbContext db) : base(db) { _db = db; }

        public async Task<IEnumerable<FeedRecord>> GetByFarmIdAsync(int farmId)
            => await _db.FeedRecords
                .Include(f => f.Cattle)
                .Include(f => f.RecordedByWorker)
                .Where(f => f.FarmId == farmId)
                .OrderByDescending(f => f.Date)
                .ToListAsync();

        public async Task<IEnumerable<FeedRecord>> GetByCattleIdAsync(int cattleId)
            => await _db.FeedRecords
                .Where(f => f.CattleId == cattleId)
                .OrderByDescending(f => f.Date)
                .ToListAsync();

        public async Task<decimal> GetTotalCostAsync(int farmId, DateTime from, DateTime to)
            => await _db.FeedRecords
                .Where(f => f.FarmId == farmId && f.Date >= from && f.Date < to)
                .SumAsync(f => (decimal)f.QuantityKg * f.CostPerKg);

        public async Task<(IEnumerable<FeedRecord> Items, int Total)> GetPagedAsync(
            int page, int pageSize, int? farmId = null, FeedType? feedType = null)
        {
            var q = _db.FeedRecords.Include(f => f.Cattle).AsQueryable();
            if (farmId.HasValue)   q = q.Where(f => f.FarmId == farmId.Value);
            if (feedType.HasValue) q = q.Where(f => f.FeedType == feedType.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(f => f.Date)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
