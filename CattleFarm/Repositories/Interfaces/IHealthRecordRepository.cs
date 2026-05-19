using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IHealthRecordRepository : IRepository<HealthRecord>
    {
        Task<IEnumerable<HealthRecord>> GetByCattleIdAsync(int cattleId);
        Task<HealthRecord?>             GetLatestByCattleIdAsync(int cattleId);
        Task<IEnumerable<HealthRecord>> GetHighRiskAsync(int farmId);
        Task<(IEnumerable<HealthRecord> Items, int Total)> GetPagedAsync(int page, int pageSize, int? cattleId = null, RiskLevel? riskLevel = null);
    }
}
