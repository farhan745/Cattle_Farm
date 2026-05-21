using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IDriverRepository : IRepository<Driver>
    {
        Task<IEnumerable<Driver>> GetAvailableAsync();
        Task<IEnumerable<Driver>> GetByStatusAsync(DriverStatus status);
        Task<Driver?> GetWithVehicleAsync(int driverId);
        Task<IEnumerable<Driver>> GetAllWithVehicleAsync();
        Task<(IEnumerable<Driver> Items, int Total)> GetPagedAsync(int page, int pageSize, DriverStatus? status = null, string? search = null);
    }
}
