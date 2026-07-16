using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ClosedXML.Excel;
using System.IO;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace CattleFarm.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _payrollService;
        private readonly IFarmService    _farmService;
        private readonly IPdfService     _pdfService;

        public PayrollController(IPayrollService payrollService, IFarmService farmService, IPdfService pdfService)
        {
            _payrollService = payrollService;
            _farmService = farmService;
            _pdfService = pdfService;
        }

        // GET: Payroll
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Index()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            IEnumerable<PayrollViewModel> payrolls;

            if (role == AppRoles.Owner)
            {
                var farms = await _farmService.GetByOwnerAsync(currentUserId);
                var farmIds = farms.Select(f => f.Id).ToList();
                payrolls = await _payrollService.GetPayrollsByFarmIdsAsync(farmIds);
            }
            else
            {
                payrolls = await _payrollService.GetAllPayrollsAsync();
            }

            return View(payrolls);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> ExportExcel()
        {
            var payrolls = (await GetVisiblePayrollsAsync()).ToList();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Payroll");
            sheet.Cell(1, 1).Value = "Smart Cattle Farm - Payroll Export";
            sheet.Cell(3, 1).Value = "Worker";
            sheet.Cell(3, 2).Value = "Farm";
            sheet.Cell(3, 3).Value = "Month";
            sheet.Cell(3, 4).Value = "Base Salary";
            sheet.Cell(3, 5).Value = "Overtime Hours";
            sheet.Cell(3, 6).Value = "Overtime Pay";
            sheet.Cell(3, 7).Value = "Bonus";
            sheet.Cell(3, 8).Value = "Deductions";
            sheet.Cell(3, 9).Value = "Net Salary";
            sheet.Cell(3, 10).Value = "Status";
            sheet.Cell(3, 11).Value = "Paid At";
            sheet.Cell(3, 12).Value = "Generated At";

            for (var i = 0; i < payrolls.Count; i++)
            {
                var payroll = payrolls[i];
                var row = 4 + i;
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(payroll.Month);
                sheet.Cell(row, 1).Value = payroll.WorkerName;
                sheet.Cell(row, 2).Value = payroll.FarmName;
                sheet.Cell(row, 3).Value = $"{monthName} {payroll.Year}";
                sheet.Cell(row, 4).Value = (double)payroll.BaseSalary;
                sheet.Cell(row, 5).Value = payroll.OvertimeHours;
                sheet.Cell(row, 6).Value = (double)payroll.OvertimePay;
                sheet.Cell(row, 7).Value = (double)payroll.Bonus;
                sheet.Cell(row, 8).Value = (double)payroll.Deductions;
                sheet.Cell(row, 9).Value = (double)payroll.NetSalary;
                sheet.Cell(row, 10).Value = payroll.IsPaid ? "Paid" : "Unpaid";
                sheet.Cell(row, 11).Value = payroll.PaidAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
                sheet.Cell(row, 12).Value = payroll.GeneratedAt.ToString("yyyy-MM-dd HH:mm");
            }

            var totalRow = 4 + payrolls.Count;
            sheet.Cell(totalRow, 3).Value = "Total";
            sheet.Cell(totalRow, 4).Value = (double)payrolls.Sum(p => p.BaseSalary);
            sheet.Cell(totalRow, 6).Value = (double)payrolls.Sum(p => p.OvertimePay);
            sheet.Cell(totalRow, 7).Value = (double)payrolls.Sum(p => p.Bonus);
            sheet.Cell(totalRow, 8).Value = (double)payrolls.Sum(p => p.Deductions);
            sheet.Cell(totalRow, 9).Value = (double)payrolls.Sum(p => p.NetSalary);
            sheet.Range(3, 1, 3, 12).Style.Font.Bold = true;
            sheet.Row(totalRow).Style.Font.Bold = true;
            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"payroll-export-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner + "," + AppRoles.Worker)]
        public async Task<IActionResult> ExportSlipPdf(int id)
        {
            var payroll = await _payrollService.GetPayrollEntityByIdAsync(id);
            if (payroll == null)
            {
                return NotFound();
            }

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (role == AppRoles.Worker && payroll.UserId != currentUserId)
            {
                return Forbid();
            }
            else if (role == AppRoles.Owner)
            {
                var farms = await _farmService.GetByOwnerAsync(currentUserId);
                if (!farms.Any(f => f.Id == payroll.FarmId))
                {
                    return Forbid();
                }
            }

            var pdfBytes = _pdfService.GeneratePayrollSlipPdf(payroll);
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(payroll.Month);
            var sanitizedWorkerName = payroll.Worker?.FullName?.Replace(" ", "_") ?? "Worker";
            var fileName = $"payroll-slip-{sanitizedWorkerName}-{monthName}-{payroll.Year}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // GET: Payroll/Details/{id}
        [Authorize(Roles = AppRoles.AdminManagerOrOwner + "," + AppRoles.Worker)]
        public async Task<IActionResult> Details(int id)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(id);
            if (payroll == null)
            {
                return NotFound();
            }

            // Ownership check: Workers can only view their own salary slips
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            if (User.IsInRole(AppRoles.Worker) && payroll.UserId != currentUserId)
            {
                return Forbid();
            }

            return View(payroll);
        }

        // GET: Payroll/Generate
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public IActionResult Generate()
        {
            return View(new PayrollGenerateViewModel());
        }

        // POST: Payroll/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Generate(PayrollGenerateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _payrollService.GenerateMonthlyPayrollAsync(model.Year, model.Month);
            return RedirectToAction(nameof(Index));
        }

        // GET: Payroll/Edit/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(id);
            if (payroll == null)
            {
                return NotFound();
            }

            var editModel = new PayrollEditViewModel
            {
                Id = payroll.Id,
                UserId = payroll.UserId,
                WorkerId = payroll.WorkerId,
                WorkerName = payroll.WorkerName,
                Year = payroll.Year,
                Month = payroll.Month,
                OvertimeHours = payroll.OvertimeHours,
                BaseSalary = payroll.BaseSalary,
                OvertimePay = payroll.OvertimePay,
                Deductions = payroll.Deductions,
                Bonus = payroll.Bonus,
                NetSalary = payroll.NetSalary,
                IsPaid = payroll.IsPaid
            };

            return View(editModel);
        }

        // POST: Payroll/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id, PayrollEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _payrollService.UpdatePayrollAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Payroll/Delete/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(id);
            if (payroll == null)
            {
                return NotFound();
            }

            return View(payroll);
        }

        // POST: Payroll/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _payrollService.DeletePayrollAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<IEnumerable<PayrollViewModel>> GetVisiblePayrollsAsync()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (role == AppRoles.Owner)
            {
                var farms = await _farmService.GetByOwnerAsync(currentUserId);
                return await _payrollService.GetPayrollsByFarmIdsAsync(farms.Select(f => f.Id));
            }

            return await _payrollService.GetAllPayrollsAsync();
        }
    }
}
