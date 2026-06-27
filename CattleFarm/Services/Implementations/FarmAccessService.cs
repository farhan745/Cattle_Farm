using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Services.Implementations
{
    public class FarmAccessService : IFarmAccessService
    {
        private readonly CattleFarmDbContext _db;

        public FarmAccessService(CattleFarmDbContext db) => _db = db;

        public Task<bool> IsFarmOwnerAsync(int farmId, int userId)
            => _db.Farms.AnyAsync(f => f.Id == farmId && f.OwnerId == userId && !f.IsDeleted);

        public Task<bool> IsAssignedManagerAsync(int farmId, int userId)
            => _db.FarmManagers.AnyAsync(m =>
                m.FarmId == farmId && m.ManagerUserId == userId && m.IsActive && !m.IsDeleted);

        public async Task<bool> CanOperateFarmAsync(int farmId, int userId, string? role)
        {
            if (role == AppRoles.Admin) return true;
            if (role == AppRoles.Owner) return await IsFarmOwnerAsync(farmId, userId);
            if (role == AppRoles.Manager) return await IsAssignedManagerAsync(farmId, userId);
            return false;
        }

        /// <summary>Owner-only farm actions (create farm, edit farm profile, join approvals).</summary>
        public async Task<bool> CanOwnFarmEntityAsync(int farmId, int userId, string? role)
        {
            if (role == AppRoles.Admin) return true;
            if (role == AppRoles.Owner) return await IsFarmOwnerAsync(farmId, userId);
            return false;
        }

        public async Task<int?> GetActiveManagerFarmIdAsync(int userId)
        {
            var membership = await _db.FarmManagers
                .Where(m => m.ManagerUserId == userId && m.IsActive && !m.IsDeleted)
                .Select(m => (int?)m.FarmId)
                .FirstOrDefaultAsync();
            return membership;
        }

        public async Task<IReadOnlyList<int>> GetAccessibleFarmIdsAsync(int userId, string? role)
        {
            if (role == AppRoles.Admin)
                return await _db.Farms.Where(f => !f.IsDeleted).Select(f => f.Id).ToListAsync();

            if (role == AppRoles.Owner)
                return await _db.Farms.Where(f => f.OwnerId == userId && !f.IsDeleted).Select(f => f.Id).ToListAsync();

            if (role == AppRoles.Manager)
            {
                var farmId = await GetActiveManagerFarmIdAsync(userId);
                return farmId.HasValue ? new List<int> { farmId.Value } : Array.Empty<int>();
            }

            return Array.Empty<int>();
        }

        public async Task<IEnumerable<Farm>> GetAccessibleFarmsAsync(int userId, string? role)
        {
            var ids = await GetAccessibleFarmIdsAsync(userId, role);
            if (ids.Count == 0) return Enumerable.Empty<Farm>();
            return await _db.Farms
                .Where(f => ids.Contains(f.Id) && !f.IsDeleted)
                .Include(f => f.Owner)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
