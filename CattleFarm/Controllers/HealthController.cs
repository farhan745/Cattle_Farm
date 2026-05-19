using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class HealthController : Controller
    {
        private readonly IHealthService _healthService;
        private readonly IVaccinationService _vaccinationService;
        private readonly ICattleService _cattleService;
        private readonly IDoctorService _doctorService;
        private readonly IAuditService _auditService;

        public HealthController(IHealthService health, IVaccinationService vaccination, ICattleService cattle, IDoctorService doctor, IAuditService audit)
        { _healthService = health; _vaccinationService = vaccination; _cattleService = cattle; _doctorService = doctor; _auditService = audit; }

        // ── Health Records ────────────────────────────────────────────────────
        public async Task<IActionResult> Index(int page = 1, int? cattleId = null, RiskLevel? riskLevel = null)
        {
            var (items, total) = await _healthService.GetPagedAsync(page, 12, cattleId, riskLevel);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)12);
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var record = await _healthService.GetByIdAsync(id);
            if (record is null) return NotFound();
            return View(record);
        }

        [Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> Create(int? cattleId)
        {
            await LoadCattleAndDoctors();
            return View(new HealthRecordViewModel { CattleId = cattleId ?? 0, RecordDate = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> Create(HealthRecordViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadCattleAndDoctors(); return View(vm); }
            var record = await _healthService.CreateAsync(vm);
            await _cattleService.UpdateHealthStatusAsync(vm.CattleId);
            await _auditService.LogActivityAsync(GetUserId(), $"Added health record for cattle ID {vm.CattleId}", "HealthRecord", record.Id);
            TempData["SuccessMessage"] = "Health record added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> Delete(int id)
        {
            await _healthService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Health record deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ── Vaccinations ──────────────────────────────────────────────────────
        public async Task<IActionResult> Vaccinations(int? cattleId)
        {
            var list = cattleId.HasValue
                ? await _vaccinationService.GetByCattleAsync(cattleId.Value)
                : await _vaccinationService.GetUpcomingAsync(60);
            ViewData["CattleId"] = cattleId;
            return View("Vaccinations", list);
        }

        [Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> AddVaccination(int? cattleId)
        {
            await LoadCattleAndDoctors();
            return View("VaccinationForm", new VaccinationViewModel { CattleId = cattleId ?? 0, VaccinationDate = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> AddVaccination(VaccinationViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadCattleAndDoctors(); return View("VaccinationForm", vm); }
            await _vaccinationService.CreateAsync(vm);
            TempData["SuccessMessage"] = "Vaccination record added.";
            return RedirectToAction(nameof(Vaccinations), new { cattleId = vm.CattleId });
        }

        private async Task LoadCattleAndDoctors()
        {
            ViewBag.Cattles = await _cattleService.SearchAsync(string.Empty); // All cattle
            ViewBag.Doctors = await _doctorService.GetAllAsync();
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
