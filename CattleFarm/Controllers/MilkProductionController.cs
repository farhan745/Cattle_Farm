using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class MilkProductionController : Controller
    {
        private readonly IMilkService    _milkService;
        private readonly ICattleService  _cattleService;
        private readonly IFarmService    _farmService;

        public MilkProductionController(IMilkService milk, ICattleService cattle, IFarmService farm)
        { _milkService = milk; _cattleService = cattle; _farmService = farm; }

        public async Task<IActionResult> Index(int farmId = 0, DateTime? from = null, DateTime? to = null, int page = 1)
        {
            var records = await _milkService.GetByFarmAsync(farmId, from, to);
            var allRecords = records.ToList();

            // Pagination
            int pageSize = 20;
            int totalCount = allRecords.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);
            var pagedRecords = allRecords.Skip((page - 1) * pageSize).Take(pageSize);

            // KPI data
            var today = DateTime.Today;
            var todayRecords = allRecords.Where(r => r.Date.Date == today);
            double totalToday = todayRecords.Sum(r => (double)(r.MorningYieldLiters + r.EveningYieldLiters));

            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthRecords = allRecords.Where(r => r.Date >= monthStart && r.Date <= today);
            double totalMonth = monthRecords.Sum(r => (double)(r.MorningYieldLiters + r.EveningYieldLiters));

            // Trend data (last 14 days)
            var trendDates = new List<string>();
            var trendYields = new List<double>();
            for (int i = 13; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                trendDates.Add(date.ToString("MMM dd"));
                var dayYield = allRecords
                    .Where(r => r.Date.Date == date)
                    .Sum(r => (double)(r.MorningYieldLiters + r.EveningYieldLiters));
                trendYields.Add(dayYield);
            }

            // Dropdowns
            var farms = await _farmService.GetAllAsync();
            var cattles = farmId > 0
                ? await _cattleService.GetByFarmIdAsync(farmId)
                : await _cattleService.SearchAsync(string.Empty);

            ViewBag.TotalYield  = allRecords.Sum(r => (double)(r.MorningYieldLiters + r.EveningYieldLiters));
            ViewBag.Farms       = farms;
            ViewBag.FarmId      = farmId;
            ViewBag.Cattles     = cattles;
            ViewData["TotalToday"]   = totalToday;
            ViewData["TotalMonth"]   = totalMonth;
            ViewData["CurrentPage"]  = page;
            ViewData["TotalPages"]   = totalPages;
            ViewBag.TrendDates  = trendDates;
            ViewBag.TrendYields = trendYields;

            return View(pagedRecords);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(int? farmId)
        {
            await LoadDropdowns(farmId);
            return View(new MilkProductionViewModel { Date = DateTime.Today, FarmId = farmId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(MilkProductionViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadDropdowns(vm.FarmId); return View(vm); }
            await _milkService.CreateAsync(vm);
            TempData["SuccessMessage"] = "Milk production logged.";
            return RedirectToAction(nameof(Index), new { farmId = vm.FarmId });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id, int farmId)
        {
            await _milkService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Record deleted.";
            return RedirectToAction(nameof(Index), new { farmId });
        }

        private async Task LoadDropdowns(int? farmId)
        {
            ViewBag.Farms  = await _farmService.GetAllAsync();
            ViewBag.Cattles = farmId.HasValue && farmId > 0
                ? await _cattleService.GetByFarmIdAsync(farmId.Value)
                : await _cattleService.SearchAsync(string.Empty);
        }
    }
}
