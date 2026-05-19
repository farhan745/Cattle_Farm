using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class CattleRepository : Repository<Cattle>, ICattleRepository
    {
        public CattleRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Cattle>> SearchAsync(string keyword)
            => await _dbSet.Where(c => c.Name.Contains(keyword) || c.Breed.Contains(keyword) || c.TagId.Contains(keyword))
                           .Include(c => c.Farm).ToListAsync();

        public async Task<(IEnumerable<Cattle> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, int? farmId = null, CattleStatus? status = null)
        {
            var q = _dbSet.Include(c => c.Farm).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(c => c.Name.Contains(search) || c.Breed.Contains(search) || c.TagId.Contains(search));
            if (farmId.HasValue) q = q.Where(c => c.FarmId == farmId.Value);
            if (status.HasValue) q = q.Where(c => c.Status == status.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(c => c.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<IEnumerable<Cattle>> GetByFarmIdAsync(int farmId)
            => await _dbSet.Where(c => c.FarmId == farmId).Include(c => c.Farm).ToListAsync();

        public async Task<IEnumerable<Cattle>> GetByStatusAsync(CattleStatus status)
            => await _dbSet.Where(c => c.Status == status).Include(c => c.Farm).ToListAsync();

        public async Task<IEnumerable<Cattle>> GetListedForSaleAsync()
            => await _dbSet.Where(c => c.IsListedForSale && c.Status == CattleStatus.Active && c.HealthStatus == HealthStatus.Healthy)
                           .Include(c => c.Farm).ToListAsync();

        public async Task<Cattle?> GetWithDetailsAsync(int id)
            => await _dbSet.Where(c => c.Id == id)
                           .Include(c => c.Farm)
                           .Include(c => c.HealthRecords.OrderByDescending(h => h.RecordDate).Take(5))
                           .Include(c => c.Vaccinations.OrderByDescending(v => v.VaccinationDate).Take(5))
                           .Include(c => c.MedicineRecords)
                           .Include(c => c.MilkProductions.OrderByDescending(m => m.Date).Take(30))
                           .FirstOrDefaultAsync();

        public async Task<int> CountAsync() => await _dbSet.CountAsync();

        public async Task<int> CountByFarmAsync(int farmId)
            => await _dbSet.CountAsync(c => c.FarmId == farmId);
    }
}
