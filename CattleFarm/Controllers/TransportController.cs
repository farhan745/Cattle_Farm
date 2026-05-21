using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class TransportController : Controller
    {
        private readonly ITransportService    _transport;
        private readonly INotificationService _notif;

        public TransportController(ITransportService transport, INotificationService notif)
        {
            _transport = transport;
            _notif     = notif;
        }

        // ── Dashboard ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var vm = await _transport.GetDashboardAsync();
            return View(vm);
        }

        // ── Vehicles ───────────────────────────────────────────────────────────
        public async Task<IActionResult> Vehicles(int page = 1, string? status = null, string? search = null)
        {
            VehicleStatus? statusFilter = Enum.TryParse<VehicleStatus>(status, out var s) ? s : null;
            var (items, total) = await _transport.GetVehiclesPagedAsync(page, 12, statusFilter, search);
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling(total / 12.0);
            ViewBag.StatusFilter = status;
            ViewBag.Search       = search;
            return View(items);
        }

        public IActionResult CreateVehicle() => View("VehicleForm", new VehicleViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVehicle(VehicleViewModel vm)
        {
            if (!ModelState.IsValid) return View("VehicleForm", vm);
            await _transport.CreateVehicleAsync(vm);
            TempData["SuccessMessage"] = "Vehicle added successfully.";
            return RedirectToAction(nameof(Vehicles));
        }

        public async Task<IActionResult> EditVehicle(int id)
        {
            var v = await _transport.GetVehicleByIdAsync(id);
            if (v is null) return NotFound();
            var vm = new VehicleViewModel
            {
                Id = v.Id, Name = v.Name, Type = v.Type, FuelType = v.FuelType,
                Status = v.Status, RegistrationNumber = v.RegistrationNumber,
                Capacity = v.Capacity, CapacityUnit = v.CapacityUnit,
                FuelCostPerKm = v.FuelCostPerKm, Notes = v.Notes
            };
            return View("VehicleForm", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(int id, VehicleViewModel vm)
        {
            if (!ModelState.IsValid) return View("VehicleForm", vm);
            var result = await _transport.UpdateVehicleAsync(id, vm);
            if (result is null) return NotFound();
            TempData["SuccessMessage"] = "Vehicle updated successfully.";
            return RedirectToAction(nameof(Vehicles));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            await _transport.DeleteVehicleAsync(id);
            TempData["SuccessMessage"] = "Vehicle removed.";
            return RedirectToAction(nameof(Vehicles));
        }

        // ── Drivers ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Drivers(int page = 1, string? status = null, string? search = null)
        {
            DriverStatus? statusFilter = Enum.TryParse<DriverStatus>(status, out var s) ? s : null;
            var (items, total) = await _transport.GetDriversPagedAsync(page, 12, statusFilter, search);
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling(total / 12.0);
            ViewBag.StatusFilter = status;
            ViewBag.Search       = search;
            return View(items);
        }

        public IActionResult CreateDriver() => View("DriverForm", new DriverViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDriver(DriverViewModel vm)
        {
            if (!ModelState.IsValid) return View("DriverForm", vm);
            await _transport.CreateDriverAsync(vm);
            TempData["SuccessMessage"] = "Driver added successfully.";
            return RedirectToAction(nameof(Drivers));
        }

        public async Task<IActionResult> EditDriver(int id)
        {
            var d = await _transport.GetDriverByIdAsync(id);
            if (d is null) return NotFound();
            var vm = new DriverViewModel
            {
                Id = d.Id, FullName = d.FullName, Phone = d.Phone,
                LicenseNumber = d.LicenseNumber, LicenseType = d.LicenseType,
                ExperienceYears = d.ExperienceYears, Address = d.Address,
                Status = d.Status, Notes = d.Notes, AssignedVehicleId = d.AssignedVehicleId
            };
            return View("DriverForm", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDriver(int id, DriverViewModel vm)
        {
            if (!ModelState.IsValid) return View("DriverForm", vm);
            var result = await _transport.UpdateDriverAsync(id, vm);
            if (result is null) return NotFound();
            TempData["SuccessMessage"] = "Driver updated successfully.";
            return RedirectToAction(nameof(Drivers));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            await _transport.DeleteDriverAsync(id);
            TempData["SuccessMessage"] = "Driver removed.";
            return RedirectToAction(nameof(Drivers));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int driverId, int vehicleId)
        {
            await _transport.AssignDriverToVehicleAsync(driverId, vehicleId);
            TempData["SuccessMessage"] = "Driver assigned to vehicle.";
            return RedirectToAction(nameof(Drivers));
        }

        // ── Transport Requests ─────────────────────────────────────────────────
        public async Task<IActionResult> Requests(int page = 1, string? status = null,
            string? type = null, string? search = null)
        {
            TripStatus?     statusFilter = Enum.TryParse<TripStatus>(status, out var st) ? st : null;
            TransportType?  typeFilter   = Enum.TryParse<TransportType>(type, out var tp) ? tp : null;
            var (items, total) = await _transport.GetRequestsPagedAsync(page, 15, statusFilter, typeFilter, null, search);
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling(total / 15.0);
            ViewBag.StatusFilter = status;
            ViewBag.TypeFilter   = type;
            ViewBag.Search       = search;
            return View(items);
        }

        public IActionResult CreateRequest()
        {
            ViewBag.Title = "New Transport Request";
            return View(new TransportRequestViewModel { ScheduledDate = DateTime.Today.AddDays(1) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(TransportRequestViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var userId = GetUserId();
            var req = await _transport.CreateRequestAsync(vm, userId);
            TempData["SuccessMessage"] = "Transport request submitted successfully.";
            return RedirectToAction(nameof(Requests));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            await _transport.ApproveRequestAsync(id);
            TempData["SuccessMessage"] = "Request approved.";
            return RedirectToAction(nameof(Requests));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(int id)
        {
            await _transport.CancelRequestAsync(id);
            TempData["SuccessMessage"] = "Request cancelled.";
            return RedirectToAction(nameof(Requests));
        }

        // ── Trips ──────────────────────────────────────────────────────────────
        public async Task<IActionResult> Trips(int page = 1, string? status = null)
        {
            TripStatus? statusFilter = Enum.TryParse<TripStatus>(status, out var s) ? s : null;
            var (items, total) = await _transport.GetTripsPagedAsync(page, 15, statusFilter);
            ViewBag.CurrentPage  = page;
            ViewBag.TotalPages   = (int)Math.Ceiling(total / 15.0);
            ViewBag.StatusFilter = status;
            return View(items);
        }

        public async Task<IActionResult> TripDetails(int id)
        {
            var trip = await _transport.GetTripByIdAsync(id);
            if (trip is null) return NotFound();
            return View(trip);
        }

        public async Task<IActionResult> AssignTrip(int requestId)
        {
            var req      = await _transport.GetRequestByIdAsync(requestId);
            if (req is null) return NotFound();

            var vehicles = (await _transport.GetAvailableVehiclesAsync()).ToList();
            var drivers  = (await _transport.GetAvailableDriversAsync()).ToList();

            // Smart suggestions
            var suggestedVehicle = await _transport.SuggestVehicleAsync(
                req.CargoWeight ?? 0, req.RequestType);
            var suggestedDriver = await _transport.SuggestDriverAsync(suggestedVehicle?.Id);

            ViewBag.Request         = req;
            ViewBag.Vehicles        = vehicles;
            ViewBag.Drivers         = drivers;
            ViewBag.SuggestedVehicle= suggestedVehicle;
            ViewBag.SuggestedDriver = suggestedDriver;

            var vm = new TripAssignViewModel
            {
                TransportRequestId = requestId,
                DistanceKm         = req.EstimatedDistanceKm ?? 0,
                VehicleId          = suggestedVehicle?.Id ?? 0,
                DriverId           = suggestedDriver?.Id  ?? 0
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTrip(TripAssignViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var req     = await _transport.GetRequestByIdAsync(vm.TransportRequestId);
                ViewBag.Request  = req;
                ViewBag.Vehicles = await _transport.GetAvailableVehiclesAsync();
                ViewBag.Drivers  = await _transport.GetAvailableDriversAsync();
                return View(vm);
            }
            var trip = await _transport.AssignTripAsync(vm);
            TempData["SuccessMessage"] = "Trip assigned successfully.";
            return RedirectToAction(nameof(TripDetails), new { id = trip.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTrip(int id)
        {
            await _transport.StartTripAsync(id);
            TempData["SuccessMessage"] = "Trip started.";
            return RedirectToAction(nameof(TripDetails), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTrip(TripCompleteViewModel vm)
        {
            await _transport.CompleteTripAsync(vm.TripId, vm.ActualDistance,
                vm.AdditionalCost, vm.AdditionalNote);
            TempData["SuccessMessage"] = "Trip marked as completed.";
            return RedirectToAction(nameof(TripDetails), new { id = vm.TripId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTrip(int id, string? reason)
        {
            await _transport.CancelTripAsync(id, reason);
            TempData["SuccessMessage"] = "Trip cancelled.";
            return RedirectToAction(nameof(Trips));
        }

        // ── Reports ────────────────────────────────────────────────────────────
        public async Task<IActionResult> Reports(DateTime? from = null, DateTime? to = null)
        {
            var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var toDate   = to   ?? DateTime.UtcNow;
            var vm = await _transport.GetReportAsync(fromDate, toDate);
            return View(vm);
        }

        // ── API helpers ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SuggestVehicle(decimal cargoWeight, string requestType)
        {
            TransportType.TryParse(requestType, out TransportType type);
            var v = await _transport.SuggestVehicleAsync(cargoWeight, type);
            if (v is null) return Json(new { });
            return Json(new { id = v.Id, name = v.Name, registration = v.RegistrationNumber,
                capacity = v.Capacity, fuelCostPerKm = v.FuelCostPerKm });
        }

        [HttpGet]
        public async Task<IActionResult> SuggestDriver(int? vehicleId)
        {
            var d = await _transport.SuggestDriverAsync(vehicleId);
            if (d is null) return Json(new { });
            return Json(new { id = d.Id, name = d.FullName, phone = d.Phone, rating = d.Rating });
        }

        [HttpGet]
        public async Task<IActionResult> CalculateCost(decimal distanceKm, int vehicleId)
        {
            var cost = await _transport.CalculateCostAsync(distanceKm, vehicleId);
            return Json(new { cost = cost, formatted = cost.ToString("N2") });
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var parsed) ? parsed : 0;
        }
    }
}
