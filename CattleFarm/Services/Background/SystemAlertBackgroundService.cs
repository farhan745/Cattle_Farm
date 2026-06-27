using CattleFarm.Services.Interfaces;

namespace CattleFarm.Services.Background
{
    /// <summary>
    /// Periodically generates system notifications such as overdue vaccinations and expiring subscriptions.
    /// </summary>
    public class SystemAlertBackgroundService : BackgroundService
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan Interval = TimeSpan.FromHours(12);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SystemAlertBackgroundService> _logger;

        public SystemAlertBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<SystemAlertBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("System alert background service started.");

            try
            {
                await Task.Delay(InitialDelay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notifications = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notifications.CheckAndSendSystemAlertsAsync();
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while generating system alerts");
                }

                try
                {
                    await Task.Delay(Interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }
    }
}
