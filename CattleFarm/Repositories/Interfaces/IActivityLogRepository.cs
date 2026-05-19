using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IActivityLogRepository : IRepository<ActivityLog>
    {
        Task<IEnumerable<ActivityLog>> GetRecentAsync(int count = 20);
        Task<IEnumerable<ActivityLog>> GetByUserIdAsync(int userId, int count = 50);
    }
}
