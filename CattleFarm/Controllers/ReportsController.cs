using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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

        [HttpGet]
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

            var summaryRows = new List<IReadOnlyList<object?>>
            {
                new object?[] { "Smart Cattle Farm Report" },
                new object?[] { "Farm", selectedFarm.Name },
                new object?[] { "From", dateFrom.ToString("yyyy-MM-dd") },
                new object?[] { "To", dateTo.ToString("yyyy-MM-dd") },
                Array.Empty<object?>(),
                new object?[] { "Metric", "Value" },
                new object?[] { "Total revenue", revenue },
                new object?[] { "Total expenses", expenses },
                new object?[] { "Net profit", profit },
                new object?[] { "Total milk yield (L)", milkTotal },
                new object?[] { "Total cattle", cattle.Count },
                new object?[] { "Active cattle", cattle.Count(c => c.Status == CattleStatus.Active) },
                new object?[] { "Sick/Critical cattle", cattle.Count(c => c.HealthStatus is HealthStatus.Sick or HealthStatus.Critical) }
            };

            var monthlyRows = new List<IReadOnlyList<object?>> { new object?[] { "Month", "Revenue", "Expenses" } };
            monthlyRows.AddRange(trend.Select(t => new object?[] { t.Month, t.Revenue, t.Expense }));

            var expenseRows = new List<IReadOnlyList<object?>> { new object?[] { "Category", "Total" } };
            expenseRows.AddRange(expenseBreakdown.Select(e => new object?[] { e.Category, e.Total }));

            var revenueRows = new List<IReadOnlyList<object?>> { new object?[] { "Source", "Total" } };
            revenueRows.AddRange(revenueBreakdown.Select(r => new object?[] { r.Source, r.Total }));

            var excelBytes = BuildWorkbook(
                ("Summary", summaryRows),
                ("Monthly Trend", monthlyRows),
                ("Expenses", expenseRows),
                ("Revenue", revenueRows));

            var fileName = $"farm-report-{selectedFarm.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(
                excelBytes,
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

        private static byte[] BuildWorkbook(params (string Name, IEnumerable<IReadOnlyList<object?>> Rows)[] worksheets)
        {
            using var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                for (var i = 0; i < worksheets.Length; i++)
                {
                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(CreateSheetData(worksheets[i].Rows));

                    sheets.Append(new Sheet
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (uint)i + 1,
                        Name = worksheets[i].Name
                    });
                }

                workbookPart.Workbook.Save();
            }

            return stream.ToArray();
        }

        private static SheetData CreateSheetData(IEnumerable<IReadOnlyList<object?>> rows)
        {
            var sheetData = new SheetData();
            foreach (var values in rows)
            {
                var row = new Row();
                foreach (var value in values)
                {
                    row.Append(CreateCell(value));
                }

                sheetData.Append(row);
            }

            return sheetData;
        }

        private static Cell CreateCell(object? value)
        {
            if (value == null)
                return new Cell { DataType = CellValues.String, CellValue = new CellValue(string.Empty) };

            return value switch
            {
                decimal decimalValue => CreateNumberCell(decimalValue),
                double doubleValue => CreateNumberCell(doubleValue),
                int intValue => CreateNumberCell(intValue),
                long longValue => CreateNumberCell(longValue),
                float floatValue => CreateNumberCell(floatValue),
                _ => new Cell
                {
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new Text(value.ToString() ?? string.Empty))
                }
            };
        }

        private static Cell CreateNumberCell<T>(T value) where T : IFormattable =>
            new()
            {
                DataType = CellValues.Number,
                CellValue = new CellValue(value.ToString(null, CultureInfo.InvariantCulture))
            };

        private int GetUserId() =>
            int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    }
}
