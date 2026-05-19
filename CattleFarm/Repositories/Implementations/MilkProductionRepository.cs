using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class MilkProductionRepository : Repository<MilkProduction>, IMilkProductionRepository
    {
        public MilkProductionRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<MilkProduction>> GetByFarmIdAsync(int farmId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(m => m.FarmId == farmId).Include(m => m.Cattle).AsQueryable();
            if (from.HasValue) q = q.Where(m => m.Date >= from.Value);
            if (to.HasValue)   q = q.Where(m => m.Date <= to.Value);
            return await q.OrderByDescending(m => m.Date).ToListAsync();
        }

        public async Task<IEnumerable<MilkProduction>> GetByCattleIdAsync(int cattleId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(m => m.CattleId == cattleId).AsQueryable();
            if (from.HasValue) q = q.Where(m => m.Date >= from.Value);
            if (to.HasValue)   q = q.Where(m => m.Date <= to.Value);
            return await q.OrderByDescending(m => m.Date).ToListAsync();
        }

        public async Task<double> GetTotalYieldByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(m => m.FarmId == farmId).AsQueryable();
            if (from.HasValue) q = q.Where(m => m.Date >= from.Value);
            if (to.HasValue)   q = q.Where(m => m.Date <= to.Value);
            return await q.SumAsync(m => m.MorningYieldLiters + m.EveningYieldLiters);
        }

        public async Task<double> GetTotalYieldByCattleAsync(int cattleId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(m => m.CattleId == cattleId).AsQueryable();
            if (from.HasValue) q = q.Where(m => m.Date >= from.Value);
            if (to.HasValue)   q = q.Where(m => m.Date <= to.Value);
            return await q.SumAsync(m => m.MorningYieldLiters + m.EveningYieldLiters);
        }
    }
}
