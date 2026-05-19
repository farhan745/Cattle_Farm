using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
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

        public FinancialController(IFinancialService financial, IFarmService farm, IAuditService audit)
        { _financialService = financial; _farmService = farm; _auditService = audit; }

        public async Task<IActionResult> Index(int farmId = 0, int year = 0, int month = 0)
        {
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
            ViewBag.Farms           = await _farmService.GetAllAsync();
            ViewBag.SelectedFarmId  = farmId;
            ViewBag.Year = year; ViewBag.Month = month;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddExpense(ExpenseViewModel vm)
        {
            if (ModelState.IsValid) await _financialService.CreateExpenseAsync(vm, GetUserId());
            TempData["SuccessMessage"] = "Expense recorded.";
            return RedirectToAction(nameof(Index), new { farmId = vm.FarmId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExpense(int id, int farmId)
        {
            await _financialService.DeleteExpenseAsync(id);
            TempData["SuccessMessage"] = "Expense deleted.";
            return RedirectToAction(nameof(Index), new { farmId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRevenue(RevenueViewModel vm)
        {
            if (ModelState.IsValid) await _financialService.CreateRevenueAsync(vm, GetUserId());
            TempData["SuccessMessage"] = "Revenue recorded.";
            return RedirectToAction(nameof(Index), new { farmId = vm.FarmId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRevenue(int id, int farmId)
        {
            await _financialService.DeleteRevenueAsync(id);
            TempData["SuccessMessage"] = "Revenue entry deleted.";
            return RedirectToAction(nameof(Index), new { farmId });
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
