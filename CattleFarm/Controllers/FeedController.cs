using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private readonly IFarmAccessService _farmAccess;
        private readonly CattleFarmDbContext _db;
        private const int PageSize = 15;

        public FeedController(IUnitOfWork uow, IAuditService audit, IFarmAccessService farmAccess, CattleFarmDbContext db)
        {
            _uow = uow;
            _audit = audit;
            _farmAccess = farmAccess;
            _db = db;
        }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, FeedType? feedType = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            IEnumerable<FeedRecord> items;
            int total;

            if (role == AppRoles.Manager)
            {
                var managerFarmId = await _farmAccess.GetActiveManagerFarmIdAsync(userId);
                if (!managerFarmId.HasValue)
                {
                    ViewBag.Farms = Enumerable.Empty<Farm>();
                    ViewData["CurrentPage"] = page;
                    ViewData["TotalPages"] = 0;
                    return View(Enumerable.Empty<FeedRecord>());
                }
                farmId ??= managerFarmId;
                if (farmId != managerFarmId)
                {
                    ViewBag.Farms = await _farmAccess.GetAccessibleFarmsAsync(userId, role);
                    return View(Enumerable.Empty<FeedRecord>());
                }
                var mgrResult = await _uow.FeedRecords.GetPagedAsync(page, PageSize, managerFarmId.Value, feedType);
                items = mgrResult.Items;
                total = mgrResult.Total;
                ViewBag.Farms = await _farmAccess.GetAccessibleFarmsAsync(userId, role);
            }
            else if (role == AppRoles.Owner)
            {
                var ownerFarms = await _uow.Farms.GetByOwnerIdAsync(userId);
                var ownerFarmIds = ownerFarms.Select(f => f.Id).ToList();

                if (farmId.HasValue)
                {
                    if (!ownerFarmIds.Contains(farmId.Value))
                    {
                        farmId = -1; // Force no results if searching a farm not owned
                    }
                    var result = await _uow.FeedRecords.GetPagedAsync(page, PageSize, farmId.Value, feedType);
                    items = result.Items;
                    total = result.Total;
                }
                else
                {
                    var result = await _uow.FeedRecords.GetPagedAsync(page, PageSize, null, feedType, ownerFarmIds);
                    items = result.Items;
                    total = result.Total;
                }
                ViewBag.Farms = ownerFarms;
            }
            else
            {
                var result = await _uow.FeedRecords.GetPagedAsync(page, PageSize, farmId, feedType);
                items = result.Items;
                total = result.Total;
                ViewBag.Farms = await _uow.Farms.GetAllAsync();
            }

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["TotalCount"]  = total;
            ViewData["FarmId"]      = farmId;
            ViewData["FeedType"]    = feedType;
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
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (role == AppRoles.Owner)
            {
                ViewBag.Farms = await _uow.Farms.GetByOwnerIdAsync(userId);
            }
            else
            {
                ViewBag.Farms = await _uow.Farms.GetAllAsync();
            }

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

        // ── COST PER LITER OF MILK ANALYTICS ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CostPerLiter(int? farmId = null, DateTime? from = null, DateTime? to = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var accessibleFarms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            if (farmId.HasValue && !accessibleFarms.Any(f => f.Id == farmId.Value))
                return Forbid();

            var selectedFarmId = farmId ?? accessibleFarms.FirstOrDefault()?.Id ?? 0;
            if (selectedFarmId == 0)
            {
                return View(new List<CostPerLiterViewModel>());
            }

            var startDate = from ?? DateTime.Today.AddDays(-30);
            var endDate = to ?? DateTime.Today;

            // Group feed costs by date
            var feedCosts = await _db.FeedRecords
                .Where(f => f.FarmId == selectedFarmId && f.Date >= startDate && f.Date <= endDate)
                .GroupBy(f => f.Date.Date)
                .Select(g => new { Date = g.Key, Cost = g.Sum(f => (decimal)f.QuantityKg * f.CostPerKg) })
                .ToListAsync();

            // Group milk yields by date
            var milkYields = await _db.MilkProductions
                .Where(m => m.FarmId == selectedFarmId && m.Date >= startDate && m.Date <= endDate)
                .GroupBy(m => m.Date.Date)
                .Select(g => new { Date = g.Key, Liters = g.Sum(m => m.MorningYieldLiters + m.EveningYieldLiters) })
                .ToListAsync();

            // Join the two sets by Date
            var dates = feedCosts.Select(f => f.Date).Union(milkYields.Select(m => m.Date)).OrderBy(d => d).ToList();
            var result = new List<CostPerLiterViewModel>();

            foreach (var date in dates)
            {
                var feed = feedCosts.FirstOrDefault(f => f.Date == date);
                var milk = milkYields.FirstOrDefault(m => m.Date == date);

                result.Add(new CostPerLiterViewModel
                {
                    Date = date,
                    TotalFeedCost = feed?.Cost ?? 0,
                    TotalMilkLiters = milk?.Liters ?? 0
                });
            }

            ViewBag.Farms = accessibleFarms;
            ViewBag.FarmId = selectedFarmId;
            ViewBag.From = startDate.ToString("yyyy-MM-dd");
            ViewBag.To = endDate.ToString("yyyy-MM-dd");

            return View(result);
        }

        // ── SEASONAL FEED ADJUSTMENT RECOMMENDATIONS ───────────────────────
        [HttpGet]
        public IActionResult SeasonalRecommendations(int? farmId = null)
        {
            var month = DateTime.Today.Month;
            string season;
            string advice;
            List<string> feeds;

            if (month >= 3 && month <= 5)
            {
                season = "Summer (Hot & Dry)";
                advice = "Ensure continuous supply of clean, cool water. Feed green fodder early in the morning and late in the evening to prevent heat stress. Supplement with salt and buffer minerals to maintain rumen health.";
                feeds = new List<string> { "Green Maize", "Napier Grass", "Wheat Bran", "Mineral Mixture" };
            }
            else if (month >= 6 && month <= 10)
            {
                season = "Monsoon (Wet & Humid)";
                advice = "Avoid feeding wet or damp grass that could contain mold or parasites. Store feed in elevated, dry areas. Provide dry hay or straw to balance moisture levels from fresh green grass.";
                feeds = new List<string> { "Dry Straw / Hay", "Rice Polish", "Mustard Oil Cake", "DCP / Calcium Supplements" };
            }
            else
            {
                season = "Winter (Cold & Dry)";
                advice = "Increase feed quantity by 10-15% as animals need extra energy to maintain body temperature. Feed high-quality legume hays and energy concentrates. Ensure water is not too cold.";
                feeds = new List<string> { "Alfalfa Hay", "Silage", "Molasses", "Crushed Maize / Barley" };
            }

            ViewBag.Season = season;
            ViewBag.Advice = advice;
            ViewBag.Feeds = feeds;
            ViewBag.FarmId = farmId;

            return View();
        }
    }
}
