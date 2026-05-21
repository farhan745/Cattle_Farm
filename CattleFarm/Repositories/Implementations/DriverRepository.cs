using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class DriverRepository : Repository<Driver>, IDriverRepository
    {
        public DriverRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Driver>> GetAvailableAsync()
            => await _dbSet.Where(d => d.Status == DriverStatus.Available)
                           .Include(d => d.AssignedVehicle)
                           .OrderBy(d => d.FullName).ToListAsync();

        public async Task<IEnumerable<Driver>> GetByStatusAsync(DriverStatus status)
            => await _dbSet.Where(d => d.Status == status)
                           .Include(d => d.AssignedVehicle)
                           .OrderBy(d => d.FullName).ToListAsync();

        public async Task<Driver?> GetWithVehicleAsync(int driverId)
            => await _dbSet.Where(d => d.Id == driverId)
                           .Include(d => d.AssignedVehicle)
                           .FirstOrDefaultAsync();

        public async Task<IEnumerable<Driver>> GetAllWithVehicleAsync()
            => await _dbSet.Include(d => d.AssignedVehicle)
                           .OrderBy(d => d.FullName).ToListAsync();

        public async Task<(IEnumerable<Driver> Items, int Total)> GetPagedAsync(
            int page, int pageSize, DriverStatus? status = null, string? search = null)
        {
            var q = _dbSet.Include(d => d.AssignedVehicle).AsQueryable();
            if (status.HasValue) q = q.Where(d => d.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(d => d.FullName.Contains(search) || d.Phone.Contains(search)
                               || d.LicenseNumber.Contains(search));
            int total = await q.CountAsync();
            var items = await q.OrderBy(d => d.FullName)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
