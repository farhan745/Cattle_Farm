using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IFarmService
    {
        Task<IEnumerable<Farm>> GetAllAsync();
        Task<IEnumerable<Farm>> GetByOwnerAsync(int ownerId);
        Task<Farm?>             GetByIdAsync(int id);
        Task<Farm?>             GetWithDetailsAsync(int id);
        Task<Farm>              CreateAsync(FarmViewModel vm, int ownerId);
        Task<bool>              UpdateAsync(int id, FarmViewModel vm);
        Task<bool>              DeleteAsync(int id);
        Task<bool>              ApproveAsync(int id);
        Task<bool>              RejectAsync(int id);
        Task<(IEnumerable<Farm> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null);
    }
}
