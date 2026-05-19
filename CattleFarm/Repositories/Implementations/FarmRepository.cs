using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class FarmRepository : Repository<Farm>, IFarmRepository
    {
        public FarmRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Farm>> GetByOwnerIdAsync(int ownerId)
            => await _dbSet.Where(f => f.OwnerId == ownerId).Include(f => f.Owner).ToListAsync();

        public async Task<IEnumerable<Farm>> GetApprovedFarmsAsync()
            => await _dbSet.Where(f => f.ApprovalStatus == ApprovalStatus.Approved && f.IsActive).ToListAsync();

        public async Task<IEnumerable<Farm>> SearchAsync(string keyword)
            => await _dbSet.Where(f => f.Name.Contains(keyword) || f.Location.Contains(keyword))
                           .Include(f => f.Owner).ToListAsync();

        public async Task<Farm?> GetWithDetailsAsync(int id)
            => await _dbSet.Where(f => f.Id == id)
                           .Include(f => f.Owner)
                           .Include(f => f.Cattles)
                           .Include(f => f.Workers)
                           .Include(f => f.Doctors)
                           .Include(f => f.Products)
                           .FirstOrDefaultAsync();

        public async Task<int> CountAsync() => await _dbSet.CountAsync();

        public async Task<IEnumerable<Farm>> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var q = _dbSet.Include(f => f.Owner).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(f => f.Name.Contains(search) || f.Location.Contains(search));
            return await q.OrderByDescending(f => f.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
    }
}
