using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;


namespace CattleFarm.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorService _doctorService;
        private readonly IAuditService  _auditService;
        private readonly IInvitationService _invitationService;
        private const int PageSize = 10;

        public DoctorController(
            IDoctorService doctorService,
            IAuditService auditService,
            IInvitationService invitationService)
        {
            _doctorService = doctorService;
            _auditService = auditService;
            _invitationService = invitationService;
        }

        [Authorize]
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var (items, total) = await _doctorService.GetPagedAsync(page, PageSize, search);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            return View(items);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor is null) return NotFound();
            return View(doctor);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public IActionResult Create() => View(new DoctorViewModel());

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(DoctorViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var doctor = await _doctorService.CreateAsync(vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Added doctor: {doctor.FullName}", "Doctor", doctor.Id);
            TempData["SuccessMessage"] = $"Dr. {doctor.FullName} added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var d = await _doctorService.GetByIdAsync(id);
            if (d is null) return NotFound();
            var vm = new DoctorViewModel { Id = d.Id, FullName = d.FullName, Specialization = d.Specialization, Phone = d.Phone, Email = d.Email, LicenseNumber = d.LicenseNumber, ConsultationFee = d.ConsultationFee, IsAvailable = d.IsAvailable, Notes = d.Notes, ExistingImagePath = d.ImagePath };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, DoctorViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            await _doctorService.UpdateAsync(id, vm);
            TempData["SuccessMessage"] = "Doctor profile updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            await _doctorService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Doctor record deleted.";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult RegisterVeterinarian()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View(new DoctorSelfRegisterVM());
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterVeterinarian(DoctorSelfRegisterVM vm)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid) return View(vm);

            try
            {
                var (user, doctor) = await _doctorService.SelfRegisterAsync(vm);
                await SignInUserAsync(user);
                TempData["SuccessMessage"] = $"Welcome, Dr. {doctor.FullName}! Your veterinarian profile is ready.";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CompleteProfile(string? token)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(CompleteProfile), new { token });
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.InvitationError = "This invitation link is missing a security token. Please use the full link from your email, or ask the farm owner to resend the invitation.";
                return View(new CompleteDoctorProfileVM());
            }

            var invite = await _invitationService.GetByTokenAsync(token);
            if (invite == null)
            {
                ViewBag.InvitationError = "Invitation not found. The link may be incorrect — please check the email or request a new invitation.";
                return View(new CompleteDoctorProfileVM { Token = token });
            }

            if (!await _invitationService.ValidateTokenAsync(token))
            {
                var status = invite.InvitationStatus.ToString();
                ViewBag.InvitationError = invite.IsUsed || invite.InvitationStatus == InvitationStatus.Accepted
                    ? "This invitation has already been used. You can log in if you already registered, or ask for a new invitation."
                    : invite.InvitationStatus == InvitationStatus.Revoked
                        ? "This invitation was revoked. Please contact the farm owner for a new link."
                        : invite.ExpiresAt < DateTime.UtcNow
                            ? "This invitation has expired. Please ask the farm owner to send a new invitation."
                            : $"This invitation cannot be used (status: {status}).";
                return View(new CompleteDoctorProfileVM
                {
                    Token = token,
                    FullName = invite.DoctorName,
                    Email = invite.Email,
                    PhoneNumber = invite.PhoneNumber ?? string.Empty
                });
            }

            var vm = new CompleteDoctorProfileVM
            {
                Token = token,
                FullName = invite.DoctorName,
                Email = invite.Email,
                PhoneNumber = invite.PhoneNumber ?? string.Empty
            };

            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CompleteProfile(CompleteDoctorProfileVM vm)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(CompleteProfile), new { token = vm.Token });
            }

            if (string.IsNullOrWhiteSpace(vm.Token))
            {
                ViewBag.InvitationError = "Missing invitation token. Please open the full link from your email.";
                return View(vm);
            }

            DoctorInvitation? invite = await _invitationService.GetByTokenAsync(vm.Token);
            if (invite == null)
            {
                ViewBag.InvitationError = "Invitation not found.";
                return View(vm);
            }

            if (!await _invitationService.ValidateTokenAsync(vm.Token))
            {
                ViewBag.InvitationError = "This invitation is invalid, expired, or has already been used.";
                vm.FullName = invite.DoctorName;
                vm.Email = invite.Email;
                vm.PhoneNumber = invite.PhoneNumber ?? string.Empty;
                return View(vm);
            }

            if (vm.LicenseDocument == null || vm.LicenseDocument.Length == 0)
                ModelState.AddModelError("LicenseDocument", "Please upload a valid copy of your veterinary license document.");

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                await _doctorService.CompleteProfileAsync(vm, invite);
                TempData["SuccessMessage"] = "Profile registration complete! Please log in with your credentials.";
                return RedirectToAction("Login", "Account");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                return View(vm);
            }
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("FullName", user.FullName),
                new("ProfileImage", user.ProfileImagePath ?? "")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
