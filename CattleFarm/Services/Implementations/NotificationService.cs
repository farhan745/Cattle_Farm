using CattleFarm.Hubs;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using Microsoft.AspNetCore.SignalR;

namespace CattleFarm.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHubContext<FarmDashboardHub> _hub;

        public NotificationService(IUnitOfWork uow, IHubContext<FarmDashboardHub> hub)
        {
            _uow = uow;
            _hub = hub;
        }

        public async Task<IEnumerable<Notification>> GetByUserAsync(int userId, bool unreadOnly = false)
            => await _uow.Notifications.GetByUserIdAsync(userId, unreadOnly);

        public async Task<int> GetUnreadCountAsync(int userId)
            => await _uow.Notifications.GetUnreadCountAsync(userId);

        public async Task MarkAllReadAsync(int userId)
        {
            await _uow.Notifications.MarkAllReadAsync(userId);
            await _uow.SaveChangesAsync();
            await _hub.Clients.Group(FarmDashboardHub.UserGroup(userId))
                .SendAsync("NotificationsRead", new { unreadCount = 0 });
        }

        public async Task MarkReadAsync(int notificationId)
        {
            var n = await _uow.Notifications.GetByIdAsync(notificationId);
            if (n is null) return;
            n.IsRead = true; n.ReadAt = DateTime.UtcNow;
            _uow.Notifications.Update(n);
            await _uow.SaveChangesAsync();
            await _hub.Clients.Group(FarmDashboardHub.UserGroup(n.UserId))
                .SendAsync("NotificationRead", new { notificationId });
        }

        public async Task SendAsync(int userId, string title, string message, NotificationType type, string? entityType = null, int? entityId = null)
        {
            var notification = new Notification
            {
                UserId = userId, Title = title, Message = message, Type = type,
                RelatedEntityType = entityType, RelatedEntityId = entityId
            };
            await _uow.Notifications.AddAsync(notification);
            await _uow.SaveChangesAsync();
            var unreadCount = await GetUnreadCountAsync(userId);
            await _hub.Clients.Group(FarmDashboardHub.UserGroup(userId))
                .SendAsync("NotificationReceived", new
                {
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    Type = notification.Type.ToString(),
                    notification.RelatedEntityType,
                    notification.RelatedEntityId,
                    notification.CreatedAt,
                    unreadCount
                });
        }

        public async Task SendToRoleAsync(string role, string title, string message, NotificationType type)
        {
            var users = await _uow.Users.GetByRoleAsync(role);
            foreach (var user in users)
                await _uow.Notifications.AddAsync(new Notification { UserId = user.Id, Title = title, Message = message, Type = type });
            await _uow.SaveChangesAsync();
        }

        public async Task CheckAndSendSystemAlertsAsync()
        {
            // Vaccination reminders
            var overdueVax = await _uow.Vaccinations.GetOverdueAsync();
            foreach (var v in overdueVax)
            {
                var cattle = await _uow.Cattles.GetByIdAsync(v.CattleId);
                if (cattle?.Farm?.OwnerId is int ownerId)
                    await SendAsync(ownerId, "Vaccination Overdue", $"{cattle.Name} is overdue for {v.VaccineName}", NotificationType.Vaccination, "Cattle", v.CattleId);
            }
            // Subscription expiry reminders
            var expiring = await _uow.Subscriptions.GetExpiringAsync(7);
            foreach (var sub in expiring)
                await SendAsync(sub.UserId, "Subscription Expiring", $"Your {sub.Plan} subscription expires on {sub.ExpiryDate:MMM dd, yyyy}", NotificationType.Subscription);
            await _uow.SaveChangesAsync();
        }
    }
}
