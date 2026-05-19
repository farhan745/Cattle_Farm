using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IWorkerRepository : IRepository<Worker>
    {
        Task<IEnumerable<Worker>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Worker>> GetAvailableWorkersAsync(int farmId);
        Task<(IEnumerable<Worker> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null);
        Task<int> CountByFarmAsync(int farmId);
    }
}
