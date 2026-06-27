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
        private readonly IAuditService _auditService;
        private const int PageSize = 10;

        public DoctorController(IDoctorService doctorService, IAuditService auditService)
        {
            _doctorService = doctorService;
            _auditService = auditService;
        }

        /// <summary>Global veterinarian directory — visible to all signed-in users.</summary>
        [Authorize]
        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            var (items, total) = await _doctorService.GetPagedAsync(page, PageSize, search);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["Search"] = search;
            return View(items);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor is null) return NotFound();
            return View(doctor);
        }

        [Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> Edit()
        {
            var doctor = await _doctorService.GetByUserIdAsync(GetUserId());
            if (doctor is null) return NotFound();
            return View(MapToViewModel(doctor));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> Edit(DoctorViewModel vm)
        {
            var doctor = await _doctorService.GetByUserIdAsync(GetUserId());
            if (doctor is null) return NotFound();

            if (!ModelState.IsValid) return View(vm);

            await _doctorService.UpdateAsync(doctor.Id, vm);
            TempData["SuccessMessage"] = "Your veterinarian profile was updated.";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult RegisterVeterinarian()
            => RedirectToAction("Register", "Account", new { role = AppRoles.Doctor });

        [AllowAnonymous]
        public IActionResult CompleteProfile(string? token)
            => RedirectToAction("Register", "Account", new { role = AppRoles.Doctor });

        private static DoctorViewModel MapToViewModel(Doctor d) => new()
        {
            Id = d.Id,
            FullName = d.FullName,
            Specialization = d.Specialization,
            Phone = d.Phone,
            Email = d.Email,
            LicenseNumber = d.LicenseNumber,
            ConsultationFee = d.ConsultationFee,
            IsAvailable = d.IsAvailable,
            Notes = d.Notes,
            ExistingImagePath = d.ImagePath
        };

        private Task SignInUserAsync(User user)
            => CattleFarm.Authorization.UserClaimsHelper.SignInAsync(HttpContext, user);

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var p) ? p : 0;
        }
    }
}
