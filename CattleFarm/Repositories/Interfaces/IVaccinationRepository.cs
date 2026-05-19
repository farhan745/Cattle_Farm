using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IVaccinationRepository : IRepository<Vaccination>
    {
        Task<IEnumerable<Vaccination>> GetByCattleIdAsync(int cattleId);
        Task<IEnumerable<Vaccination>> GetUpcomingAsync(int daysAhead = 30);
        Task<IEnumerable<Vaccination>> GetOverdueAsync();
    }
}
