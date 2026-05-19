using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class RevenueRepository : Repository<Revenue>, IRevenueRepository
    {
        public RevenueRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Revenue>> GetByFarmIdAsync(int farmId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(r => r.FarmId == farmId).AsQueryable();
            if (from.HasValue) q = q.Where(r => r.Date >= from.Value);
            if (to.HasValue)   q = q.Where(r => r.Date <= to.Value);
            return await q.OrderByDescending(r => r.Date).ToListAsync();
        }

        public async Task<decimal> GetTotalByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(r => r.FarmId == farmId).AsQueryable();
            if (from.HasValue) q = q.Where(r => r.Date >= from.Value);
            if (to.HasValue)   q = q.Where(r => r.Date <= to.Value);
            return await q.SumAsync(r => r.Amount);
        }

        public async Task<(IEnumerable<Revenue> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, RevenueSource? source = null)
        {
            var q = _dbSet.AsQueryable();
            if (farmId.HasValue) q = q.Where(r => r.FarmId == farmId.Value);
            if (source.HasValue) q = q.Where(r => r.Source == source.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(r => r.Date).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
