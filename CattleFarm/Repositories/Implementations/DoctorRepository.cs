using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
            => await _dbSet.Where(d => !d.IsDeleted && d.IsActive && d.ApprovalStatus == ApprovalStatus.Approved && d.IsAvailable).ToListAsync();

        public async Task<IEnumerable<Doctor>> GetPendingApprovalAsync()
            => await _dbSet
                .Include(d => d.User)
                .Where(d => !d.IsDeleted && d.ApprovalStatus == ApprovalStatus.Pending)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

        public async Task<(IEnumerable<Doctor> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var q = _dbSet.Where(d => !d.IsDeleted && d.IsActive && d.ApprovalStatus == ApprovalStatus.Approved).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(d => d.FullName.Contains(search) || d.Specialization.Contains(search));
            int total = await q.CountAsync();
            var items = await q.OrderBy(d => d.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public override async Task<Doctor?> GetByIdAsync(int id)
            => await _dbSet.Include(d => d.User).FirstOrDefaultAsync(d => d.Id == id);

        public async Task<int> CountAsync() => await _dbSet.CountAsync();

        public async Task<Doctor?> GetByUserIdAsync(int userId)
            => await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);

        public async Task<Doctor?> GetByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLower();
            return await _dbSet.FirstOrDefaultAsync(d => d.Email != null && d.Email.ToLower() == normalized);
        }
    }
}
