using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IHealthService
    {
        Task<IEnumerable<HealthRecord>> GetByCattleAsync(int cattleId);
        Task<IEnumerable<HealthRecord>> GetHighRiskAsync(int farmId);
        Task<HealthRecord?> GetByIdAsync(int id);
        Task<HealthRecord>  CreateAsync(HealthRecordViewModel vm);
        Task<bool>          UpdateAsync(int id, HealthRecordViewModel vm);
        Task<bool>          DeleteAsync(int id);
        Task<(IEnumerable<HealthRecord> Items, int Total)> GetPagedAsync(int page, int pageSize, int? cattleId = null, RiskLevel? risk = null);
        Task<RiskLevel>     CalculateRiskAsync(int cattleId);
    }
}
