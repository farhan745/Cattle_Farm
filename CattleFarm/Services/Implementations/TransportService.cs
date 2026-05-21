using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class TransportService : ITransportService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notif;

        public TransportService(IUnitOfWork uow, INotificationService notif)
        {
            _uow   = uow;
            _notif = notif;
        }

        // ── Dashboard ──────────────────────────────────────────────────────────
        public async Task<TransportDashboardViewModel> GetDashboardAsync()
        {
            var vehicles = (await _uow.Vehicles.GetAllWithDriverAsync()).ToList();
            var drivers  = (await _uow.Drivers.GetAllWithVehicleAsync()).ToList();

            var ongoingTrips    = (await _uow.Trips.GetByStatusAsync(TripStatus.Ongoing)).ToList();
            var pendingRequests = (await _uow.TransportRequests.GetByStatusAsync(TripStatus.Pending)).ToList();
            var completedTrips  = (await _uow.Trips.GetByStatusAsync(TripStatus.Completed)).ToList();

            var totalCostThisMonth = await _uow.Trips.GetTotalCostAsync(
                new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), DateTime.UtcNow);

            var monthlyStats = (await _uow.Trips.GetMonthlyStatsAsync(6)).ToList();

            return new TransportDashboardViewModel
            {
                TotalVehicles      = vehicles.Count,
                AvailableVehicles  = vehicles.Count(v => v.Status == VehicleStatus.Available),
                OnTripVehicles     = vehicles.Count(v => v.Status == VehicleStatus.OnTrip),
                MaintenanceVehicles= vehicles.Count(v => v.Status == VehicleStatus.Maintenance),
                TotalDrivers       = drivers.Count,
                AvailableDrivers   = drivers.Count(d => d.Status == DriverStatus.Available),
                OnTripDrivers      = drivers.Count(d => d.Status == DriverStatus.OnTrip),
                PendingRequests    = pendingRequests.Count,
                OngoingTrips       = ongoingTrips.Count,
                CompletedTripsTotal= completedTrips.Count,
                TotalCostThisMonth = totalCostThisMonth,
                TotalCostAllTime   = completedTrips.Sum(t => t.TotalCost),
                RecentTrips        = completedTrips.OrderByDescending(t => t.EndTime).Take(5).ToList(),
                OngoingTripList    = ongoingTrips,
                MonthlyStats       = monthlyStats,
                VehicleStatusBreakdown = new Dictionary<string, int>
                {
                    ["Available"]   = vehicles.Count(v => v.Status == VehicleStatus.Available),
                    ["On Trip"]     = vehicles.Count(v => v.Status == VehicleStatus.OnTrip),
                    ["Maintenance"] = vehicles.Count(v => v.Status == VehicleStatus.Maintenance),
                    ["Retired"]     = vehicles.Count(v => v.Status == VehicleStatus.Retired),
                }
            };
        }

        // ── Vehicles ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<Vehicle>> GetAllVehiclesAsync()
            => await _uow.Vehicles.GetAllWithDriverAsync();

        public async Task<(IEnumerable<Vehicle> Items, int Total)> GetVehiclesPagedAsync(
            int page, int pageSize, VehicleStatus? status = null, string? search = null)
            => await _uow.Vehicles.GetPagedAsync(page, pageSize, status, search);

        public async Task<Vehicle?> GetVehicleByIdAsync(int id)
            => await _uow.Vehicles.GetWithDriverAsync(id);

        public async Task<Vehicle> CreateVehicleAsync(VehicleViewModel vm)
        {
            var vehicle = new Vehicle
            {
                Name               = vm.Name,
                Type               = vm.Type,
                RegistrationNumber = vm.RegistrationNumber,
                Capacity           = vm.Capacity,
                CapacityUnit       = vm.CapacityUnit ?? "tonnes",
                FuelType           = vm.FuelType,
                FuelCostPerKm      = vm.FuelCostPerKm,
                Status             = VehicleStatus.Available,
                Notes              = vm.Notes,
                CreatedAt          = DateTime.UtcNow,
                UpdatedAt          = DateTime.UtcNow
            };
            await _uow.Vehicles.AddAsync(vehicle);
            await _uow.SaveChangesAsync();
            return vehicle;
        }

        public async Task<Vehicle?> UpdateVehicleAsync(int id, VehicleViewModel vm)
        {
            var vehicle = await _uow.Vehicles.GetByIdAsync(id);
            if (vehicle is null) return null;

            vehicle.Name               = vm.Name;
            vehicle.Type               = vm.Type;
            vehicle.RegistrationNumber = vm.RegistrationNumber;
            vehicle.Capacity           = vm.Capacity;
            vehicle.CapacityUnit       = vm.CapacityUnit ?? "tonnes";
            vehicle.FuelType           = vm.FuelType;
            vehicle.FuelCostPerKm      = vm.FuelCostPerKm;
            vehicle.Status             = vm.Status;
            vehicle.Notes              = vm.Notes;
            vehicle.UpdatedAt          = DateTime.UtcNow;

            _uow.Vehicles.Update(vehicle);
            await _uow.SaveChangesAsync();
            return vehicle;
        }

        public async Task<bool> DeleteVehicleAsync(int id)
        {
            var vehicle = await _uow.Vehicles.GetByIdAsync(id);
            if (vehicle is null) return false;
            vehicle.IsDeleted  = true;
            vehicle.UpdatedAt  = DateTime.UtcNow;
            _uow.Vehicles.Update(vehicle);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
            => await _uow.Vehicles.GetAvailableAsync();

        // ── Drivers ────────────────────────────────────────────────────────────
        public async Task<IEnumerable<Driver>> GetAllDriversAsync()
            => await _uow.Drivers.GetAllWithVehicleAsync();

        public async Task<(IEnumerable<Driver> Items, int Total)> GetDriversPagedAsync(
            int page, int pageSize, DriverStatus? status = null, string? search = null)
            => await _uow.Drivers.GetPagedAsync(page, pageSize, status, search);

        public async Task<Driver?> GetDriverByIdAsync(int id)
            => await _uow.Drivers.GetWithVehicleAsync(id);

        public async Task<Driver> CreateDriverAsync(DriverViewModel vm)
        {
            var driver = new Driver
            {
                FullName        = vm.FullName,
                Phone           = vm.Phone,
                LicenseNumber   = vm.LicenseNumber,
                LicenseType     = vm.LicenseType,
                ExperienceYears = vm.ExperienceYears,
                Address         = vm.Address,
                Status          = DriverStatus.Available,
                Notes           = vm.Notes,
                CreatedAt       = DateTime.UtcNow,
                UpdatedAt       = DateTime.UtcNow
            };
            await _uow.Drivers.AddAsync(driver);
            await _uow.SaveChangesAsync();
            return driver;
        }

        public async Task<Driver?> UpdateDriverAsync(int id, DriverViewModel vm)
        {
            var driver = await _uow.Drivers.GetByIdAsync(id);
            if (driver is null) return null;

            driver.FullName        = vm.FullName;
            driver.Phone           = vm.Phone;
            driver.LicenseNumber   = vm.LicenseNumber;
            driver.LicenseType     = vm.LicenseType;
            driver.ExperienceYears = vm.ExperienceYears;
            driver.Address         = vm.Address;
            driver.Status          = vm.Status;
            driver.Notes           = vm.Notes;
            driver.UpdatedAt       = DateTime.UtcNow;

            _uow.Drivers.Update(driver);
            await _uow.SaveChangesAsync();
            return driver;
        }

        public async Task<bool> DeleteDriverAsync(int id)
        {
            var driver = await _uow.Drivers.GetByIdAsync(id);
            if (driver is null) return false;
            driver.IsDeleted = true;
            driver.UpdatedAt = DateTime.UtcNow;
            _uow.Drivers.Update(driver);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Driver>> GetAvailableDriversAsync()
            => await _uow.Drivers.GetAvailableAsync();

        public async Task<bool> AssignDriverToVehicleAsync(int driverId, int vehicleId)
        {
            var driver  = await _uow.Drivers.GetByIdAsync(driverId);
            var vehicle = await _uow.Vehicles.GetByIdAsync(vehicleId);
            if (driver is null || vehicle is null) return false;

            driver.AssignedVehicleId = vehicleId;
            driver.UpdatedAt         = DateTime.UtcNow;
            vehicle.UpdatedAt        = DateTime.UtcNow;
            _uow.Drivers.Update(driver);
            _uow.Vehicles.Update(vehicle);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnassignDriverAsync(int driverId)
        {
            var driver = await _uow.Drivers.GetByIdAsync(driverId);
            if (driver is null) return false;
            driver.AssignedVehicleId = null;
            driver.UpdatedAt         = DateTime.UtcNow;
            _uow.Drivers.Update(driver);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ── Transport Requests ─────────────────────────────────────────────────
        public async Task<(IEnumerable<TransportRequest> Items, int Total)> GetRequestsPagedAsync(
            int page, int pageSize, TripStatus? status = null,
            TransportType? type = null, int? farmId = null, string? search = null)
            => await _uow.TransportRequests.GetPagedAsync(page, pageSize, status, type, farmId, search);

        public async Task<TransportRequest?> GetRequestByIdAsync(int id)
            => await _uow.TransportRequests.GetWithDetailsAsync(id);

        public async Task<TransportRequest> CreateRequestAsync(TransportRequestViewModel vm, int requestedByUserId)
        {
            var req = new TransportRequest
            {
                RequestType           = vm.RequestType,
                PickupLocation        = vm.PickupLocation,
                Destination           = vm.Destination,
                ScheduledDate         = vm.ScheduledDate,
                EstimatedDistanceKm   = vm.EstimatedDistanceKm,
                CargoWeight           = vm.CargoWeight,
                CargoDescription      = vm.CargoDescription,
                Notes                 = vm.Notes,
                FarmId                = vm.FarmId,
                OrderId               = vm.OrderId,
                Status                = TripStatus.Pending,
                RequestedByUserId     = requestedByUserId,
                CreatedAt             = DateTime.UtcNow,
                UpdatedAt             = DateTime.UtcNow
            };
            await _uow.TransportRequests.AddAsync(req);
            await _uow.SaveChangesAsync();
            return req;
        }

        public async Task<bool> ApproveRequestAsync(int requestId)
        {
            var req = await _uow.TransportRequests.GetByIdAsync(requestId);
            if (req is null) return false;
            req.Status    = TripStatus.Approved;
            req.UpdatedAt = DateTime.UtcNow;
            _uow.TransportRequests.Update(req);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelRequestAsync(int requestId)
        {
            var req = await _uow.TransportRequests.GetByIdAsync(requestId);
            if (req is null) return false;
            req.Status    = TripStatus.Cancelled;
            req.UpdatedAt = DateTime.UtcNow;
            _uow.TransportRequests.Update(req);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ── Trips ──────────────────────────────────────────────────────────────
        public async Task<(IEnumerable<Trip> Items, int Total)> GetTripsPagedAsync(
            int page, int pageSize, TripStatus? status = null)
            => await _uow.Trips.GetPagedAsync(page, pageSize, status);

        public async Task<Trip?> GetTripByIdAsync(int id)
            => await _uow.Trips.GetWithDetailsAsync(id);

        public async Task<Trip> AssignTripAsync(TripAssignViewModel vm)
        {
            var req     = await _uow.TransportRequests.GetByIdAsync(vm.TransportRequestId);
            var vehicle = await _uow.Vehicles.GetByIdAsync(vm.VehicleId);
            var driver  = await _uow.Drivers.GetByIdAsync(vm.DriverId);

            if (req is null || vehicle is null || driver is null)
                throw new InvalidOperationException("Invalid request, vehicle, or driver.");

            var cost = await CalculateCostAsync(vm.DistanceKm, vm.VehicleId);

            var trip = new Trip
            {
                TransportRequestId = vm.TransportRequestId,
                VehicleId          = vm.VehicleId,
                DriverId           = vm.DriverId,
                DistanceKm         = vm.DistanceKm,
                FuelCostPerKm      = vehicle.FuelCostPerKm,
                FuelCost           = vehicle.FuelCostPerKm * vm.DistanceKm,
                BaseCost           = cost,
                AdditionalCost     = 0,
                TotalCost          = cost,
                RouteNotes         = vm.RouteNotes,
                Notes              = vm.Notes,
                Status             = TripStatus.Assigned,
                CreatedAt          = DateTime.UtcNow,
                UpdatedAt          = DateTime.UtcNow
            };

            await _uow.Trips.AddAsync(trip);

            req.Status     = TripStatus.Assigned;
            req.UpdatedAt  = DateTime.UtcNow;
            vehicle.Status = VehicleStatus.Assigned;
            vehicle.UpdatedAt = DateTime.UtcNow;
            driver.Status  = DriverStatus.Available; // stays available until trip starts

            _uow.TransportRequests.Update(req);
            _uow.Vehicles.Update(vehicle);
            _uow.Drivers.Update(driver);

            await _uow.SaveChangesAsync();
            return trip;
        }

        public async Task<bool> StartTripAsync(int tripId)
        {
            var trip = await _uow.Trips.GetWithDetailsAsync(tripId);
            if (trip is null) return false;

            trip.Status    = TripStatus.Ongoing;
            trip.StartTime = DateTime.UtcNow;
            trip.UpdatedAt = DateTime.UtcNow;

            var vehicle = await _uow.Vehicles.GetByIdAsync(trip.VehicleId);
            var driver  = await _uow.Drivers.GetByIdAsync(trip.DriverId);
            if (vehicle != null) { vehicle.Status = VehicleStatus.OnTrip; _uow.Vehicles.Update(vehicle); }
            if (driver  != null) { driver.Status  = DriverStatus.OnTrip;  _uow.Drivers.Update(driver); }

            var req = await _uow.TransportRequests.GetByIdAsync(trip.TransportRequestId);
            if (req != null) { req.Status = TripStatus.Ongoing; _uow.TransportRequests.Update(req); }

            _uow.Trips.Update(trip);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteTripAsync(int tripId, decimal? actualDistance = null,
            decimal? additionalCost = null, string? additionalNote = null)
        {
            var trip = await _uow.Trips.GetWithDetailsAsync(tripId);
            if (trip is null) return false;

            if (actualDistance.HasValue)
            {
                trip.DistanceKm = actualDistance.Value;
                trip.FuelCost   = trip.FuelCostPerKm * actualDistance.Value;
            }

            trip.AdditionalCost    = additionalCost ?? 0;
            trip.AdditionalCostNote= additionalNote;
            trip.TotalCost         = trip.BaseCost + trip.FuelCost + trip.AdditionalCost;
            trip.Status            = TripStatus.Completed;
            trip.EndTime           = DateTime.UtcNow;
            trip.UpdatedAt         = DateTime.UtcNow;

            var vehicle = await _uow.Vehicles.GetByIdAsync(trip.VehicleId);
            var driver  = await _uow.Drivers.GetByIdAsync(trip.DriverId);
            if (vehicle != null) { vehicle.Status = VehicleStatus.Available; _uow.Vehicles.Update(vehicle); }
            if (driver  != null) { driver.Status  = DriverStatus.Available;  _uow.Drivers.Update(driver); }

            var req = await _uow.TransportRequests.GetByIdAsync(trip.TransportRequestId);
            if (req != null) { req.Status = TripStatus.Completed; _uow.TransportRequests.Update(req); }

            _uow.Trips.Update(trip);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelTripAsync(int tripId, string? reason = null)
        {
            var trip = await _uow.Trips.GetWithDetailsAsync(tripId);
            if (trip is null) return false;

            trip.Status    = TripStatus.Cancelled;
            trip.Notes     = string.IsNullOrEmpty(reason) ? trip.Notes : $"Cancelled: {reason}";
            trip.UpdatedAt = DateTime.UtcNow;

            var vehicle = await _uow.Vehicles.GetByIdAsync(trip.VehicleId);
            var driver  = await _uow.Drivers.GetByIdAsync(trip.DriverId);
            if (vehicle != null) { vehicle.Status = VehicleStatus.Available; _uow.Vehicles.Update(vehicle); }
            if (driver  != null) { driver.Status  = DriverStatus.Available;  _uow.Drivers.Update(driver); }

            var req = await _uow.TransportRequests.GetByIdAsync(trip.TransportRequestId);
            if (req != null) { req.Status = TripStatus.Cancelled; _uow.TransportRequests.Update(req); }

            _uow.Trips.Update(trip);
            await _uow.SaveChangesAsync();
            return true;
        }

        // ── Smart Suggestions ──────────────────────────────────────────────────
        public async Task<Vehicle?> SuggestVehicleAsync(decimal cargoWeight, TransportType type)
        {
            var available = (await _uow.Vehicles.GetAvailableAsync()).ToList();
            // Prefer vehicle with capacity >= cargo weight, sorted by closest fit
            return available
                .Where(v => v.Capacity >= cargoWeight)
                .OrderBy(v => v.Capacity - cargoWeight)
                .ThenBy(v => v.FuelCostPerKm)
                .FirstOrDefault()
                ?? available.OrderByDescending(v => v.Capacity).FirstOrDefault();
        }

        public async Task<Driver?> SuggestDriverAsync(int? vehicleId = null)
        {
            var available = (await _uow.Drivers.GetAvailableAsync()).ToList();
            if (vehicleId.HasValue)
            {
                // Prefer driver already assigned to this vehicle
                var assigned = available.FirstOrDefault(d => d.AssignedVehicleId == vehicleId);
                if (assigned != null) return assigned;
            }
            return available.OrderByDescending(d => d.Rating)
                            .ThenByDescending(d => d.ExperienceYears)
                            .FirstOrDefault();
        }

        public async Task<decimal> CalculateCostAsync(decimal distanceKm, int vehicleId)
        {
            var vehicle = await _uow.Vehicles.GetByIdAsync(vehicleId);
            if (vehicle is null) return distanceKm * 10m; // fallback rate
            return distanceKm * vehicle.FuelCostPerKm;
        }

        // ── Reports ────────────────────────────────────────────────────────────
        public async Task<TransportReportViewModel> GetReportAsync(DateTime from, DateTime to)
        {
            var (trips, _)    = await _uow.Trips.GetPagedAsync(1, 10000);
            var periodTrips   = trips.Where(t => t.CreatedAt >= from && t.CreatedAt <= to).ToList();
            var completedTrips= periodTrips.Where(t => t.Status == TripStatus.Completed).ToList();

            var vehicles = (await _uow.Vehicles.GetAllWithDriverAsync()).ToList();
            var drivers  = (await _uow.Drivers.GetAllWithVehicleAsync()).ToList();

            return new TransportReportViewModel
            {
                From              = from,
                To                = to,
                TotalTrips        = periodTrips.Count,
                CompletedTrips    = completedTrips.Count,
                CancelledTrips    = periodTrips.Count(t => t.Status == TripStatus.Cancelled),
                TotalDistanceKm   = completedTrips.Sum(t => t.DistanceKm),
                TotalCost         = completedTrips.Sum(t => t.TotalCost),
                AverageTripCost   = completedTrips.Count > 0 ? completedTrips.Average(t => t.TotalCost) : 0,
                AverageTripDistKm = completedTrips.Count > 0 ? completedTrips.Average(t => (double)t.DistanceKm) : 0,
                VehicleUsage = vehicles.Select(v => new VehicleUsageStat
                {
                    VehicleName   = v.Name,
                    Registration  = v.RegistrationNumber,
                    TripCount     = completedTrips.Count(t => t.VehicleId == v.Id),
                    TotalKm       = completedTrips.Where(t => t.VehicleId == v.Id).Sum(t => t.DistanceKm),
                    TotalCost     = completedTrips.Where(t => t.VehicleId == v.Id).Sum(t => t.TotalCost),
                }).ToList(),
                DriverPerformance = drivers.Select(d => new DriverPerformanceStat
                {
                    DriverName   = d.FullName,
                    TripCount    = completedTrips.Count(t => t.DriverId == d.Id),
                    TotalKm      = completedTrips.Where(t => t.DriverId == d.Id).Sum(t => t.DistanceKm),
                    Rating       = d.Rating,
                }).ToList(),
                MonthlyStats = (await _uow.Trips.GetMonthlyStatsAsync(12)).ToList()
            };
        }

        public async Task<IEnumerable<MonthlyTransportStat>> GetMonthlyStatsAsync(int months = 6)
            => await _uow.Trips.GetMonthlyStatsAsync(months);
    }
}
