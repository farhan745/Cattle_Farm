using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CattleFarm.Models;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly CattleFarmDbContext _context;
        private readonly IFarmService _farmService;

        public EmployeeController(IEmployeeService employeeService, CattleFarmDbContext context, IFarmService farmService)
        {
            _employeeService = employeeService;
            _context = context;
            _farmService = farmService;
        }

        // GET: Employee
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return View(employees);
        }

        // GET: Employee/Details/{id}
        [Authorize(Roles = AppRoles.AdminManagerOrOwner + "," + AppRoles.Worker)]
        public async Task<IActionResult> Details(int id)
        {
            // Extract logged-in User ID (int)
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);

            // Ownership check: Workers can only view their own profile
            if (User.IsInRole(AppRoles.Worker) && id != currentUserId)
            {
                return Forbid();
            }

            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Fetch the worker associated with this employee user ID
            var worker = await _context.Workers
                .Include(w => w.Farm)
                .FirstOrDefaultAsync(w => w.UserId == id && !w.IsDeleted);

            if (worker != null)
            {
                // Fetch the recent daily attendance records for this worker
                var dailyAttendances = await _context.Attendances
                    .Include(a => a.MarkedByUser)
                    .Where(a => a.WorkerId == worker.Id)
                    .OrderByDescending(a => a.Date)
                    .Take(10)
                    .ToListAsync();

                ViewBag.DailyAttendances = dailyAttendances;
                ViewBag.Worker = worker;
            }

            return View(employee);
        }

        // GET: Employee/Create
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Create()
        {
            await LoadFarmsDropdownAsync();
            return View(new EmployeeCreateViewModel());
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFarmsDropdownAsync();
                return View(model);
            }

            // If Owner, force FarmId to one of their own farms
            if (User.IsInRole(AppRoles.Owner) && !model.FarmId.HasValue)
            {
                int currentUserId = GetCurrentUserId();
                var ownerFarms = await _farmService.GetByOwnerAsync(currentUserId);
                model.FarmId = ownerFarms.FirstOrDefault()?.Id;
            }

            await _employeeService.CreateEmployeeAsync(model);
            TempData["SuccessMessage"] = $"Employee '{model.FullName}' hired successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Employee/Edit/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var editModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                Department = employee.Department,
                Designation = employee.Designation,
                BaseSalary = employee.BaseSalary,
                IsActive = employee.IsActive,
                Role = employee.Role
            };

            return View(editModel);
        }

        // POST: Employee/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _employeeService.UpdateEmployeeAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: Employee/Delete/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employee/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task LoadFarmsDropdownAsync()
        {
            int userId = GetCurrentUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            ViewBag.Farms = role == AppRoles.Owner
                ? await _farmService.GetByOwnerAsync(userId)
                : await _farmService.GetAllAsync();
        }

        private int GetCurrentUserId()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            return id;
        }
    }
}
