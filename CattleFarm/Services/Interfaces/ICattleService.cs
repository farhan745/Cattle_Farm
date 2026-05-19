using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface ICattleService
    {
        Task<(IEnumerable<Cattle> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, int? farmId = null, CattleStatus? status = null);
        Task<IEnumerable<Cattle>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Cattle>> SearchAsync(string keyword);
        Task<IEnumerable<Cattle>> GetListedForSaleAsync();
        Task<Cattle?>             GetByIdAsync(int id);
        Task<Cattle?>             GetWithDetailsAsync(int id);
        Task<Cattle>              CreateAsync(CattleViewModel vm);
        Task<bool>                UpdateAsync(int id, CattleViewModel vm);
        Task<bool>                DeleteAsync(int id);
        Task<bool>                RestoreAsync(int id);
        Task                      UpdateHealthStatusAsync(int cattleId);
    }
}
