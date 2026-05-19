using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IMilkProductionRepository : IRepository<MilkProduction>
    {
        Task<IEnumerable<MilkProduction>> GetByFarmIdAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<MilkProduction>> GetByCattleIdAsync(int cattleId, DateTime? from = null, DateTime? to = null);
        Task<double>  GetTotalYieldByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<double>  GetTotalYieldByCattleAsync(int cattleId, DateTime? from = null, DateTime? to = null);
    }
}
