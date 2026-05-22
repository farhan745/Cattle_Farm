using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _payrollService;
        private readonly IFarmService    _farmService;

        public PayrollController(IPayrollService payrollService, IFarmService farmService)
        {
            _payrollService = payrollService;
            _farmService = farmService;
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
    }
}
