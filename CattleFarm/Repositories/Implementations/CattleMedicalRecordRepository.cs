using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class CattleMedicalRecordRepository : Repository<CattleMedicalRecord>, ICattleMedicalRecordRepository
    {
        public CattleMedicalRecordRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<CattleMedicalRecord>> GetByCattleIdAsync(int cattleId)
            => await _dbSet
                .Where(m => m.CattleId == cattleId)
                .Include(m => m.Doctor)
                .OrderByDescending(m => m.ExaminationDate)
                .ToListAsync();

        public async Task<CattleMedicalRecord?> GetByIdWithDetailsAsync(int id)
            => await _dbSet
                .Include(m => m.Cattle)
                .Include(m => m.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);
    }
}
