using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    /// <summary>
    /// Admin-only controller for viewing all users and changing their roles.
    /// Regular users cannot access any action here.
    /// </summary>
    [Authorize(Roles = AppRoles.Admin)]
    public class UserManagementController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public UserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        // ─── INDEX — list all users ───────────────────────────────────────────

        /// <summary>GET /UserManagement — shows all registered users.</summary>
        public async Task<IActionResult> Index()
        {
            var users = await _userManagementService.GetAllUsersAsync();

            // Pass the available roles to the view for the dropdown
            ViewBag.AvailableRoles = AppRoles.All;

            // Pass the current admin's id so the view can disable their own row
            ViewBag.CurrentUserId = GetCurrentUserId();

            return View(users);
        }

        // ─── CHANGE ROLE ──────────────────────────────────────────────────────

        /// <summary>
        /// POST /UserManagement/ChangeRole
        /// Changes the role of the target user.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(ChangeRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid request.";
                return RedirectToAction(nameof(Index));
            }

            var callerUserId = GetCurrentUserId();

            if (callerUserId == model.UserId)
            {
                TempData["ErrorMessage"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _userManagementService.ChangeRoleAsync(
                model.UserId, model.NewRole, callerUserId);

            if (success)
                TempData["SuccessMessage"] = $"Role updated to '{model.NewRole}' successfully.";
            else
                TempData["ErrorMessage"] = "Failed to update role. The role may be invalid.";

            return RedirectToAction(nameof(Index));
        }

        // ─── Helper ───────────────────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }
    }
}
