using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class DoctorInvitationRepository : Repository<DoctorInvitation>, IDoctorInvitationRepository
    {
        public DoctorInvitationRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<DoctorInvitation?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(di => di.Farm)
                .Include(di => di.CreatedByUser)
                .Include(di => di.RevokedByUser)
                .Include(di => di.Doctor)
                .FirstOrDefaultAsync(di => di.Token == token);
        }

        public async Task<IEnumerable<DoctorInvitation>> GetByCreatorAsync(int userId)
        {
            return await _dbSet
                .Include(di => di.Farm)
                .Include(di => di.CreatedByUser)
                .Where(di => di.CreatedByUserId == userId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<DoctorInvitation> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var q = _dbSet
                .Include(di => di.Farm)
                .Include(di => di.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                q = q.Where(di => di.DoctorName.Contains(search) || di.Email.Contains(search) || di.PhoneNumber.Contains(search));
            }

            int total = await q.CountAsync();
            var items = await q
                .OrderByDescending(di => di.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<bool> IsEmailAlreadyInvitedAsync(string email)
        {
            return await _dbSet.AnyAsync(di => di.Email == email && 
                (di.InvitationStatus == InvitationStatus.Pending || di.InvitationStatus == InvitationStatus.Accepted));
        }
    }
}
