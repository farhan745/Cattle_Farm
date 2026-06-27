using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using CattleFarm.Hubs;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class SmartMonitoringController : Controller
    {
        private readonly CattleFarmDbContext _db;
        private readonly IFarmAccessService _farmAccess;
        private readonly IAuditService _audit;
        private readonly IHubContext<FarmDashboardHub> _hubContext;

        public SmartMonitoringController(
            CattleFarmDbContext db, 
            IFarmAccessService farmAccess, 
            IAuditService audit,
            IHubContext<FarmDashboardHub> hubContext)
        {
            _db = db;
            _farmAccess = farmAccess;
            _audit = audit;
            _hubContext = hubContext;
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Index(int? farmId = null)
        {
            var farms = (await GetAccessibleFarmsAsync()).ToList();
            var selectedFarm = farmId.HasValue
                ? farms.FirstOrDefault(f => f.Id == farmId.Value)
                : farms.FirstOrDefault();

            if (selectedFarm is null)
            {
                ViewBag.Farms = farms;
                return View(Array.Empty<SensorReading>());
            }

            var latestReadings = await _db.SensorReadings
                .Where(s => s.FarmId == selectedFarm.Id)
                .OrderByDescending(s => s.RecordedAt)
                .Take(30)
                .ToListAsync();

            ViewBag.Farms = farms;
            ViewBag.SelectedFarm = selectedFarm;
            ViewBag.LatestGps = await _db.GpsTrackerSnapshots
                .Include(g => g.Cattle)
                .Where(g => g.FarmId == selectedFarm.Id)
                .OrderByDescending(g => g.RecordedAt)
                .Take(10)
                .ToListAsync();
            ViewBag.FeedingCommands = await _db.AutomatedFeedingCommands
                .Where(c => c.FarmId == selectedFarm.Id)
                .OrderByDescending(c => c.ScheduledAt)
                .Take(10)
                .ToListAsync();
            ViewBag.MilkImports = await _db.MilkMachineImports
                .Include(m => m.Cattle)
                .Where(m => m.FarmId == selectedFarm.Id)
                .OrderByDescending(m => m.CollectedAt)
                .Take(10)
                .ToListAsync();
            ViewBag.PendingSync = await _db.OfflineSyncItems
                .CountAsync(s => s.FarmId == selectedFarm.Id && s.Status == OfflineSyncStatus.Pending);

            return View(latestReadings);
        }

        [HttpGet("/api/smart-monitoring/summary")]
        public async Task<IActionResult> Summary(int farmId)
        {
            if (!await CanAccessFarmAsync(farmId)) return Forbid();

            var today = DateTime.UtcNow.Date;
            var latestSensors = await _db.SensorReadings
                .Where(s => s.FarmId == farmId)
                .GroupBy(s => s.ReadingType)
                .Select(g => g.OrderByDescending(x => x.RecordedAt).First())
                .ToListAsync();

            var milkMachineLiters = await _db.MilkMachineImports
                .Where(m => m.FarmId == farmId && m.CollectedAt >= today)
                .SumAsync(m => m.YieldLiters);

            return Json(new
            {
                sensors = latestSensors.Select(s => new
                {
                    type = s.ReadingType.ToString(),
                    value = s.Value,
                    unit = s.Unit,
                    deviceId = s.DeviceId,
                    recordedAt = s.RecordedAt
                }),
                gpsDevices = await _db.GpsTrackerSnapshots
                    .Where(g => g.FarmId == farmId && g.RecordedAt >= today)
                    .Select(g => g.TrackerId)
                    .Distinct()
                    .CountAsync(),
                pendingFeedingCommands = await _db.AutomatedFeedingCommands
                    .CountAsync(c => c.FarmId == farmId && c.Status == FeedingCommandStatus.Pending),
                milkMachineLiters,
                offlineSyncPending = await _db.OfflineSyncItems
                    .CountAsync(s => s.FarmId == farmId && s.Status == OfflineSyncStatus.Pending)
            });
        }

        [HttpGet("/api/smart-monitoring/latest-readings")]
        public async Task<IActionResult> LatestReadings(int farmId)
        {
            if (!await CanAccessFarmAsync(farmId)) return Forbid();

            var readings = await _db.SensorReadings
                .Where(s => s.FarmId == farmId)
                .OrderByDescending(s => s.RecordedAt)
                .Take(30)
                .ToListAsync();

            return Json(readings);
        }

        [HttpPost("/api/smart-monitoring/sensor-readings")]
        public async Task<IActionResult> RecordSensorReading([FromBody] SensorReadingRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!await CanAccessFarmAsync(request.FarmId)) return Forbid();

            var reading = new SensorReading
            {
                FarmId = request.FarmId,
                DeviceId = request.DeviceId.Trim(),
                ReadingType = request.ReadingType,
                Value = request.Value,
                Unit = request.Unit.Trim(),
                BarnZone = request.BarnZone?.Trim(),
                RecordedAt = request.RecordedAt ?? DateTime.UtcNow
            };

            await _db.SensorReadings.AddAsync(reading);
            await _db.SaveChangesAsync();
            await _audit.LogActivityAsync(GetUserId(), $"Sensor reading received from {reading.DeviceId}", "SensorReading", reading.Id);

            // SignalR broadcast
            await _hubContext.Clients.Group(FarmDashboardHub.FarmGroup(request.FarmId))
                .SendAsync("ReceiveSensorReading", new
                {
                    reading.Id,
                    reading.DeviceId,
                    reading.ReadingType,
                    reading.Value,
                    reading.Unit,
                    reading.BarnZone,
                    reading.RecordedAt
                });

            return Created($"/api/smart-monitoring/sensor-readings/{reading.Id}", reading);
        }

        [HttpPost("/api/smart-monitoring/gps")]
        public async Task<IActionResult> RecordGpsSnapshot([FromBody] GpsSnapshotRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!await CanAccessFarmAsync(request.FarmId)) return Forbid();

            var snapshot = new GpsTrackerSnapshot
            {
                FarmId = request.FarmId,
                CattleId = request.CattleId,
                TrackerId = request.TrackerId.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                SpeedKph = request.SpeedKph,
                RecordedAt = request.RecordedAt ?? DateTime.UtcNow
            };

            await _db.GpsTrackerSnapshots.AddAsync(snapshot);
            await _db.SaveChangesAsync();

            // SignalR broadcast
            await _hubContext.Clients.Group(FarmDashboardHub.FarmGroup(request.FarmId))
                .SendAsync("ReceiveGpsSnapshot", new
                {
                    snapshot.Id,
                    snapshot.CattleId,
                    snapshot.TrackerId,
                    snapshot.Latitude,
                    snapshot.Longitude,
                    snapshot.SpeedKph,
                    snapshot.RecordedAt
                });

            return Created($"/api/smart-monitoring/gps/{snapshot.Id}", snapshot);
        }

        [HttpPost("/api/smart-monitoring/feeding-commands")]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> QueueFeedingCommand([FromBody] FeedingCommandRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!await CanAccessFarmAsync(request.FarmId)) return Forbid();

            var command = new AutomatedFeedingCommand
            {
                FarmId = request.FarmId,
                CattleId = request.CattleId,
                ControllerId = request.ControllerId.Trim(),
                FeedName = request.FeedName.Trim(),
                QuantityKg = request.QuantityKg,
                ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
                Notes = request.Notes?.Trim()
            };

            await _db.AutomatedFeedingCommands.AddAsync(command);
            await _db.SaveChangesAsync();
            await _audit.LogActivityAsync(GetUserId(), $"Queued feeder command for {command.QuantityKg:n1} kg {command.FeedName}", "AutomatedFeedingCommand", command.Id);

            // SignalR broadcast
            await _hubContext.Clients.Group(FarmDashboardHub.FarmGroup(request.FarmId))
                .SendAsync("ReceiveFeedingCommand", new
                {
                    command.Id,
                    command.CattleId,
                    command.ControllerId,
                    command.FeedName,
                    command.QuantityKg,
                    command.ScheduledAt,
                    command.Notes
                });

            return Created($"/api/smart-monitoring/feeding-commands/{command.Id}", command);
        }

        [HttpPost("/api/smart-monitoring/milk-machine-imports")]
        public async Task<IActionResult> ImportMilkMachineReading([FromBody] MilkMachineImportRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!await CanAccessFarmAsync(request.FarmId)) return Forbid();

            var import = new MilkMachineImport
            {
                FarmId = request.FarmId,
                CattleId = request.CattleId,
                MachineId = request.MachineId.Trim(),
                YieldLiters = request.YieldLiters,
                FatPercentage = request.FatPercentage,
                ProteinPercentage = request.ProteinPercentage,
                CollectedAt = request.CollectedAt ?? DateTime.UtcNow
            };

            await _db.MilkMachineImports.AddAsync(import);
            await _db.SaveChangesAsync();

            // SignalR broadcast
            await _hubContext.Clients.Group(FarmDashboardHub.FarmGroup(request.FarmId))
                .SendAsync("ReceiveMilkMachineReading", new
                {
                    import.Id,
                    import.CattleId,
                    import.MachineId,
                    import.YieldLiters,
                    import.FatPercentage,
                    import.ProteinPercentage,
                    import.CollectedAt
                });

            return Created($"/api/smart-monitoring/milk-machine-imports/{import.Id}", import);
        }

        [HttpPost("/api/offline-sync")]
        public async Task<IActionResult> QueueOfflineSync([FromBody] OfflineSyncRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (!await CanAccessFarmAsync(request.FarmId)) return Forbid();

            var item = new OfflineSyncItem
            {
                FarmId = request.FarmId,
                ClientId = request.ClientId.Trim(),
                EntityName = request.EntityName.Trim(),
                PayloadJson = request.PayloadJson
            };

            await _db.OfflineSyncItems.AddAsync(item);
            await _db.SaveChangesAsync();
            return Accepted(new { item.Id, item.Status });
        }

        private async Task<IEnumerable<Farm>> GetAccessibleFarmsAsync()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            return await _farmAccess.GetAccessibleFarmsAsync(GetUserId(), role);
        }

        private async Task<bool> CanAccessFarmAsync(int farmId)
        {
            var farms = await GetAccessibleFarmsAsync();
            return farms.Any(f => f.Id == farmId);
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    }

    public record SensorReadingRequest(int FarmId, string DeviceId, SensorReadingType ReadingType, decimal Value, string Unit, string? BarnZone, DateTime? RecordedAt);
    public record GpsSnapshotRequest(int FarmId, int? CattleId, string TrackerId, decimal Latitude, decimal Longitude, decimal? SpeedKph, DateTime? RecordedAt);
    public record FeedingCommandRequest(int FarmId, int? CattleId, string ControllerId, string FeedName, double QuantityKg, DateTime? ScheduledAt, string? Notes);
    public record MilkMachineImportRequest(int FarmId, int? CattleId, string MachineId, decimal YieldLiters, decimal? FatPercentage, decimal? ProteinPercentage, DateTime? CollectedAt);
    public record OfflineSyncRequest(int FarmId, string ClientId, string EntityName, string PayloadJson);
}
