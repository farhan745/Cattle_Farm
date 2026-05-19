using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private const int PageSize = 15;

        public FeedController(IUnitOfWork uow, IAuditService audit)
        {
            _uow   = uow;
            _audit = audit;
        }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, FeedType? feedType = null)
        {
            var (items, total) = await _uow.FeedRecords.GetPagedAsync(page, PageSize, farmId, feedType);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["TotalCount"]  = total;
            ViewData["FarmId"]      = farmId;
            ViewData["FeedType"]    = feedType;
            ViewBag.Farms = await _uow.Farms.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var f = await _uow.FeedRecords.GetByIdAsync(id);
            if (f is null) return NotFound();
            return View(f);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(int? farmId = null)
        {
            await LoadDropdowns(farmId);
            return View(new FeedViewModel { Date = DateTime.Today, FarmId = farmId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(FeedViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadDropdowns(vm.FarmId); return View(vm); }
            var f = new FeedRecord
            {
                FarmId     = vm.FarmId,
                CattleId   = vm.CattleId == 0 ? null : vm.CattleId,
                FeedType   = vm.FeedType,
                FeedName   = vm.FeedName,
                QuantityKg = vm.QuantityKg,
                CostPerKg  = vm.CostPerKg,
                Date       = vm.Date,
                Supplier   = vm.Supplier,
                Notes      = vm.Notes
            };
            await _uow.FeedRecords.AddAsync(f);
            await _uow.SaveChangesAsync();
            await _audit.LogActivityAsync(GetUserId(), $"Recorded feed: {vm.FeedName} ({vm.QuantityKg} kg)", "Feed", f.Id);
            TempData["SuccessMessage"] = "Feed record added.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var f = await _uow.FeedRecords.GetByIdAsync(id);
            if (f is null) return NotFound();
            await LoadDropdowns(f.FarmId);
            return View(MapToVm(f));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, FeedViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) { await LoadDropdowns(vm.FarmId); return View(vm); }
            var f = await _uow.FeedRecords.GetByIdAsync(id);
            if (f is null) return NotFound();
            f.FarmId     = vm.FarmId;
            f.CattleId   = vm.CattleId == 0 ? null : vm.CattleId;
            f.FeedType   = vm.FeedType;
            f.FeedName   = vm.FeedName;
            f.QuantityKg = vm.QuantityKg;
            f.CostPerKg  = vm.CostPerKg;
            f.Date       = vm.Date;
            f.Supplier   = vm.Supplier;
            f.Notes      = vm.Notes;
            _uow.FeedRecords.Update(f);
            await _uow.SaveChangesAsync();
            TempData["SuccessMessage"] = "Feed record updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            var f = await _uow.FeedRecords.GetByIdAsync(id);
            if (f != null) { _uow.FeedRecords.Delete(f); await _uow.SaveChangesAsync(); }
            TempData["SuccessMessage"] = "Feed record deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? farmId)
        {
            ViewBag.Farms  = await _uow.Farms.GetAllAsync();
            ViewBag.Cattle = farmId.HasValue
                ? await _uow.Cattles.GetByFarmIdAsync(farmId.Value)
                : await _uow.Cattles.GetAllAsync();
        }

        private static FeedViewModel MapToVm(FeedRecord f) => new()
        {
            Id = f.Id, FarmId = f.FarmId, CattleId = f.CattleId ?? 0,
            FeedType = f.FeedType, FeedName = f.FeedName,
            QuantityKg = f.QuantityKg, CostPerKg = f.CostPerKg,
            Date = f.Date, Supplier = f.Supplier, Notes = f.Notes
        };

        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    }
}
