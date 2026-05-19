using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class BreedingRepository : Repository<Breeding>, IBreedingRepository
    {
        private readonly CattleFarmDbContext _db;
        public BreedingRepository(CattleFarmDbContext db) : base(db) { _db = db; }

        public async Task<IEnumerable<Breeding>> GetByFarmIdAsync(int farmId)
            => await _db.Breedings
                .Include(b => b.Cattle)
                .Include(b => b.Sire)
                .Where(b => b.FarmId == farmId)
                .OrderByDescending(b => b.BreedingDate)
                .ToListAsync();

        public async Task<IEnumerable<Breeding>> GetByCattleIdAsync(int cattleId)
            => await _db.Breedings
                .Include(b => b.Sire)
                .Where(b => b.CattleId == cattleId)
                .OrderByDescending(b => b.BreedingDate)
                .ToListAsync();

        public async Task<IEnumerable<Breeding>> GetPendingAsync(int farmId)
            => await _db.Breedings
                .Include(b => b.Cattle)
                .Where(b => b.FarmId == farmId && b.Outcome == BreedingOutcome.Pending)
                .OrderBy(b => b.ExpectedCalvingDate)
                .ToListAsync();

        public async Task<(IEnumerable<Breeding> Items, int Total)> GetPagedAsync(
            int page, int pageSize, int? farmId = null, BreedingOutcome? outcome = null)
        {
            var q = _db.Breedings.Include(b => b.Cattle).Include(b => b.Sire).AsQueryable();
            if (farmId.HasValue)   q = q.Where(b => b.FarmId == farmId.Value);
            if (outcome.HasValue)  q = q.Where(b => b.Outcome == outcome.Value);
            var total = await q.CountAsync();
            var items = await q.OrderByDescending(b => b.BreedingDate)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
