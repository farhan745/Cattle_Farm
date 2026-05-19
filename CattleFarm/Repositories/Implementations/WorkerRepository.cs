using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class WorkerRepository : Repository<Worker>, IWorkerRepository
    {
        public WorkerRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Worker>> GetByFarmIdAsync(int farmId)
            => await _dbSet.Where(w => w.FarmId == farmId).Include(w => w.Farm).ToListAsync();

        public async Task<IEnumerable<Worker>> GetAvailableWorkersAsync(int farmId)
            => await _dbSet.Where(w => w.FarmId == farmId && w.IsAvailable && w.IsActive).ToListAsync();

        public async Task<(IEnumerable<Worker> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null)
        {
            var q = _dbSet.Include(w => w.Farm).AsQueryable();
            if (farmId.HasValue) q = q.Where(w => w.FarmId == farmId.Value);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(w => w.FullName.Contains(search) || w.Role.Contains(search));
            int total = await q.CountAsync();
            var items = await q.OrderBy(w => w.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<int> CountByFarmAsync(int farmId)
            => await _dbSet.CountAsync(w => w.FarmId == farmId);
    }
}
