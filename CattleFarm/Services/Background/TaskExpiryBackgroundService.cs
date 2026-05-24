using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CattleFarm.Models;

namespace CattleFarm.Services.Background
{
    /// <summary>
    /// Runs every 10 minutes.
    /// Marks Open/Accepted tasks as Expired once their ExpiresAt passes.
    /// </summary>
    public class TaskExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TaskExpiryBackgroundService> _logger;
        private static readonly TimeSpan _interval = TimeSpan.FromMinutes(10);

        public TaskExpiryBackgroundService(IServiceScopeFactory scopeFactory,
                                           ILogger<TaskExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task expiry background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExpireTasksAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in task expiry job");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ExpireTasksAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CattleFarmDbContext>();

            var now = DateTime.UtcNow;

            var expirableStatuses = new[]
            {
                Models.TaskStatus.Open,
                Models.TaskStatus.Accepted,
                Models.TaskStatus.InProgress
            };

            var toExpire = await db.TaskAssignments
                .Where(t => !t.IsDeleted
                         && t.ExpiresAt.HasValue
                         && t.ExpiresAt.Value <= now
                         && expirableStatuses.Contains(t.Status))
                .ToListAsync();

            if (toExpire.Count == 0) return;

            foreach (var task in toExpire)
            {
                task.Status    = Models.TaskStatus.Expired;
                task.UpdatedAt = now;
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("Expired {Count} tasks at {Time}", toExpire.Count, now);
        }
    }
}
