using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICattleService      _cattleService;
        private readonly IDoctorService      _doctorService;
        private readonly IFarmService        _farmService;

        public AppointmentController(IAppointmentService appointment, ICattleService cattle, IDoctorService doctor, IFarmService farm)
        { _appointmentService = appointment; _cattleService = cattle; _doctorService = doctor; _farmService = farm; }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, AppointmentStatus? status = null)
        {
            var (items, total) = await _appointmentService.GetPagedAsync(page, 10, farmId, status);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)10);
            ViewBag.Farms = await _farmService.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var appt = await _appointmentService.GetByIdAsync(id);
            if (appt is null) return NotFound();
            return View(appt);
        }

        [Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> Create(int? cattleId, int? farmId)
        {
            await LoadDropdowns();
            return View(new AppointmentViewModel { CattleId = cattleId ?? 0, FarmId = farmId ?? 0, ScheduledAt = DateTime.Now.AddDays(1) });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> Create(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadDropdowns(); return View(vm); }
            var appt = await _appointmentService.CreateAsync(vm, GetUserId());
            TempData["SuccessMessage"] = "Appointment scheduled successfully.";
            return RedirectToAction(nameof(Details), new { id = appt.Id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status, string? notes)
        {
            await _appointmentService.UpdateStatusAsync(id, status, notes);
            TempData["SuccessMessage"] = $"Appointment marked as {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> Complete(int id)
        {
            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Completed, null);
            TempData["SuccessMessage"] = "Appointment marked as completed.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> Cancel(int id)
        {
            await _appointmentService.UpdateStatusAsync(id, AppointmentStatus.Cancelled, null);
            TempData["SuccessMessage"] = "Appointment cancelled.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            await _appointmentService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Appointment deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Cattles = await _cattleService.GetByFarmIdAsync(0);
            ViewBag.Doctors = await _doctorService.GetAllAsync();
            ViewBag.Farms   = await _farmService.GetAllAsync();
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
