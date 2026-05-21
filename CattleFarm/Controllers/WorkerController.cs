using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
    public class WorkerController : Controller
    {
        private readonly IWorkerService _workerService;
        private readonly IFarmService   _farmService;
        private readonly IAuditService  _auditService;
        private const int PageSize = 10;

        public WorkerController(IWorkerService workerService, IFarmService farmService, IAuditService auditService)
        { _workerService = workerService; _farmService = farmService; _auditService = auditService; }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, string? search = null)
        {
            var (items, total) = await _workerService.GetPagedAsync(page, PageSize, farmId, search);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["FarmId"]      = farmId;
            await LoadFarmsAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var worker = await _workerService.GetByIdAsync(id);
            if (worker is null) return NotFound();
            return View(worker);
        }

        public async Task<IActionResult> Create(int? farmId = null)
        {
            await LoadFarmsAsync();
            return View(new WorkerViewModel { FarmId = farmId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkerViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }
            var worker = await _workerService.CreateAsync(vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Hired worker: {worker.FullName}", "Worker", worker.Id);
            TempData["SuccessMessage"] = $"Worker '{worker.FullName}' hired successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var worker = await _workerService.GetByIdAsync(id);
            if (worker is null) return NotFound();
            await LoadFarmsAsync();
            var vm = new WorkerViewModel { Id = worker.Id, FullName = worker.FullName, Role = worker.Role, Phone = worker.Phone, Email = worker.Email, Skills = worker.Skills, ExperienceYears = worker.ExperienceYears, Salary = worker.Salary, IsAvailable = worker.IsAvailable, FarmId = worker.FarmId, Notes = worker.Notes, ExistingImagePath = worker.ImagePath };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkerViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }
            await _workerService.UpdateAsync(id, vm);
            TempData["SuccessMessage"] = "Worker updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _workerService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Worker record deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadFarmsAsync() => ViewBag.Farms = await _farmService.GetAllAsync();
        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
