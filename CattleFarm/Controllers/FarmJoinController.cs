using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class FarmJoinController : Controller
    {
        private readonly IFarmJoinService _joinService;
        private readonly CattleFarmDbContext _db;

        public FarmJoinController(IFarmJoinService joinService, CattleFarmDbContext db)
        {
            _joinService = joinService;
            _db = db;
        }

        private int UserId()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            return id;
        }

        // ══════════════════════════════════════════════════════════
        //  WORKER SIDE
        // ══════════════════════════════════════════════════════════

        // GET /FarmJoin/Browse
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> Browse()
        {
            var vm = await _joinService.GetBrowseViewModelAsync(UserId());
            ViewBag.JoinMode = "Worker";
            return View(vm);
        }

        // GET /FarmJoin/Apply/{farmId}
        [Authorize(Roles = AppRoles.Worker)]
        public IActionResult Apply(int farmId, string farmName)
        {
            ViewBag.JoinMode = "Worker";
            return View(new FarmJoinApplyViewModel { FarmId = farmId, FarmName = farmName });
        }

        // POST /FarmJoin/Apply
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> Apply(FarmJoinApplyViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, msg) = await _joinService.ApplyAsync(model.FarmId, UserId(), model.Message);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = msg;
            return RedirectToAction(nameof(Browse));
        }

        // GET /FarmJoin/MyRequests
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> MyRequests()
        {
            var list = await _joinService.GetMyRequestsAsync(UserId());
            ViewBag.JoinMode = "Worker";
            return View(list);
        }

        // POST /FarmJoin/Leave
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> Leave(int farmId)
        {
            var ok = await _joinService.LeaveAsync(farmId, UserId());
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "You have left the farm."
                : "Could not leave the farm.";
            return RedirectToAction(nameof(Browse));
        }

        // ══════════════════════════════════════════════════════════
        //  MANAGER SIDE (apply to farm like workers)
        // ══════════════════════════════════════════════════════════

        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> ManagerBrowse()
        {
            var vm = await _joinService.GetManagerBrowseViewModelAsync(UserId());
            ViewBag.JoinMode = "Manager";
            return View("Browse", vm);
        }

        [Authorize(Roles = AppRoles.Manager)]
        public IActionResult ManagerApply(int farmId, string farmName)
        {
            ViewBag.JoinMode = "Manager";
            return View("Apply", new FarmJoinApplyViewModel { FarmId = farmId, FarmName = farmName });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> ManagerApply(FarmJoinApplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.JoinMode = "Manager";
                return View("Apply", model);
            }

            var (success, msg) = await _joinService.ApplyAsync(model.FarmId, UserId(), model.Message);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = msg;
            return RedirectToAction(nameof(ManagerBrowse));
        }

        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> ManagerRequests()
        {
            var list = await _joinService.GetManagerRequestsAsync(UserId());
            ViewBag.JoinMode = "Manager";
            return View("MyRequests", list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Manager)]
        public async Task<IActionResult> LeaveManager(int farmId)
        {
            var ok = await _joinService.LeaveManagerAsync(farmId, UserId());
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "You left the farm."
                : "Could not leave the farm.";
            return RedirectToAction(nameof(ManagerBrowse));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> RemoveManager(int farmManagerId)
        {
            var ok = await _joinService.RemoveManagerAsync(farmManagerId, UserId());
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Manager removed from farm."
                : "Could not remove manager.";
            return RedirectToAction(nameof(Incoming));
        }

        // ══════════════════════════════════════════════════════════
        //  OWNER SIDE
        // ══════════════════════════════════════════════════════════

        // GET /FarmJoin/Incoming
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Incoming()
        {
            var list = await _joinService.GetIncomingAsync(UserId());
            return View(list);
        }

        // POST /FarmJoin/Accept
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Accept(int requestId)
        {
            var (success, msg) = await _joinService.AcceptAsync(requestId, UserId());
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = msg;
            return RedirectToAction(nameof(Incoming));
        }

        // POST /FarmJoin/Reject
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Reject(int requestId, string? note)
        {
            var (success, msg) = await _joinService.RejectAsync(requestId, UserId(), note);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = msg;
            return RedirectToAction(nameof(Incoming));
        }

        // POST /FarmJoin/RemoveWorker
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> RemoveWorker(int farmWorkerId)
        {
            var ok = await _joinService.RemoveWorkerAsync(farmWorkerId, UserId());
            TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok
                ? "Worker removed from farm."
                : "Could not remove worker.";
            return RedirectToAction(nameof(Incoming));
        }

        // ══════════════════════════════════════════════════════════
        //  CREATE LOGIN FOR MANUALLY-ADDED WORKER
        // ══════════════════════════════════════════════════════════

        // GET /FarmJoin/CreateWorkerLogin/{workerId}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> CreateWorkerLogin(int workerId)
        {
            var worker = await _db.Workers
                .Include(w => w.Farm)
                .FirstOrDefaultAsync(w => w.Id == workerId && !w.IsDeleted
                                       && w.Farm!.OwnerId == UserId());

            if (worker == null) return NotFound();

            if (worker.UserId.HasValue)
            {
                TempData["InfoMessage"] = "This worker already has a login account.";
                return RedirectToAction("Index", "Worker");
            }

            var vm = new CreateWorkerLoginViewModel
            {
                WorkerId    = worker.Id,
                WorkerName  = worker.FullName,
                WorkerEmail = worker.Email,
                Email       = worker.Email ?? string.Empty,
                Username    = worker.FullName.ToLower()
                                    .Replace(" ", ".")
                                    .Replace("'", "")
            };
            return View(vm);
        }

        // POST /FarmJoin/CreateWorkerLogin
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> CreateWorkerLogin(CreateWorkerLoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, msg) = await _joinService.CreateWorkerLoginAsync(model, UserId());

            if (!success)
            {
                ModelState.AddModelError(string.Empty, msg);
                return View(model);
            }

            TempData["SuccessMessage"] = msg;
            return RedirectToAction("Index", "Worker");
        }
    }
}
