using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using ClosedXML.Excel;
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
        private readonly IFarmAccessService _farmAccess;
        private readonly IPdfService _pdf;

        public ReportsController(IUnitOfWork uow, IFinancialService fin, IFarmAccessService farmAccess, IPdfService pdf)
        {
            _uow = uow;
            _fin = fin;
            _farmAccess = farmAccess;
            _pdf = pdf;
        }

        public async Task<IActionResult> Index(int? farmId = null, DateTime? from = null, DateTime? to = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var farms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            var selectedFarm = farmId.HasValue
                ? farms.FirstOrDefault(f => f.Id == farmId.Value)
                : farms.FirstOrDefault();

            if (selectedFarm != null)
            {
                if (!farms.Any(f => f.Id == selectedFarm.Id))
                {
                    return Forbid();
                }
            }

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

        public async Task<IActionResult> ExportExcel(int? farmId = null, DateTime? from = null, DateTime? to = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var farms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            var selectedFarm = farmId.HasValue
                ? farms.FirstOrDefault(f => f.Id == farmId.Value)
                : farms.FirstOrDefault();

            if (selectedFarm == null)
                return NotFound("No accessible farm was found for export.");

            var dateFrom = from ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var dateTo = to ?? DateTime.UtcNow;
            var fid = selectedFarm.Id;

            var revenue = await _fin.GetTotalRevenueAsync(fid, dateFrom, dateTo);
            var expenses = await _fin.GetTotalExpensesAsync(fid, dateFrom, dateTo);
            var profit = revenue - expenses;
            var milkTotal = await _uow.MilkProductions.GetTotalYieldByFarmAsync(fid, dateFrom, dateTo.AddDays(1));
            var trend = (await _fin.GetMonthlyTrendAsync(fid, 6)).ToList();
            var cattle = (await _uow.Cattles.GetByFarmIdAsync(fid)).ToList();
            var expenseBreakdown = (await _uow.Expenses.GetByFarmIdAsync(fid, dateFrom, dateTo))
                .GroupBy(e => e.Category)
                .Select(g => new { Category = g.Key.ToString(), Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();
            var revenueBreakdown = (await _uow.Revenues.GetByFarmIdAsync(fid, dateFrom, dateTo))
                .GroupBy(r => r.Source)
                .Select(g => new { Source = g.Key.ToString(), Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .ToList();

            using var workbook = new XLWorkbook();
            var summary = workbook.Worksheets.Add("Summary");
            summary.Cell(1, 1).Value = "Smart Cattle Farm Report";
            summary.Cell(2, 1).Value = "Farm";
            summary.Cell(2, 2).Value = selectedFarm.Name;
            summary.Cell(3, 1).Value = "From";
            summary.Cell(3, 2).Value = dateFrom;
            summary.Cell(4, 1).Value = "To";
            summary.Cell(4, 2).Value = dateTo;

            summary.Cell(6, 1).Value = "Metric";
            summary.Cell(6, 2).Value = "Value";
            var rows = new (string Metric, object Value)[]
            {
                ("Total revenue", revenue),
                ("Total expenses", expenses),
                ("Net profit", profit),
                ("Total milk yield (L)", milkTotal),
                ("Total cattle", cattle.Count),
                ("Active cattle", cattle.Count(c => c.Status == CattleStatus.Active)),
                ("Sick/Critical cattle", cattle.Count(c => c.HealthStatus is HealthStatus.Sick or HealthStatus.Critical))
            };

            for (var i = 0; i < rows.Length; i++)
            {
                summary.Cell(7 + i, 1).Value = rows[i].Metric;
                summary.Cell(7 + i, 2).Value = XLCellValue.FromObject(rows[i].Value);
            }

            var monthly = workbook.Worksheets.Add("Monthly Trend");
            monthly.Cell(1, 1).Value = "Month";
            monthly.Cell(1, 2).Value = "Revenue";
            monthly.Cell(1, 3).Value = "Expenses";
            for (var i = 0; i < trend.Count; i++)
            {
                monthly.Cell(2 + i, 1).Value = trend[i].Month;
                monthly.Cell(2 + i, 2).Value = trend[i].Revenue;
                monthly.Cell(2 + i, 3).Value = trend[i].Expense;
            }

            var expensesSheet = workbook.Worksheets.Add("Expenses");
            expensesSheet.Cell(1, 1).Value = "Category";
            expensesSheet.Cell(1, 2).Value = "Total";
            for (var i = 0; i < expenseBreakdown.Count; i++)
            {
                expensesSheet.Cell(2 + i, 1).Value = expenseBreakdown[i].Category;
                expensesSheet.Cell(2 + i, 2).Value = expenseBreakdown[i].Total;
            }

            var revenueSheet = workbook.Worksheets.Add("Revenue");
            revenueSheet.Cell(1, 1).Value = "Source";
            revenueSheet.Cell(1, 2).Value = "Total";
            for (var i = 0; i < revenueBreakdown.Count; i++)
            {
                revenueSheet.Cell(2 + i, 1).Value = revenueBreakdown[i].Source;
                revenueSheet.Cell(2 + i, 2).Value = revenueBreakdown[i].Total;
            }

            foreach (var sheet in workbook.Worksheets)
            {
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Columns().AdjustToContents();
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"farm-report-{selectedFarm.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int? farmId = null, DateTime? from = null, DateTime? to = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var farms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            var selectedFarm = farmId.HasValue
                ? farms.FirstOrDefault(f => f.Id == farmId.Value)
                : farms.FirstOrDefault();

            if (selectedFarm == null)
                return NotFound("No accessible farm was found for export.");

            var dateFrom = from ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var dateTo = to ?? DateTime.UtcNow;
            var fid = selectedFarm.Id;

            var revenue = await _fin.GetTotalRevenueAsync(fid, dateFrom, dateTo);
            var expenses = await _fin.GetTotalExpensesAsync(fid, dateFrom, dateTo);
            var profit = revenue - expenses;
            var milkTotal = await _uow.MilkProductions.GetTotalYieldByFarmAsync(fid, dateFrom, dateTo.AddDays(1));
            
            var cattle = (await _uow.Cattles.GetByFarmIdAsync(fid)).ToList();
            var totalCattle = cattle.Count;
            var activeCattle = cattle.Count(c => c.Status == CattleStatus.Active);
            var sickCattle = cattle.Count(c => c.HealthStatus is HealthStatus.Sick or HealthStatus.Critical);

            var expenseBreakdown = (await _uow.Expenses.GetByFarmIdAsync(fid, dateFrom, dateTo))
                .GroupBy(e => e.Category)
                .Select(g => (Category: g.Key.ToString(), Total: g.Sum(x => x.Amount)))
                .OrderByDescending(x => x.Total)
                .ToList();

            var revenueBreakdown = (await _uow.Revenues.GetByFarmIdAsync(fid, dateFrom, dateTo))
                .GroupBy(r => r.Source)
                .Select(g => (Source: g.Key.ToString(), Total: g.Sum(x => x.Amount)))
                .OrderByDescending(x => x.Total)
                .ToList();

            var pdfBytes = _pdf.GenerateReportPdf(
                selectedFarm.Name, dateFrom, dateTo,
                revenue, expenses, profit, milkTotal,
                totalCattle, activeCattle, sickCattle,
                expenseBreakdown, revenueBreakdown);

            var fileName = $"financial-report-{selectedFarm.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    }
}
