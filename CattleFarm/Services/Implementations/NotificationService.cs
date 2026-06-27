using CattleFarm.Hubs;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IHubContext<FarmDashboardHub> _hub;
        private readonly CattleFarmDbContext _db;
        private readonly ISmsService _sms;

        public NotificationService(IUnitOfWork uow, IHubContext<FarmDashboardHub> hub, CattleFarmDbContext db, ISmsService sms)
        {
            _uow = uow;
            _hub = hub;
            _db = db;
            _sms = sms;
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
            if (await _uow.Notifications.HasUnreadAsync(userId, type, entityType, entityId))
                return;

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

            // Send SMS alert via stub SMS service
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user != null && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await _sms.SendSmsAsync(user.PhoneNumber, $"{title}: {message}");
            }
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
            var now = DateTime.UtcNow;

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

            // Sick or critical cattle follow-up reminders
            var sickCattle = await _db.Cattles
                .Include(c => c.Farm)
                .Where(c => !c.IsDeleted
                    && c.Status == CattleStatus.Active
                    && (c.HealthStatus == HealthStatus.Sick || c.HealthStatus == HealthStatus.Critical)
                    && c.Farm != null
                    && !c.Farm.IsDeleted)
                .Take(100)
                .ToListAsync();
            foreach (var cattle in sickCattle)
            {
                if (cattle.Farm?.OwnerId is int ownerId)
                {
                    await SendAsync(
                        ownerId,
                        "Cattle Health Follow-up",
                        $"{cattle.Name} ({cattle.TagId}) is marked {cattle.HealthStatus}. Please review treatment or schedule a veterinarian visit.",
                        NotificationType.HealthAlert,
                        "Cattle",
                        cattle.Id);
                }
            }

            // Pending appointments scheduled soon
            var soon = now.AddDays(2);
            var pendingAppointments = await _db.Appointments
                .Include(a => a.Farm)
                .Include(a => a.Doctor)
                .Include(a => a.Cattle)
                .Where(a => a.Status == AppointmentStatus.Pending
                    && a.ScheduledAt >= now
                    && a.ScheduledAt <= soon)
                .Take(100)
                .ToListAsync();
            foreach (var appointment in pendingAppointments)
            {
                if (appointment.Farm?.OwnerId is int ownerId)
                {
                    await SendAsync(
                        ownerId,
                        "Appointment Pending",
                        $"Appointment for {appointment.Cattle?.Name ?? "cattle"} is still pending for {appointment.ScheduledAt:MMM dd, yyyy h:mm tt}.",
                        NotificationType.Appointment,
                        "Appointment",
                        appointment.Id);
                }

                if (appointment.Doctor?.UserId is int doctorUserId)
                {
                    await SendAsync(
                        doctorUserId,
                        "Appointment Needs Response",
                        $"A veterinarian appointment request is pending for {appointment.ScheduledAt:MMM dd, yyyy h:mm tt}.",
                        NotificationType.AppointmentRequested,
                        "Appointment",
                        appointment.Id);
                }
            }

            // Unpaid salary reminders for generated payrolls
            var payPeriodCutoff = now.AddDays(-7);
            var unpaidPayrolls = await _db.Payrolls
                .Include(p => p.Farm)
                .Include(p => p.Worker)
                .Where(p => !p.IsDeleted && !p.IsPaid && p.GeneratedAt <= payPeriodCutoff)
                .Take(100)
                .ToListAsync();
            foreach (var payroll in unpaidPayrolls)
            {
                if (payroll.Farm?.OwnerId is int ownerId)
                {
                    await SendAsync(
                        ownerId,
                        "Payroll Pending",
                        $"{payroll.Worker?.FullName ?? "A worker"} has an unpaid salary slip for {payroll.Month:D2}/{payroll.Year}.",
                        NotificationType.SalaryUpdate,
                        "Payroll",
                        payroll.Id);
                }

                if (payroll.UserId > 0)
                {
                    await SendAsync(
                        payroll.UserId,
                        "Salary Pending",
                        $"Your salary slip for {payroll.Month:D2}/{payroll.Year} is still pending payment.",
                        NotificationType.SalaryUpdate,
                        "Payroll",
                        payroll.Id);
                }
            }

            // Pending order/payment reminders
            var orderCutoff = now.AddDays(-1);
            var pendingOrders = await _db.Orders
                .Include(o => o.Farm)
                .Where(o => o.CreatedAt <= orderCutoff
                    && o.OrderStatus != OrderStatus.Delivered
                    && o.OrderStatus != OrderStatus.Cancelled
                    && (o.OrderStatus == OrderStatus.Pending || o.PaymentStatus == PaymentStatus.Pending || o.PaymentStatus == PaymentStatus.Failed))
                .Take(100)
                .ToListAsync();
            foreach (var order in pendingOrders)
            {
                if (order.Farm?.OwnerId is int ownerId)
                {
                    await SendAsync(
                        ownerId,
                        "Order Needs Attention",
                        $"Order #{order.Id} is {order.OrderStatus} with {order.PaymentStatus} payment status.",
                        NotificationType.OrderUpdate,
                        "Order",
                        order.Id);
                }

                await SendAsync(
                    order.CustomerId,
                    order.PaymentStatus == PaymentStatus.Failed ? "Payment Failed" : "Order Pending",
                    $"Order #{order.Id} is {order.OrderStatus}; payment status is {order.PaymentStatus}.",
                    order.PaymentStatus == PaymentStatus.Failed ? NotificationType.Payment : NotificationType.OrderUpdate,
                    "Order",
                    order.Id);
            }

            // Low feed stock alerts
            var lowStockFeeds = await _db.FeedInventories
                .Include(fi => fi.Farm)
                .Where(fi => fi.Farm != null
                    && !fi.Farm.IsDeleted
                    && fi.StockQuantityKg <= fi.MinStockThresholdKg)
                .Take(100)
                .ToListAsync();
            foreach (var fi in lowStockFeeds)
            {
                if (fi.Farm?.OwnerId is int ownerId)
                {
                    await SendAsync(
                        ownerId,
                        "Low Feed Stock",
                        $"{fi.FeedType} stock is low ({fi.StockQuantityKg:F1} kg remaining, threshold: {fi.MinStockThresholdKg:F1} kg) at {fi.Farm.Name}. Please restock soon.",
                        NotificationType.LowFeedStock,
                        "FeedInventory",
                        fi.Id);
                }
            }

            await _uow.SaveChangesAsync();
        }
    }
}
