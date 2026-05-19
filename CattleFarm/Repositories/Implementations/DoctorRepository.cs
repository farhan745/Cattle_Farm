using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Doctor>> GetByFarmIdAsync(int farmId)
            => await _dbSet.Where(d => d.FarmId == farmId).ToListAsync();

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
            => await _dbSet.Where(d => d.IsAvailable && d.IsActive).ToListAsync();

        public async Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var q = _dbSet.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(d => d.FullName.Contains(search) || d.Specialization.Contains(search));
            int total = await q.CountAsync();
            var items = await q.OrderBy(d => d.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<int> CountAsync() => await _dbSet.CountAsync();
    }
}
