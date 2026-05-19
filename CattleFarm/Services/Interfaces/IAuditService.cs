using CattleFarm.Models;

namespace CattleFarm.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(int? userId, string action, string entityName, int? entityId, string? oldValues = null, string? newValues = null, string? ip = null);
        Task LogActivityAsync(int? userId, string description, string? entityName = null, int? entityId = null, string? ip = null);
        Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 50);
        Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAuditLogsAsync(int page, int pageSize, string? entity = null, int? userId = null);
        Task<IEnumerable<ActivityLog>> GetRecentActivityAsync(int count = 20);
    }
}
