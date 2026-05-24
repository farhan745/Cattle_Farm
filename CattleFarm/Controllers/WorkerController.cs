using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
    public class WorkerController : Controller
    {
        private readonly IWorkerService _workerService;
        private readonly IFarmService   _farmService;
        private readonly IAuditService  _auditService;
        private readonly CattleFarmDbContext _db;
        private const int PageSize = 10;

        public WorkerController(IWorkerService workerService, IFarmService farmService, IAuditService auditService, CattleFarmDbContext db)
        { _workerService = workerService; _farmService = farmService; _auditService = auditService; _db = db; }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, string? search = null)
        {
            var (items, total) = await _workerService.GetPagedAsync(page, PageSize, farmId, search);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["FarmId"]      = farmId;
            await LoadFarmsAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var worker = await _workerService.GetByIdAsync(id);
            if (worker is null) return NotFound();
            await SyncSalaryHistoryFromPayrollAsync(worker);

            ViewBag.SalaryHistory = await _db.SalaryHistories
                .Where(s => s.WorkerId == id)
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .Take(12)
                .ToListAsync();
            return View(worker);
        }

        public async Task<IActionResult> Create(int? farmId = null)
        {
            await LoadFarmsAsync();
            return View(new WorkerViewModel { FarmId = farmId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WorkerViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }
            var worker = await _workerService.CreateAsync(vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Hired worker: {worker.FullName}", "Worker", worker.Id);
            TempData["SuccessMessage"] = $"Worker '{worker.FullName}' hired successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var worker = await _workerService.GetByIdAsync(id);
            if (worker is null) return NotFound();
            await LoadFarmsAsync();
            var vm = new WorkerViewModel { Id = worker.Id, FullName = worker.FullName, Role = worker.Role, Phone = worker.Phone, Email = worker.Email, Skills = worker.Skills, ExperienceYears = worker.ExperienceYears, Salary = worker.Salary, IsAvailable = worker.IsAvailable, FarmId = worker.FarmId ?? 0, Notes = worker.Notes, ExistingImagePath = worker.ImagePath };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkerViewModel vm)
        {
            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }
            await _workerService.UpdateAsync(id, vm);
            TempData["SuccessMessage"] = "Worker updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _workerService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Worker record deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadFarmsAsync()
        {
            var userId = GetUserId();
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
            ViewBag.Farms = role == AppRoles.Owner
                ? await _farmService.GetByOwnerAsync(userId)
                : await _farmService.GetAllAsync();
        }

        private async Task SyncSalaryHistoryFromPayrollAsync(Worker worker)
        {
            var payrolls = await _db.Payrolls
                .Where(p => p.WorkerId == worker.Id && !p.IsDeleted)
                .ToListAsync();
            if (!payrolls.Any()) return;

            var histories = await _db.SalaryHistories
                .Where(s => s.WorkerId == worker.Id)
                .ToListAsync();
            var changed = false;

            foreach (var payroll in payrolls)
            {
                var history = histories.FirstOrDefault(s => s.Year == payroll.Year && s.Month == payroll.Month);
                if (history == null)
                {
                    history = new SalaryHistory
                    {
                        FarmId = payroll.FarmId,
                        WorkerId = payroll.WorkerId,
                        WorkerUserId = payroll.UserId > 0 ? payroll.UserId : worker.UserId ?? 0,
                        Year = payroll.Year,
                        Month = payroll.Month,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _db.SalaryHistories.AddAsync(history);
                    histories.Add(history);
                    changed = true;
                }

                var payrollBonus = payroll.Bonus + payroll.OvertimePay;
                var nextBonus = Math.Max(history.Bonus, payrollBonus);
                var nextBaseSalary = payroll.BaseSalary > 0 ? payroll.BaseSalary : worker.Salary;
                var nextTotal = nextBaseSalary + nextBonus;

                if (history.FarmId != payroll.FarmId ||
                    history.WorkerUserId == 0 ||
                    history.BaseSalary != nextBaseSalary ||
                    history.Bonus != nextBonus ||
                    history.TotalSalary != nextTotal)
                {
                    history.FarmId = payroll.FarmId;
                    history.WorkerUserId = payroll.UserId > 0 ? payroll.UserId : worker.UserId ?? 0;
                    history.BaseSalary = nextBaseSalary;
                    history.Bonus = nextBonus;
                    history.TotalSalary = nextTotal;
                    changed = true;
                }
            }

            if (changed)
                await _db.SaveChangesAsync();
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
