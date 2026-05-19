using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class FarmController : Controller
    {
        private readonly IFarmService _farmService;
        private readonly IAuditService _auditService;
        private readonly ISubscriptionService _subscriptionService;
        private const int PageSize = 10;

        public FarmController(IFarmService farmService, IAuditService auditService, ISubscriptionService subscriptionService)
        {
            _farmService = farmService;
            _auditService = auditService;
            _subscriptionService = subscriptionService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var (items, total) = await _farmService.GetPagedAsync(page, PageSize, search);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"]      = search;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var farm = await _farmService.GetWithDetailsAsync(id);
            if (farm is null) return NotFound();
            return View(farm);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public IActionResult Create() => View(new FarmViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(FarmViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var userId = GetUserId();
            var farm = await _farmService.CreateAsync(vm, userId);
            await _auditService.LogActivityAsync(userId, $"Registered farm: {farm.Name}", "Farm", farm.Id);
            TempData["SuccessMessage"] = $"Farm '{farm.Name}' registered — awaiting approval.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var farm = await _farmService.GetByIdAsync(id);
            if (farm is null) return NotFound();
            var vm = new FarmViewModel { Id = farm.Id, Name = farm.Name, Location = farm.Location, FarmType = farm.FarmType, SizeInAcres = farm.SizeInAcres, Capacity = farm.Capacity, Description = farm.Description, Latitude = farm.Latitude, Longitude = farm.Longitude, ExistingImagePath = farm.ImagePath };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, FarmViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            await _farmService.UpdateAsync(id, vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Updated farm: {vm.Name}", "Farm", id);
            TempData["SuccessMessage"] = "Farm updated successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Approve(int id)
        {
            await _farmService.ApproveAsync(id);
            TempData["SuccessMessage"] = "Farm approved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Reject(int id)
        {
            await _farmService.RejectAsync(id);
            TempData["SuccessMessage"] = "Farm rejected.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await _farmService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Farm deleted.";
            return RedirectToAction(nameof(Index));
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var parsed) ? parsed : 0;
        }
    }
}
