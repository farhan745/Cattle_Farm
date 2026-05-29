using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize(Roles = AppRoles.AdminOrOwner)]
    public class InvitationController : Controller
    {
        private readonly IInvitationService _invitationService;
        private readonly IAuditService _auditService;

        public InvitationController(
            IInvitationService invitationService,
            IAuditService auditService)
        {
            _invitationService = invitationService;
            _auditService = auditService;
        }

        // GET: Invitation
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            const int pageSize = 10;
            var (items, total) = await _invitationService.GetPagedAsync(page, pageSize, search);

            var vm = new DoctorInvitationListVM
            {
                Items = items,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Search = search
            };

            return View(vm);
        }

        // GET: Invitation/Create
        public IActionResult Create() => View(new CreateDoctorInvitationVM());

        // POST: Invitation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDoctorInvitationVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                var invitation = await _invitationService.CreateInvitationAsync(vm, GetUserId(), baseUrl);
                TempData["SuccessMessage"] = $"Invitation sent successfully to Dr. {invitation.DoctorName} ({invitation.Email}).";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return View(vm);
            }
        }

        // POST: Invitation/Resend/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resend(int id)
        {
            try
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
                bool success = await _invitationService.ResendInvitationAsync(id, baseUrl);
                if (success)
                {
                    TempData["SuccessMessage"] = "Invitation resent successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to resend invitation. It may have already been accepted or is in an invalid state.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Invitation/Revoke/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(int id)
        {
            try
            {
                bool success = await _invitationService.RevokeInvitationAsync(id, GetUserId());
                if (success)
                {
                    TempData["SuccessMessage"] = "Invitation revoked successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to revoke invitation. It may have already been accepted.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Invitation/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invitation = await _invitationService.GetByIdAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }

            return View(invitation);
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var p) ? p : 0;
        }
    }
}
