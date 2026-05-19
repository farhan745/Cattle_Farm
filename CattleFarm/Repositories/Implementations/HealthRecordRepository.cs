using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class HealthRecordRepository : Repository<HealthRecord>, IHealthRecordRepository
    {
        public HealthRecordRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<HealthRecord>> GetByCattleIdAsync(int cattleId)
            => await _dbSet.Where(h => h.CattleId == cattleId).Include(h => h.Doctor)
                           .OrderByDescending(h => h.RecordDate).ToListAsync();

        public async Task<HealthRecord?> GetLatestByCattleIdAsync(int cattleId)
            => await _dbSet.Where(h => h.CattleId == cattleId)
                           .OrderByDescending(h => h.RecordDate).FirstOrDefaultAsync();

        public async Task<IEnumerable<HealthRecord>> GetHighRiskAsync(int farmId)
            => await _dbSet.Include(h => h.Cattle).ThenInclude(c => c!.Farm)
                           .Where(h => h.Cattle!.FarmId == farmId && h.RiskLevel == RiskLevel.High)
                           .OrderByDescending(h => h.RecordDate).ToListAsync();

        public async Task<(IEnumerable<HealthRecord> Items, int Total)> GetPagedAsync(int page, int pageSize, int? cattleId = null, RiskLevel? riskLevel = null)
        {
            var q = _dbSet.Include(h => h.Cattle).Include(h => h.Doctor).AsQueryable();
            if (cattleId.HasValue) q = q.Where(h => h.CattleId == cattleId.Value);
            if (riskLevel.HasValue) q = q.Where(h => h.RiskLevel == riskLevel.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(h => h.RecordDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
