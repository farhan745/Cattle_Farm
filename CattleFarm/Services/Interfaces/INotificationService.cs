using CattleFarm.Models;

namespace CattleFarm.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetByUserAsync(int userId, bool unreadOnly = false);
        Task<int>  GetUnreadCountAsync(int userId);
        Task       MarkAllReadAsync(int userId);
        Task       MarkReadAsync(int notificationId);
        Task       SendAsync(int userId, string title, string message, NotificationType type, string? entityType = null, int? entityId = null);
        Task       SendToRoleAsync(string role, string title, string message, NotificationType type);
        Task       CheckAndSendSystemAlertsAsync();
    }
}
