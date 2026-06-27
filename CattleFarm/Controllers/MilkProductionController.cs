using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class MilkProductionController : Controller
    {
        private readonly IMilkService    _milkService;
        private readonly ICattleService  _cattleService;
        private readonly IFarmService    _farmService;
        private readonly CattleFarmDbContext _db;
        private readonly IFarmAccessService _farmAccess;

        public MilkProductionController(
            IMilkService milk, 
            ICattleService cattle, 
            IFarmService farm,
            CattleFarmDbContext db,
            IFarmAccessService farmAccess)
        { 
            _milkService = milk; 
            _cattleService = cattle; 
            _farmService = farm; 
            _db = db;
            _farmAccess = farmAccess;
        }

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
            ViewBag.From        = from?.ToString("yyyy-MM-dd");
            ViewBag.To          = to?.ToString("yyyy-MM-dd");
            ViewBag.Cattles     = cattles;
            ViewData["TotalToday"]   = todayRecords.Sum(r => (double)(r.MorningYieldLiters + r.EveningYieldLiters));
            ViewData["TotalMonth"]   = monthRecords.Sum(r => (double)(r.MorningYieldLiters + r.EveningYieldLiters));
            ViewData["CurrentPage"]  = page;
            ViewData["TotalPages"]   = totalPages;
            ViewBag.TrendDates  = trendDates;
            ViewBag.TrendYields = trendYields;

            return View(pagedRecords);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> ExportExcel(int farmId = 0, DateTime? from = null, DateTime? to = null)
        {
            if (farmId <= 0)
                return BadRequest("Select a farm before exporting milk production.");

            var records = (await _milkService.GetByFarmAsync(farmId, from, to)).ToList();
            var farms = await _farmService.GetAllAsync();
            var farmName = farms.FirstOrDefault(f => f.Id == farmId)?.Name ?? $"Farm {farmId}";

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Milk Production");
            sheet.Cell(1, 1).Value = "Smart Cattle Farm - Milk Production";
            sheet.Cell(2, 1).Value = "Farm";
            sheet.Cell(2, 2).Value = farmName;
            sheet.Cell(3, 1).Value = "From";
            sheet.Cell(3, 2).Value = from?.ToString("yyyy-MM-dd") ?? "All";
            sheet.Cell(4, 1).Value = "To";
            sheet.Cell(4, 2).Value = to?.ToString("yyyy-MM-dd") ?? "All";

            sheet.Cell(6, 1).Value = "Date";
            sheet.Cell(6, 2).Value = "Cattle";
            sheet.Cell(6, 3).Value = "Tag";
            sheet.Cell(6, 4).Value = "Morning (L)";
            sheet.Cell(6, 5).Value = "Evening (L)";
            sheet.Cell(6, 6).Value = "Total (L)";
            sheet.Cell(6, 7).Value = "Notes";

            for (var i = 0; i < records.Count; i++)
            {
                var row = 7 + i;
                var record = records[i];
                sheet.Cell(row, 1).Value = record.Date;
                sheet.Cell(row, 2).Value = record.Cattle?.Name ?? "";
                sheet.Cell(row, 3).Value = record.Cattle?.TagId ?? "";
                sheet.Cell(row, 4).Value = record.MorningYieldLiters;
                sheet.Cell(row, 5).Value = record.EveningYieldLiters;
                sheet.Cell(row, 6).Value = record.TotalYieldLiters;
                sheet.Cell(row, 7).Value = record.Notes ?? "";
            }

            var totalRow = 8 + records.Count;
            sheet.Cell(totalRow, 5).Value = "Total";
            sheet.Cell(totalRow, 6).Value = records.Sum(r => r.TotalYieldLiters);
            sheet.Range(6, 1, 6, 7).Style.Font.Bold = true;
            sheet.Row(totalRow).Style.Font.Bold = true;
            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"milk-production-{farmId}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
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

        // ── TOP PRODUCERS ────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> TopProducers(int? farmId = null, int days = 30)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var accessibleFarms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            if (farmId.HasValue && !accessibleFarms.Any(f => f.Id == farmId.Value))
                return Forbid();

            var selectedFarmId = farmId ?? accessibleFarms.FirstOrDefault()?.Id ?? 0;
            if (selectedFarmId == 0)
            {
                return View(new List<TopProducerItemViewModel>());
            }

            var startDate = DateTime.Today.AddDays(-days);

            var topCattle = await _db.MilkProductions
                .Include(m => m.Cattle)
                .Where(m => m.FarmId == selectedFarmId && m.Date >= startDate && m.Cattle != null)
                .GroupBy(m => new { m.CattleId, m.Cattle!.TagId, m.Cattle.Name, m.Cattle.Breed, m.Cattle.HealthStatus })
                .Select(g => new TopProducerItemViewModel
                {
                    CattleId = g.Key.CattleId,
                    TagId = g.Key.TagId,
                    Name = g.Key.Name,
                    Breed = g.Key.Breed,
                    HealthStatus = g.Key.HealthStatus,
                    TotalLiters = g.Sum(m => m.MorningYieldLiters + m.EveningYieldLiters),
                    AverageLiters = g.Average(m => m.MorningYieldLiters + m.EveningYieldLiters),
                    RecordCount = g.Count()
                })
                .OrderByDescending(x => x.AverageLiters)
                .Take(20)
                .ToListAsync();

            ViewBag.Farms = accessibleFarms;
            ViewBag.FarmId = selectedFarmId;
            ViewBag.Days = days;

            return View(topCattle);
        }

        // ── SUDDEN MILK YIELD DROP ALERTS ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> DropAlerts(int? farmId = null)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var accessibleFarms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            if (farmId.HasValue && !accessibleFarms.Any(f => f.Id == farmId.Value))
                return Forbid();

            var selectedFarmId = farmId ?? accessibleFarms.FirstOrDefault()?.Id ?? 0;
            if (selectedFarmId == 0)
            {
                return View(new List<MilkDropAlertViewModel>());
            }

            var alerts = await _milkService.DetectYieldDropsAsync(selectedFarmId);

            ViewBag.Farms = accessibleFarms;
            ViewBag.FarmId = selectedFarmId;

            return View(alerts);
        }

        private int GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        private string? GetUserRole()
        {
            if (User.IsInRole(AppRoles.Admin)) return AppRoles.Admin;
            if (User.IsInRole(AppRoles.Owner)) return AppRoles.Owner;
            if (User.IsInRole(AppRoles.Manager)) return AppRoles.Manager;
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
