using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface ITransportService
    {
        // ── Dashboard Stats ───────────────────────────────────────────────────
        Task<TransportDashboardViewModel> GetDashboardAsync();

        // ── Vehicles ──────────────────────────────────────────────────────────
        Task<IEnumerable<Vehicle>> GetAllVehiclesAsync();
        Task<(IEnumerable<Vehicle> Items, int Total)> GetVehiclesPagedAsync(
            int page, int pageSize, VehicleStatus? status = null, string? search = null);
        Task<Vehicle?> GetVehicleByIdAsync(int id);
        Task<Vehicle> CreateVehicleAsync(VehicleViewModel vm);
        Task<Vehicle?> UpdateVehicleAsync(int id, VehicleViewModel vm);
        Task<bool> DeleteVehicleAsync(int id);
        Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync();

        // ── Drivers ───────────────────────────────────────────────────────────
        Task<IEnumerable<Driver>> GetAllDriversAsync();
        Task<(IEnumerable<Driver> Items, int Total)> GetDriversPagedAsync(
            int page, int pageSize, DriverStatus? status = null, string? search = null);
        Task<Driver?> GetDriverByIdAsync(int id);
        Task<Driver> CreateDriverAsync(DriverViewModel vm);
        Task<Driver?> UpdateDriverAsync(int id, DriverViewModel vm);
        Task<bool> DeleteDriverAsync(int id);
        Task<IEnumerable<Driver>> GetAvailableDriversAsync();
        Task<bool> AssignDriverToVehicleAsync(int driverId, int vehicleId);
        Task<bool> UnassignDriverAsync(int driverId);

        // ── Transport Requests ────────────────────────────────────────────────
        Task<(IEnumerable<TransportRequest> Items, int Total)> GetRequestsPagedAsync(
            int page, int pageSize, TripStatus? status = null,
            TransportType? type = null, int? farmId = null, string? search = null);
        Task<TransportRequest?> GetRequestByIdAsync(int id);
        Task<TransportRequest> CreateRequestAsync(TransportRequestViewModel vm, int requestedByUserId);
        Task<bool> ApproveRequestAsync(int requestId);
        Task<bool> CancelRequestAsync(int requestId);

        // ── Trips ─────────────────────────────────────────────────────────────
        Task<(IEnumerable<Trip> Items, int Total)> GetTripsPagedAsync(
            int page, int pageSize, TripStatus? status = null);
        Task<Trip?> GetTripByIdAsync(int id);
        Task<Trip> AssignTripAsync(TripAssignViewModel vm);
        Task<bool> StartTripAsync(int tripId);
        Task<bool> CompleteTripAsync(int tripId, decimal? actualDistance = null, decimal? additionalCost = null, string? additionalNote = null);
        Task<bool> CancelTripAsync(int tripId, string? reason = null);

        // ── Smart Suggestions ─────────────────────────────────────────────────
        Task<Vehicle?> SuggestVehicleAsync(decimal cargoWeight, TransportType type);
        Task<Driver?> SuggestDriverAsync(int? vehicleId = null);
        Task<decimal> CalculateCostAsync(decimal distanceKm, int vehicleId);

        // ── Reports ───────────────────────────────────────────────────────────
        Task<TransportReportViewModel> GetReportAsync(DateTime from, DateTime to);
        Task<IEnumerable<MonthlyTransportStat>> GetMonthlyStatsAsync(int months = 6);
    }
}
