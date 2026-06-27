using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId, bool unreadOnly = false);
        Task<int>  GetUnreadCountAsync(int userId);
        Task       MarkAllReadAsync(int userId);
        Task<bool> HasUnreadAsync(int userId, NotificationType type, string? entityType, int? entityId);
    }
}
