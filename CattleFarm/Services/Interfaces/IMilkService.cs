using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IMilkService
    {
        Task<IEnumerable<MilkProduction>> GetByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<MilkProduction>> GetByCattleAsync(int cattleId, DateTime? from = null, DateTime? to = null);
        Task<MilkProduction?> GetByIdAsync(int id);
        Task<MilkProduction>  CreateAsync(MilkProductionViewModel vm);
        Task<bool>            UpdateAsync(int id, MilkProductionViewModel vm);
        Task<bool>            DeleteAsync(int id);
        Task<double>          GetTotalYieldByFarmAsync(int farmId, DateTime? from, DateTime? to);
    }
}
