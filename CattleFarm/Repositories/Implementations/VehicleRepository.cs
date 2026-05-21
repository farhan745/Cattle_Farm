using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Vehicle>> GetAvailableAsync()
            => await _dbSet.Where(v => v.Status == VehicleStatus.Available)
                           .Include(v => v.Driver)
                           .OrderBy(v => v.Name).ToListAsync();

        public async Task<IEnumerable<Vehicle>> GetByStatusAsync(VehicleStatus status)
            => await _dbSet.Where(v => v.Status == status)
                           .Include(v => v.Driver)
                           .OrderBy(v => v.Name).ToListAsync();

        public async Task<Vehicle?> GetWithDriverAsync(int vehicleId)
            => await _dbSet.Where(v => v.Id == vehicleId)
                           .Include(v => v.Driver)
                           .FirstOrDefaultAsync();

        public async Task<IEnumerable<Vehicle>> GetAllWithDriverAsync()
            => await _dbSet.Include(v => v.Driver)
                           .OrderBy(v => v.Name).ToListAsync();

        public async Task<(IEnumerable<Vehicle> Items, int Total)> GetPagedAsync(
            int page, int pageSize, VehicleStatus? status = null, string? search = null)
        {
            var q = _dbSet.Include(v => v.Driver).AsQueryable();
            if (status.HasValue)  q = q.Where(v => v.Status == status.Value);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(v => v.Name.Contains(search) || v.RegistrationNumber.Contains(search));
            int total = await q.CountAsync();
            var items = await q.OrderBy(v => v.Name)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
