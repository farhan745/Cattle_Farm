using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class VaccinationRepository : Repository<Vaccination>, IVaccinationRepository
    {
        public VaccinationRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Vaccination>> GetByCattleIdAsync(int cattleId)
            => await _dbSet.Where(v => v.CattleId == cattleId)
                           .OrderByDescending(v => v.VaccinationDate).ToListAsync();

        public async Task<IEnumerable<Vaccination>> GetUpcomingAsync(int daysAhead = 30)
        {
            var cutoff = DateTime.UtcNow.AddDays(daysAhead);
            return await _dbSet.Include(v => v.Cattle).ThenInclude(c => c!.Farm)
                               .Where(v => v.NextDueDate.HasValue && v.NextDueDate <= cutoff && v.NextDueDate >= DateTime.UtcNow)
                               .OrderBy(v => v.NextDueDate).ToListAsync();
        }

        public async Task<IEnumerable<Vaccination>> GetOverdueAsync()
            => await _dbSet.Include(v => v.Cattle)
                           .Where(v => v.NextDueDate.HasValue && v.NextDueDate < DateTime.UtcNow)
                           .OrderBy(v => v.NextDueDate).ToListAsync();
    }
}
