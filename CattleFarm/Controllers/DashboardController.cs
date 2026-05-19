using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboard;
        private readonly IFarmService _farmService;
        private readonly INotificationService _notificationService;

        public DashboardController(IDashboardService dashboard, IFarmService farmService, INotificationService notificationService)
        {
            _dashboard = dashboard;
            _farmService = farmService;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(int? farmId = null)
        {
            var userId = GetUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(userId);

            return role switch
            {
                AppRoles.Admin   => View("Admin",    await _dashboard.GetAdminDashboardAsync()),
                AppRoles.Owner   => await OwnerDashboard(userId, farmId),
                AppRoles.Manager => await OwnerDashboard(userId, farmId),
                AppRoles.Worker  => View("Worker",   await _dashboard.GetWorkerDashboardAsync(userId)),
                AppRoles.Doctor  => View("Doctor",   await _dashboard.GetDoctorDashboardAsync(userId)),
                _                => View("Customer", await _dashboard.GetCustomerDashboardAsync(userId))
            };
        }

        private async Task<IActionResult> OwnerDashboard(int userId, int? farmId)
        {
            var farms = (await _farmService.GetByOwnerAsync(userId)).ToList();
            if (!farms.Any()) return View("NoFarms");
            if (farms.Count == 1 || farmId.HasValue)
            {
                var vm = await _dashboard.GetOwnerDashboardAsync(userId, farmId ?? farms.First().Id);
                return View("Owner", vm);
            }
            // Multiple farms → show farm selector
            return View("FarmSelector", farms);
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var parsed) ? parsed : 0;
        }
    }
}
