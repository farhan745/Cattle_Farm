using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly IUserManagementService _userService;
        private readonly IFarmService           _farmService;
        private readonly ICattleService         _cattleService;
        private readonly IWorkerService         _workerService;
        private readonly IDoctorService         _doctorService;
        private readonly ISubscriptionService   _subscriptionService;
        private readonly IAuditService          _auditService;
        private readonly INotificationService   _notificationService;
        private readonly ICurrencyService       _currencyService;

        public AdminController(IUserManagementService user, IFarmService farm, ICattleService cattle,
            IWorkerService worker, IDoctorService doctor, ISubscriptionService subscription,
            IAuditService audit, INotificationService notification, ICurrencyService currency)
        {
            _userService = user; _farmService = farm; _cattleService = cattle;
            _workerService = worker; _doctorService = doctor; _subscriptionService = subscription;
            _auditService = audit; _notificationService = notification;
            _currencyService = currency;
        }

        public IActionResult Index() => RedirectToAction(nameof(Users));


        // ── Users ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Users(int page = 1, string? search = null, string? role = null)
        {
            var users = await _userService.GetAllUsersAsync();
            if (!string.IsNullOrWhiteSpace(search))
                users = users.Where(u => u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                         u.Username.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(role))
                users = users.Where(u => u.Role == role);
            var list   = users.ToList();
            const int pageSize = 20;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(list.Count / (double)pageSize);
            ViewData["Search"]      = search;
            ViewData["Role"]        = role;
            ViewBag.AvailableRoles  = AppRoles.All;
            ViewBag.CurrentUserId   = GetUserId();
            return View(list.Skip((page-1)*pageSize).Take(pageSize));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            var success = await _userService.ChangeRoleAsync(model.UserId, model.NewRole, GetUserId());
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success ? $"Role updated to '{model.NewRole}'." : "Failed to update role.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user is null) return NotFound();
            user.IsActive = !user.IsActive;
            await _userService.UpdateAsync(user);
            TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")}."; 
            return RedirectToAction(nameof(Users));
        }

        // ── Farms ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Farms()
        {
            var farms = await _farmService.GetAllAsync();
            return View(farms);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFarm(int id) { await _farmService.ApproveAsync(id); TempData["SuccessMessage"] = "Farm approved."; return RedirectToAction(nameof(Farms)); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFarm(int id) { await _farmService.RejectAsync(id); TempData["SuccessMessage"] = "Farm rejected."; return RedirectToAction(nameof(Farms)); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFarm(int id) { await _farmService.DeleteAsync(id); TempData["SuccessMessage"] = "Farm deleted."; return RedirectToAction(nameof(Farms)); }

        // ── Subscriptions ─────────────────────────────────────────────────────
        public async Task<IActionResult> Subscriptions(int page = 1)
        {
            var (items, total) = await _subscriptionService.GetPagedAsync(page, 20);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)20);
            return View(items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeSubscription(int id)
        {
            await _subscriptionService.RevokeAsync(id);
            TempData["SuccessMessage"] = "Subscription revoked.";
            return RedirectToAction(nameof(Subscriptions));
        }

        // ── Audit Logs ────────────────────────────────────────────────────────
        public async Task<IActionResult> AuditLogs(int page = 1, string? entity = null)
        {
            var (items, total) = await _auditService.GetPagedAuditLogsAsync(page, 20, entity);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)20);
            ViewData["Entity"]      = entity;
            return View(items);
        }

        // ── Activity ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Activity()
        {
            var activity = await _auditService.GetRecentActivityAsync(50);
            return View(activity);
        }

        // ── Broadcast Notification ────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BroadcastNotification(string role, string title, string message)
        {
            await _notificationService.SendToRoleAsync(role, title, message, Models.NotificationType.System);
            TempData["SuccessMessage"] = $"Notification sent to all {role} users.";
            return RedirectToAction(nameof(Index));
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
