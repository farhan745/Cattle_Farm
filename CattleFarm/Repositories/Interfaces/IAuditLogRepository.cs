using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, int entityId);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(int userId);
        Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(int page, int pageSize, string? entity = null, int? userId = null);
    }
}
