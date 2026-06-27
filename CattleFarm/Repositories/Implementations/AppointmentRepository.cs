using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(CattleFarmDbContext context) : base(context) { }

        public override async Task<Appointment?> GetByIdAsync(int id)
            => await _dbSet
                .Include(a => a.Cattle)
                .Include(a => a.Doctor)
                .Include(a => a.Farm)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Appointment>> GetByFarmIdAsync(int farmId)
            => await _dbSet.Where(a => a.FarmId == farmId)
                           .Include(a => a.Cattle).Include(a => a.Doctor)
                           .OrderByDescending(a => a.ScheduledAt).ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
            => await _dbSet.Where(a => a.DoctorId == doctorId)
                           .Include(a => a.Cattle).Include(a => a.Farm)
                           .OrderBy(a => a.ScheduledAt).ToListAsync();

        public async Task<IEnumerable<Appointment>> GetByCattleIdAsync(int cattleId)
            => await _dbSet.Where(a => a.CattleId == cattleId)
                           .Include(a => a.Doctor)
                           .OrderByDescending(a => a.ScheduledAt).ToListAsync();

        public async Task<IEnumerable<Appointment>> GetUpcomingAsync(int farmId, int daysAhead = 7)
        {
            var cutoff = DateTime.UtcNow.AddDays(daysAhead);
            return await _dbSet.Where(a => a.FarmId == farmId && a.ScheduledAt >= DateTime.UtcNow && a.ScheduledAt <= cutoff
                               && (a.Status == AppointmentStatus.Accepted || a.Status == AppointmentStatus.Pending))
                               .Include(a => a.Cattle).Include(a => a.Doctor)
                               .OrderBy(a => a.ScheduledAt).ToListAsync();
        }

        public async Task<(IEnumerable<Appointment> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            int? farmId = null,
            AppointmentStatus? status = null,
            int? doctorId = null,
            IReadOnlyCollection<int>? farmIds = null)
        {
            var q = _dbSet.Include(a => a.Cattle).Include(a => a.Doctor).Include(a => a.Farm).AsQueryable();
            if (farmId.HasValue) q = q.Where(a => a.FarmId == farmId.Value);
            if (doctorId.HasValue) q = q.Where(a => a.DoctorId == doctorId.Value);
            if (farmIds is { Count: > 0 }) q = q.Where(a => farmIds.Contains(a.FarmId));
            if (status.HasValue) q = q.Where(a => a.Status == status.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(a => a.ScheduledAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
