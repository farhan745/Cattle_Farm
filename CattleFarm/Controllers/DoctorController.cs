using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IFarmService   _farmService;
        private readonly IAuditService  _auditService;
        private const int PageSize = 10;

        public DoctorController(IDoctorService doctorService, IFarmService farmService, IAuditService auditService)
        { _doctorService = doctorService; _farmService = farmService; _auditService = auditService; }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var (items, total) = await _doctorService.GetPagedAsync(page, PageSize, search);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor is null) return NotFound();
            return View(doctor);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create() { await LoadFarmsAsync(); return View(new DoctorViewModel()); }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(DoctorViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }
            var doctor = await _doctorService.CreateAsync(vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Added doctor: {doctor.FullName}", "Doctor", doctor.Id);
            TempData["SuccessMessage"] = $"Dr. {doctor.FullName} added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var d = await _doctorService.GetByIdAsync(id);
            if (d is null) return NotFound();
            await LoadFarmsAsync();
            var vm = new DoctorViewModel { Id = d.Id, FullName = d.FullName, Specialization = d.Specialization, Phone = d.Phone, Email = d.Email, LicenseNumber = d.LicenseNumber, ConsultationFee = d.ConsultationFee, IsAvailable = d.IsAvailable, FarmId = d.FarmId, Notes = d.Notes, ExistingImagePath = d.ImagePath };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, DoctorViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }
            await _doctorService.UpdateAsync(id, vm);
            TempData["SuccessMessage"] = "Doctor profile updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await _doctorService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Doctor record deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadFarmsAsync() => ViewBag.Farms = await _farmService.GetAllAsync();
        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
