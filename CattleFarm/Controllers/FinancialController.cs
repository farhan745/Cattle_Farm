using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
    public class FinancialController : Controller
    {
        private readonly IFinancialService _financialService;
        private readonly IFarmService _farmService;
        private readonly IAuditService _auditService;
        private readonly IFarmAccessService _farmAccess;
        private readonly IUnitOfWork _uow;

        public FinancialController(IFinancialService financial, IFarmService farm, IAuditService audit, IFarmAccessService farmAccess, IUnitOfWork uow)
        { _financialService = financial; _farmService = farm; _auditService = audit; _farmAccess = farmAccess; _uow = uow; }

        public async Task<IActionResult> Index(int farmId = 0, int year = 0, int month = 0)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var farms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            if (farmId == 0 && farms.Any())
            {
                farmId = farms.First().Id;
            }

            if (farmId > 0 && !farms.Any(f => f.Id == farmId))
            {
                return Forbid();
            }

            if (year == 0) year = DateTime.UtcNow.Year;
            if (month == 0) month = DateTime.UtcNow.Month;
            var from = new DateTime(year, month, 1);
            var to   = from.AddMonths(1).AddDays(-1);
            ViewBag.TotalExpenses   = await _financialService.GetTotalExpensesAsync(farmId, from, to);
            ViewBag.TotalRevenue    = await _financialService.GetTotalRevenueAsync(farmId, from, to);
            ViewBag.NetProfit       = ViewBag.TotalRevenue - ViewBag.TotalExpenses;
            ViewBag.Expenses        = await _financialService.GetExpensesAsync(farmId, from, to);
            ViewBag.Revenues        = await _financialService.GetRevenuesAsync(farmId, from, to);
            ViewBag.MonthlyTrend    = await _financialService.GetMonthlyTrendAsync(farmId, 12);
            ViewBag.Farms           = farms;
            ViewBag.SelectedFarmId  = farmId;
            ViewBag.Year = year; ViewBag.Month = month;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(ExpenseViewModel vm)
        {
            var userId = GetUserId();
            var role   = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!await _farmAccess.CanOperateFarmAsync(vm.FarmId, userId, role))
                return Forbid();
            if (ModelState.IsValid) await _financialService.CreateExpenseAsync(vm, userId);
            TempData["SuccessMessage"] = "Expense recorded.";
            return RedirectToAction(nameof(Index), new { farmId = vm.FarmId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id, int farmId)
        {
            var userId = GetUserId();
            var role   = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!await _farmAccess.CanOperateFarmAsync(farmId, userId, role))
                return Forbid();
            await _financialService.DeleteExpenseAsync(id);
            TempData["SuccessMessage"] = "Expense deleted.";
            return RedirectToAction(nameof(Index), new { farmId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRevenue(RevenueViewModel vm)
        {
            var userId = GetUserId();
            var role   = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!await _farmAccess.CanOperateFarmAsync(vm.FarmId, userId, role))
                return Forbid();
            if (ModelState.IsValid) await _financialService.CreateRevenueAsync(vm, userId);
            TempData["SuccessMessage"] = "Revenue recorded.";
            return RedirectToAction(nameof(Index), new { farmId = vm.FarmId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRevenue(int id, int farmId)
        {
            var userId = GetUserId();
            var role   = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!await _farmAccess.CanOperateFarmAsync(farmId, userId, role))
                return Forbid();
            await _financialService.DeleteRevenueAsync(id);
            TempData["SuccessMessage"] = "Revenue entry deleted.";
            return RedirectToAction(nameof(Index), new { farmId });
        }

        // ── P&L REPORT (WITH GRAPH VIEW) ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ProfitLoss(int farmId = 0, int? year = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var farms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            if (farmId == 0 && farms.Any())
            {
                farmId = farms.First().Id;
            }

            if (farmId > 0 && !farms.Any(f => f.Id == farmId))
            {
                return Forbid();
            }

            var activeYear = year ?? DateTime.Today.Year;

            // Fetch monthly revenues and expenses for the active year
            var revenues = await _uow.Revenues.GetByFarmIdAsync(farmId, new DateTime(activeYear, 1, 1), new DateTime(activeYear, 12, 31));
            var expenses = await _uow.Expenses.GetByFarmIdAsync(farmId, new DateTime(activeYear, 1, 1), new DateTime(activeYear, 12, 31));

            var monthlyProfitLoss = new List<MonthlyProfitLossViewModel>();

            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(activeYear, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var revAmount = revenues.Where(r => r.Date >= monthStart && r.Date <= monthEnd).Sum(r => r.Amount);
                var expAmount = expenses.Where(e => e.Date >= monthStart && e.Date <= monthEnd).Sum(e => e.Amount);

                monthlyProfitLoss.Add(new MonthlyProfitLossViewModel
                {
                    MonthName = monthStart.ToString("MMMM"),
                    MonthNumber = month,
                    Revenue = revAmount,
                    Expense = expAmount
                });
            }

            ViewBag.Farms = farms;
            ViewBag.SelectedFarmId = farmId;
            ViewBag.Year = activeYear;

            return View(monthlyProfitLoss);
        }

        // ── REVENUE BREAKDOWN ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> RevenueBreakdown(int farmId = 0, int? year = null, int? month = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var farms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            if (farmId == 0 && farms.Any())
            {
                farmId = farms.First().Id;
            }

            if (farmId > 0 && !farms.Any(f => f.Id == farmId))
            {
                return Forbid();
            }

            var activeYear = year ?? DateTime.Today.Year;
            DateTime fromDate;
            DateTime toDate;

            if (month.HasValue && month.Value > 0 && month.Value <= 12)
            {
                fromDate = new DateTime(activeYear, month.Value, 1);
                toDate = fromDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                fromDate = new DateTime(activeYear, 1, 1);
                toDate = new DateTime(activeYear, 12, 31);
            }

            var revenues = await _uow.Revenues.GetByFarmIdAsync(farmId, fromDate, toDate);

            var breakdown = revenues
                .GroupBy(r => r.Source)
                .Select(g => new RevenueBreakdownViewModel
                {
                    Source = g.Key.ToString(),
                    Amount = g.Sum(r => r.Amount),
                    Percentage = revenues.Sum(r => r.Amount) > 0 ? (g.Sum(r => r.Amount) / revenues.Sum(r => r.Amount)) * 100 : 0
                })
                .ToList();

            ViewBag.Farms = farms;
            ViewBag.SelectedFarmId = farmId;
            ViewBag.Year = activeYear;
            ViewBag.Month = month;
            ViewBag.TotalRevenue = revenues.Sum(r => r.Amount);

            return View(breakdown);
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
