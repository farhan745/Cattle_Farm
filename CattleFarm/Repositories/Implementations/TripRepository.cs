using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class TripRepository : Repository<Trip>, ITripRepository
    {
        public TripRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Trip>> GetByStatusAsync(TripStatus status)
            => await _dbSet.Where(t => t.Status == status)
                           .Include(t => t.Vehicle)
                           .Include(t => t.Driver)
                           .Include(t => t.TransportRequest)
                           .OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Trip>> GetByVehicleIdAsync(int vehicleId)
            => await _dbSet.Where(t => t.VehicleId == vehicleId)
                           .Include(t => t.Driver)
                           .Include(t => t.TransportRequest)
                           .OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Trip>> GetByDriverIdAsync(int driverId)
            => await _dbSet.Where(t => t.DriverId == driverId)
                           .Include(t => t.Vehicle)
                           .Include(t => t.TransportRequest)
                           .OrderByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<Trip?> GetWithDetailsAsync(int id)
            => await _dbSet.Where(t => t.Id == id)
                           .Include(t => t.Vehicle)
                           .Include(t => t.Driver)
                           .Include(t => t.TransportRequest)
                               .ThenInclude(r => r.RequestedByUser)
                           .Include(t => t.TransportRequest)
                               .ThenInclude(r => r.Order)
                           .Include(t => t.TransportRequest)
                               .ThenInclude(r => r.Farm)
                           .FirstOrDefaultAsync();

        public async Task<(IEnumerable<Trip> Items, int Total)> GetPagedAsync(
            int page, int pageSize, TripStatus? status = null,
            int? vehicleId = null, int? driverId = null)
        {
            var q = _dbSet.Include(t => t.Vehicle)
                          .Include(t => t.Driver)
                          .Include(t => t.TransportRequest).AsQueryable();

            if (status.HasValue)   q = q.Where(t => t.Status == status.Value);
            if (vehicleId.HasValue) q = q.Where(t => t.VehicleId == vehicleId.Value);
            if (driverId.HasValue)  q = q.Where(t => t.DriverId == driverId.Value);

            int total = await q.CountAsync();
            var items = await q.OrderByDescending(t => t.CreatedAt)
                               .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<IEnumerable<Trip>> GetOngoingAsync()
            => await _dbSet.Where(t => t.Status == TripStatus.Ongoing)
                           .Include(t => t.Vehicle)
                           .Include(t => t.Driver)
                           .Include(t => t.TransportRequest).ToListAsync();

        public async Task<int> CountByStatusAsync(TripStatus status)
            => await _dbSet.CountAsync(t => t.Status == status);

        public async Task<decimal> GetTotalCostAsync(DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(t => t.Status == TripStatus.Completed).AsQueryable();
            if (from.HasValue) q = q.Where(t => t.CreatedAt >= from.Value);
            if (to.HasValue)   q = q.Where(t => t.CreatedAt <= to.Value);
            return await q.SumAsync(t => t.TotalCost);
        }

        public async Task<IEnumerable<MonthlyTransportStat>> GetMonthlyStatsAsync(int months = 6)
        {
            var from = DateTime.UtcNow.AddMonths(-months + 1);
            var data = await _dbSet
                .Where(t => t.Status == TripStatus.Completed && t.CreatedAt >= from)
                .ToListAsync();

            return data
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new MonthlyTransportStat
                {
                    Month      = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    TripCount  = g.Count(),
                    TotalCost  = g.Sum(t => t.TotalCost),
                    TotalKm    = g.Sum(t => t.DistanceKm)
                })
                .OrderBy(s => s.Month)
                .ToList();
        }
    }
}
