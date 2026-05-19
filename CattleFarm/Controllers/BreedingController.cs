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
    public class BreedingController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private const int PageSize = 15;

        public BreedingController(IUnitOfWork uow, IAuditService audit)
        {
            _uow   = uow;
            _audit = audit;
        }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, BreedingOutcome? outcome = null)
        {
            var (items, total) = await _uow.Breedings.GetPagedAsync(page, PageSize, farmId, outcome);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["TotalCount"]  = total;
            ViewData["FarmId"]      = farmId;
            ViewData["Outcome"]     = outcome;
            ViewBag.Farms = await _uow.Farms.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var b = await _uow.Breedings.GetByIdAsync(id);
            if (b is null) return NotFound();
            return View(b);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(int? farmId = null)
        {
            await LoadDropdowns(farmId);
            return View(new BreedingViewModel { BreedingDate = DateTime.Today, FarmId = farmId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(BreedingViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadDropdowns(vm.FarmId); return View(vm); }
            var b = new Breeding
            {
                CattleId    = vm.CattleId,
                SireId      = vm.SireId == 0 ? null : vm.SireId,
                FarmId      = vm.FarmId,
                BreedingDate          = vm.BreedingDate,
                ExpectedCalvingDate   = vm.ExpectedCalvingDate,
                Method      = vm.Method,
                Outcome     = BreedingOutcome.Pending,
                SireBreed   = vm.SireBreed,
                InseminationTechnician = vm.InseminationTechnician,
                Cost        = vm.Cost,
                Notes       = vm.Notes
            };
            await _uow.Breedings.AddAsync(b);
            await _uow.SaveChangesAsync();
            await _audit.LogActivityAsync(GetUserId(), $"Recorded breeding for cattle ID {vm.CattleId}", "Breeding", b.Id);
            TempData["SuccessMessage"] = "Breeding record created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var b = await _uow.Breedings.GetByIdAsync(id);
            if (b is null) return NotFound();
            await LoadDropdowns(b.FarmId);
            var vm = MapToVm(b);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, BreedingViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) { await LoadDropdowns(vm.FarmId); return View(vm); }
            var b = await _uow.Breedings.GetByIdAsync(id);
            if (b is null) return NotFound();
            b.CattleId    = vm.CattleId;
            b.SireId      = vm.SireId == 0 ? null : vm.SireId;
            b.FarmId      = vm.FarmId;
            b.BreedingDate          = vm.BreedingDate;
            b.ExpectedCalvingDate   = vm.ExpectedCalvingDate;
            b.ActualCalvingDate     = vm.ActualCalvingDate;
            b.Method      = vm.Method;
            b.Outcome     = vm.Outcome;
            b.CalvesCount = vm.CalvesCount;
            b.SireBreed   = vm.SireBreed;
            b.InseminationTechnician = vm.InseminationTechnician;
            b.Cost        = vm.Cost;
            b.Notes       = vm.Notes;
            _uow.Breedings.Update(b);
            await _uow.SaveChangesAsync();
            TempData["SuccessMessage"] = "Breeding record updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            var b = await _uow.Breedings.GetByIdAsync(id);
            if (b != null) { _uow.Breedings.Delete(b); await _uow.SaveChangesAsync(); }
            TempData["SuccessMessage"] = "Breeding record deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? farmId)
        {
            ViewBag.Farms = await _uow.Farms.GetAllAsync();
            if (farmId.HasValue)
                ViewBag.Cattle = await _uow.Cattles.GetByFarmIdAsync(farmId.Value);
            else
                ViewBag.Cattle = await _uow.Cattles.GetAllAsync();
        }

        private static BreedingViewModel MapToVm(Breeding b) => new()
        {
            Id = b.Id, CattleId = b.CattleId, SireId = b.SireId ?? 0, FarmId = b.FarmId,
            BreedingDate = b.BreedingDate, ExpectedCalvingDate = b.ExpectedCalvingDate,
            ActualCalvingDate = b.ActualCalvingDate, Method = b.Method, Outcome = b.Outcome,
            CalvesCount = b.CalvesCount, SireBreed = b.SireBreed,
            InseminationTechnician = b.InseminationTechnician, Cost = b.Cost, Notes = b.Notes
        };

        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    }
}
