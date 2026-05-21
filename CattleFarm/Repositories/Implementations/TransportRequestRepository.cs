using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class TransportRequestRepository : Repository<TransportRequest>, ITransportRequestRepository
    {
        public TransportRequestRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<TransportRequest>> GetByStatusAsync(TripStatus status)
            => await _dbSet.Where(r => r.Status == status)
                           .Include(r => r.RequestedByUser)
                           .Include(r => r.Farm)
                           .OrderByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<IEnumerable<TransportRequest>> GetByFarmIdAsync(int farmId)
            => await _dbSet.Where(r => r.FarmId == farmId)
                           .Include(r => r.RequestedByUser)
                           .Include(r => r.Trip)
                           .OrderByDescending(r => r.CreatedAt).ToListAsync();

        public async Task<TransportRequest?> GetWithDetailsAsync(int id)
            => await _dbSet.Where(r => r.Id == id)
                           .Include(r => r.RequestedByUser)
                           .Include(r => r.Farm)
                           .Include(r => r.Order)
                           .Include(r => r.Trip)
                               .ThenInclude(t => t != null ? t.Vehicle : null)
                           .Include(r => r.Trip)
                               .ThenInclude(t => t != null ? t.Driver : null)
                           .FirstOrDefaultAsync();

        public async Task<(IEnumerable<TransportRequest> Items, int Total)> GetPagedAsync(
            int page, int pageSize, TripStatus? status = null,
            TransportType? type = null, int? farmId = null, string? search = null)
        {
            var q = _dbSet.Include(r => r.RequestedByUser)
                          .Include(r => r.Farm)
                          .Include(r => r.Trip).AsQueryable();

            if (status.HasValue)  q = q.Where(r => r.Status == status.Value);
            if (type.HasValue)    q = q.Where(r => r.RequestType == type.Value);
            if (farmId.HasValue)  q = q.Where(r => r.FarmId == farmId.Value);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(r => r.PickupLocation.Contains(search) ||
                                 r.Destination.Contains(search) ||
                                 (r.CargoDescription != null && r.CargoDescription.Contains(search)));

            int total = await q.CountAsync();
            var items = await q.OrderByDescending(r => r.CreatedAt)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<IEnumerable<TransportRequest>> GetPendingAsync()
            => await _dbSet.Where(r => r.Status == TripStatus.Pending)
                           .Include(r => r.RequestedByUser)
                           .OrderBy(r => r.ScheduledDate).ToListAsync();

        public async Task<int> CountByStatusAsync(TripStatus status)
            => await _dbSet.CountAsync(r => r.Status == status);
    }
}
