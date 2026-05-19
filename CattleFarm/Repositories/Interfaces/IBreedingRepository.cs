using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IBreedingRepository : IRepository<Breeding>
    {
        Task<IEnumerable<Breeding>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Breeding>> GetByCattleIdAsync(int cattleId);
        Task<IEnumerable<Breeding>> GetPendingAsync(int farmId);
        Task<(IEnumerable<Breeding> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, BreedingOutcome? outcome = null);
    }
}
