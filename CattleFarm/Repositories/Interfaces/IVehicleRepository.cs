using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {
        Task<IEnumerable<Vehicle>> GetAvailableAsync();
        Task<IEnumerable<Vehicle>> GetByStatusAsync(VehicleStatus status);
        Task<Vehicle?> GetWithDriverAsync(int vehicleId);
        Task<IEnumerable<Vehicle>> GetAllWithDriverAsync();
        Task<(IEnumerable<Vehicle> Items, int Total)> GetPagedAsync(int page, int pageSize, VehicleStatus? status = null, string? search = null);
    }
}
