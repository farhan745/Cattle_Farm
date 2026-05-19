using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IVaccinationService
    {
        Task<IEnumerable<Vaccination>> GetByCattleAsync(int cattleId);
        Task<IEnumerable<Vaccination>> GetUpcomingAsync(int daysAhead = 30);
        Task<IEnumerable<Vaccination>> GetOverdueAsync();
        Task<Vaccination?> GetByIdAsync(int id);
        Task<Vaccination>  CreateAsync(VaccinationViewModel vm);
        Task<bool>         UpdateAsync(int id, VaccinationViewModel vm);
        Task<bool>         DeleteAsync(int id);
    }
}
