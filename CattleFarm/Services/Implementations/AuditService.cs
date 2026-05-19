using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;

namespace CattleFarm.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _uow;
        public AuditService(IUnitOfWork uow) { _uow = uow; }

        public async Task LogAsync(int? userId, string action, string entityName, int? entityId, string? oldValues = null, string? newValues = null, string? ip = null)
        {
            await _uow.AuditLogs.AddAsync(new AuditLog
            {
                UserId = userId, Action = action, EntityName = entityName,
                EntityId = entityId, OldValues = oldValues, NewValues = newValues, IPAddress = ip
            });
            await _uow.SaveChangesAsync();
        }

        public async Task LogActivityAsync(int? userId, string description, string? entityName = null, int? entityId = null, string? ip = null)
        {
            await _uow.ActivityLogs.AddAsync(new ActivityLog
            {
                UserId = userId, Description = description, EntityName = entityName, EntityId = entityId, IPAddress = ip
            });
            await _uow.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 50)
        {
            var (items, _) = await _uow.AuditLogs.GetPagedAsync(1, count);
            return items;
        }

        public async Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAuditLogsAsync(int page, int pageSize, string? entity = null, int? userId = null)
            => await _uow.AuditLogs.GetPagedAsync(page, pageSize, entity, userId);

        public async Task<IEnumerable<ActivityLog>> GetRecentActivityAsync(int count = 20)
            => await _uow.ActivityLogs.GetRecentAsync(count);
    }
}
