using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Repositories.Interfaces
{
    public interface ITripRepository : IRepository<Trip>
    {
        Task<IEnumerable<Trip>> GetByStatusAsync(TripStatus status);
        Task<IEnumerable<Trip>> GetByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<Trip>> GetByDriverIdAsync(int driverId);
        Task<Trip?> GetWithDetailsAsync(int id);
        Task<(IEnumerable<Trip> Items, int Total)> GetPagedAsync(
            int page, int pageSize, TripStatus? status = null,
            int? vehicleId = null, int? driverId = null);
        Task<IEnumerable<Trip>> GetOngoingAsync();
        Task<int> CountByStatusAsync(TripStatus status);
        Task<decimal> GetTotalCostAsync(DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<MonthlyTransportStat>> GetMonthlyStatsAsync(int months = 6);
    }
}
