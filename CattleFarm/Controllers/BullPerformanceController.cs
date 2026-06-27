using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class BullPerformanceController : Controller
    {
        private readonly CattleFarmDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;
        private readonly IFarmAccessService _farmAccess;
        private const int PageSize = 15;

        public BullPerformanceController(CattleFarmDbContext db, IUnitOfWork uow, IAuditService audit, IFarmAccessService farmAccess)
        {
            _db = db;
            _uow = uow;
            _audit = audit;
            _farmAccess = farmAccess;
        }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            var accessibleFarms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            if (farmId.HasValue && !accessibleFarms.Any(f => f.Id == farmId.Value))
                return Forbid();

            var query = _db.BullPerformanceRecords
                .Include(b => b.Cattle)
                .Include(b => b.Farm)
                .AsQueryable();

            if (farmId.HasValue)
            {
                query = query.Where(b => b.FarmId == farmId.Value);
            }
            else
            {
                var farmIds = accessibleFarms.Select(f => f.Id).ToList();
                query = query.Where(b => farmIds.Contains(b.FarmId));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(b => b.EvaluationDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["TotalCount"] = total;
            ViewData["FarmId"] = farmId;
            ViewBag.Farms = accessibleFarms;

            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var record = await _db.BullPerformanceRecords
                .Include(b => b.Cattle)
                .Include(b => b.Farm)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (record is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!await _farmAccess.CanOperateFarmAsync(record.FarmId, userId, role))
                return Forbid();

            return View(record);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(int? farmId = null, int? cattleId = null)
        {
            var userId = GetUserId();
            var role = GetUserRole();

            if (farmId.HasValue && !await _farmAccess.CanOperateFarmAsync(farmId.Value, userId, role))
                return Forbid();

            await LoadDropdowns(farmId, userId, role);
            return View(new BullPerformanceRecord { EvaluationDate = DateTime.Today, FarmId = farmId ?? 0, CattleId = cattleId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(BullPerformanceRecord record)
        {
            var userId = GetUserId();
            var role = GetUserRole();

            if (!await _farmAccess.CanOperateFarmAsync(record.FarmId, userId, role))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(record.FarmId, userId, role);
                return View(record);
            }

            record.CreatedAt = DateTime.UtcNow;
            await _db.BullPerformanceRecords.AddAsync(record);
            await _db.SaveChangesAsync();

            await _audit.LogActivityAsync(userId, $"Recorded semen quality evaluation for bull ID {record.CattleId}", "BullPerformanceRecord", record.Id);
            TempData["SuccessMessage"] = "Bull performance evaluation record created.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var record = await _db.BullPerformanceRecords.FindAsync(id);
            if (record is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!await _farmAccess.CanOperateFarmAsync(record.FarmId, userId, role))
                return Forbid();

            await LoadDropdowns(record.FarmId, userId, role);
            return View("Create", record);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, BullPerformanceRecord record)
        {
            if (id != record.Id) return BadRequest();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!await _farmAccess.CanOperateFarmAsync(record.FarmId, userId, role))
                return Forbid();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(record.FarmId, userId, role);
                return View(record);
            }

            _db.BullPerformanceRecords.Update(record);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bull performance evaluation record updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _db.BullPerformanceRecords.FindAsync(id);
            if (record is null)
            {
                TempData["SuccessMessage"] = "Bull performance record deleted.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetUserId();
            var role = GetUserRole();
            if (!await _farmAccess.CanOperateFarmAsync(record.FarmId, userId, role))
                return Forbid();

            _db.BullPerformanceRecords.Remove(record);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bull performance record deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? farmId, int userId, string? role)
        {
            var farms = await _farmAccess.GetAccessibleFarmsAsync(userId, role);
            ViewBag.Farms = farms;

            if (farmId.HasValue)
            {
                ViewBag.Cattle = await _db.Cattles
                    .Where(c => c.FarmId == farmId.Value && !c.IsDeleted && c.Gender == Gender.Male)
                    .ToListAsync();
            }
            else
            {
                var farmIds = farms.Select(f => f.Id).ToList();
                ViewBag.Cattle = await _db.Cattles
                    .Where(c => farmIds.Contains(c.FarmId) && !c.IsDeleted && c.Gender == Gender.Male)
                    .ToListAsync();
            }
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
