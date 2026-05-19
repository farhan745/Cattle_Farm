using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
    public class ReportsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IFinancialService _fin;

        public ReportsController(IUnitOfWork uow, IFinancialService fin)
        {
            _uow = uow;
            _fin = fin;
        }

        public async Task<IActionResult> Index(int? farmId = null, DateTime? from = null, DateTime? to = null)
        {
            var userId = GetUserId();
            var farms  = (await _uow.Farms.GetByOwnerIdAsync(userId)).ToList();

            // Admins see all farms
            if (User.IsInRole(AppRoles.Admin))
                farms = (await _uow.Farms.GetAllAsync()).ToList();

            var selectedFarm = farmId.HasValue
                ? farms.FirstOrDefault(f => f.Id == farmId.Value)
                : farms.FirstOrDefault();

            var dateFrom = from ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var dateTo   = to   ?? DateTime.UtcNow;

            ViewBag.Farms        = farms;
            ViewBag.SelectedFarm = selectedFarm;
            ViewBag.From         = dateFrom.ToString("yyyy-MM-dd");
            ViewBag.To           = dateTo.ToString("yyyy-MM-dd");

            if (selectedFarm == null)
                return View("NoData");

            var fid = selectedFarm.Id;

            // Financial
            var revenue  = await _fin.GetTotalRevenueAsync(fid, dateFrom, dateTo);
            var expenses = await _fin.GetTotalExpensesAsync(fid, dateFrom, dateTo);
            var profit   = revenue - expenses;
            var trend    = await _fin.GetMonthlyTrendAsync(fid, 6);

            // Milk
            var milkTotal = await _uow.MilkProductions.GetTotalYieldByFarmAsync(fid, dateFrom, dateTo.AddDays(1));

            // Expense breakdown by category
            var allExpenses = (await _uow.Expenses.GetByFarmIdAsync(fid, dateFrom, dateTo))
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key.ToString(), Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Revenue breakdown by source
            var allRevenues = (await _uow.Revenues.GetByFarmIdAsync(fid, dateFrom, dateTo))
                .GroupBy(r => r.Source)
                .Select(g => new { Source = g.Key.ToString(), Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Cattle counts
            var cattle = (await _uow.Cattles.GetByFarmIdAsync(fid)).ToList();

            ViewBag.Revenue        = revenue;
            ViewBag.Expenses       = expenses;
            ViewBag.Profit         = profit;
            ViewBag.MilkTotal      = milkTotal;
            ViewBag.Trend          = trend;
            ViewBag.ExpenseBreakdown = allExpenses;
            ViewBag.RevenueBreakdown = allRevenues;
            ViewBag.TotalCattle    = cattle.Count;
            ViewBag.ActiveCattle   = cattle.Count(c => c.Status == CattleStatus.Active);
            ViewBag.SickCattle     = cattle.Count(c => c.HealthStatus is HealthStatus.Sick or HealthStatus.Critical);

            return View();
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    }
}
