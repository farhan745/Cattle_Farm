using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET: Attendance
        [Authorize(Roles = AppRoles.AdminManagerOrOwner + "," + AppRoles.Worker)]
        public async Task<IActionResult> Index()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            if (User.IsInRole(AppRoles.Worker))
            {
                var workerAttendance = await _attendanceService.GetAttendanceByUserIdAsync(currentUserId);
                return View(workerAttendance);
            }

            var allAttendance = await _attendanceService.GetAllAttendanceAsync(currentUserId, userRole);
            return View(allAttendance);
        }

        // GET: Attendance/Details/{id}
        [Authorize(Roles = AppRoles.AdminManagerOrOwner + "," + AppRoles.Worker)]
        public async Task<IActionResult> Details(int id)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var attendance = await _attendanceService.GetAttendanceByIdAsync(id, currentUserId, userRole);
            if (attendance == null)
            {
                return NotFound();
            }

            if (User.IsInRole(AppRoles.Worker) && attendance.UserId != currentUserId)
            {
                return Forbid();
            }

            return View(attendance);
        }

        // GET: Attendance/Mark
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Mark(DateTime? date)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var targetDate = date ?? DateTime.Today;
            var formModel = await _attendanceService.GetBulkAttendanceFormAsync(targetDate, currentUserId, userRole);
            return View(formModel);
        }

        // POST: Attendance/Mark
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Mark(BulkAttendanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            await _attendanceService.MarkBulkAsync(model, currentUserId, userRole);

            TempData["SuccessMessage"] = $"Attendance successfully marked for {model.Date:yyyy-MM-dd}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Attendance/Edit/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var attendance = await _attendanceService.GetAttendanceByIdAsync(id, currentUserId, userRole);
            if (attendance == null)
            {
                return NotFound();
            }

            var editModel = new AttendanceEditViewModel
            {
                Id = attendance.Id,
                UserId = attendance.UserId,
                WorkerId = attendance.WorkerId,
                Date = attendance.Date,
                Status = attendance.Status,
                Notes = attendance.Notes
            };

            return View(editModel);
        }

        // POST: Attendance/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id, AttendanceEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            try
            {
                await _attendanceService.EditAttendanceAsync(model, currentUserId, userRole);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Attendance/Delete/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var attendance = await _attendanceService.GetAttendanceByIdAsync(id, currentUserId, userRole);
            if (attendance == null)
            {
                return NotFound();
            }

            return View(attendance);
        }

        // POST: Attendance/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            try
            {
                await _attendanceService.DeleteAttendanceAsync(id, currentUserId, userRole);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
