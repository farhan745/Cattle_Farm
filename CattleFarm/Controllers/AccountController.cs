using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService            _authService;
        private readonly IEmailService           _emailService;
        private readonly IImageService           _imageService;
        private readonly IUserManagementService  _userService;
        private readonly CattleFarmDbContext      _db;
        private readonly IAuditService           _auditService;

        public AccountController(
            IAuthService authService,
            IEmailService emailService,
            IImageService imageService,
            IUserManagementService userService,
            CattleFarmDbContext db,
            IAuditService auditService)
        {
            _authService  = authService;
            _emailService = emailService;
            _imageService = imageService;
            _userService  = userService;
            _db           = db;
            _auditService = auditService;
        }

        // ─── LOGIN ────────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var user = await _authService.LoginAsync(model.Email, model.Password);
            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            // Build claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name,           user.Username),
                new(ClaimTypes.Email,          user.Email),
                new(ClaimTypes.Role,           user.Role),
                new("FullName",                user.FullName),
                new("ProfileImage",            user.ProfileImagePath ?? "")
            };
            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc   = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Dashboard");
        }

        // ─── REGISTER ─────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _authService.EmailExistsAsync(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }
            if (await _authService.UsernameExistsAsync(model.Username))
            {
                ModelState.AddModelError(nameof(model.Username), "This username is already taken.");
                return View(model);
            }

            var success = await _authService.RegisterAsync(model.Username, model.Email, model.Password, model.Role);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(model);
            }

            // Retrieve the newly created user
            var newUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);

            if (newUser != null)
            {
                // Save profile image if provided
                if (model.ProfileImage != null && _imageService.IsValidImage(model.ProfileImage))
                {
                    var imagePath = await _imageService.SaveImageAsync(model.ProfileImage, "avatars");
                    if (imagePath != null)
                    {
                        newUser.ProfileImagePath = imagePath;
                        await _db.SaveChangesAsync();
                    }
                }

                // Auto-create a Worker record so self-registered workers appear in the Worker list
                if (model.Role == AppRoles.Worker)
                {
                    var defaultFarm = await _db.Farms.FirstOrDefaultAsync(f => f.IsActive && !f.IsDeleted);
                    if (defaultFarm != null)
                    {
                        var worker = new Worker
                        {
                            FullName = model.FullName,
                            Role = "Worker",
                            Phone = model.PhoneNumber,
                            Email = model.Email,
                            Salary = 0,
                            IsActive = true,
                            IsAvailable = true,
                            FarmId = defaultFarm.Id,
                            UserId = newUser.Id,
                            HiredAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _db.Workers.AddAsync(worker);
                        await _db.SaveChangesAsync();
                    }
                }
            }

            TempData["SuccessMessage"] = "Account created successfully! You can now log in.";
            return RedirectToAction(nameof(Login));
        }

        // ─── LOGOUT ───────────────────────────────────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public class DeleteAccountRequest
        {
            public string Password { get; set; } = string.Empty;
        }

        [Authorize]
        [HttpDelete("/account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest model)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            if (model == null || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new { success = false, message = "Password is required to delete your account." });
            }

            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.IsDeleted)
            {
                return Json(new { success = false, message = "User not found or already deleted." });
            }

            // Verify Password using BCrypt
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return Json(new { success = false, message = "Incorrect password. Verification failed." });
            }

            var oldValues = $"Username: {user.Username}, Email: {user.Email}";

            // Cascade Soft-Delete to Owned Farms
            var ownedFarms = await _db.Farms.Where(f => f.OwnerId == userId && !f.IsDeleted).ToListAsync();
            foreach (var farm in ownedFarms)
            {
                farm.IsDeleted = true;
                farm.IsActive = false;
                farm.DeletedAt = DateTime.UtcNow;
                farm.UpdatedAt = DateTime.UtcNow;
            }

            // Cascade Soft-Delete to active/pending TransportRequests
            var farmIds = ownedFarms.Select(f => f.Id).ToList();
            var activeStatuses = new[] { TripStatus.Pending, TripStatus.Approved, TripStatus.Assigned, TripStatus.Ongoing };
            var activeRequests = await _db.TransportRequests
                .Where(r => !r.IsDeleted && (r.RequestedByUserId == userId || (r.FarmId != null && farmIds.Contains(r.FarmId.Value))))
                .ToListAsync();

            foreach (var req in activeRequests)
            {
                req.IsDeleted = true;
                if (activeStatuses.Contains(req.Status))
                {
                    req.Status = TripStatus.Cancelled;
                }
                req.UpdatedAt = DateTime.UtcNow;
            }

            // Clean up profile image if exists
            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                try
                {
                    _imageService.DeleteImage(user.ProfileImagePath);
                }
                catch { }
            }

            // Anonymize personal details (GDPR compliance)
            user.Username = $"deleted_user_{user.Id}";
            user.Email = $"deleted_{user.Id}@deleted.cattlefarm.local";
            user.FullName = "Deleted User";
            user.PasswordHash = "DELETED";
            user.PhoneNumber = null;
            user.Address = null;
            user.ProfileImagePath = null;
            
            user.IsActive = false;
            user.IsEmailVerified = false;
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            // Log audit logs
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogAsync(userId, "DELETE_ACCOUNT", "User", userId, oldValues, "Anonymized & Soft-Deleted", ip);
            await _auditService.LogActivityAsync(userId, "Account deleted securely and anonymized (GDPR compliant soft-delete).", "User", userId, ip);

            await _db.SaveChangesAsync();

            // Sign out
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Json(new { success = true, message = "Your account has been deleted successfully.", redirectUrl = Url.Action(nameof(Login)) });
        }

        // ─── PROFILE ──────────────────────────────────────────────────────────

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _db.Users.FindAsync(GetUserId());
            if (user == null) return NotFound();
            ViewBag.User = user;
            return View(user);
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(string fullName, string? phone, string? address)
        {
            var user = await _db.Users.FindAsync(GetUserId());
            if (user == null) return NotFound();

            user.FullName    = fullName;
            user.PhoneNumber = phone;
            user.Address     = address;
            user.UpdatedAt   = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (!_imageService.IsValidImage(avatar))
                return Json(new { success = false, error = "Invalid file. Use JPG, PNG or WEBP, max 5MB." });

            var user = await _db.Users.FindAsync(GetUserId());
            if (user == null) return Json(new { success = false, error = "User not found." });

            // Delete old avatar
            if (!string.IsNullOrEmpty(user.ProfileImagePath))
                _imageService.DeleteImage(user.ProfileImagePath);

            var path = await _imageService.SaveImageAsync(avatar, "avatars");
            if (path == null)
                return Json(new { success = false, error = "Upload failed." });

            user.ProfileImagePath = path;
            user.UpdatedAt        = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Json(new { success = true, url = path });
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && !u.IsDeleted);
            if (user != null)
            {
                var token = Guid.NewGuid().ToString("N");
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _db.SaveChangesAsync();

                var resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink ?? string.Empty);
            }

            // Always display success message for security
            TempData["SuccessMessage"] = "If a matching account was found, a password reset link has been sent to your email address.";
            return RedirectToAction(nameof(Login));
        }

        // GET: Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("A token is required for password reset.");
            }

            return View(new ResetPasswordViewModel { Token = token });
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            if (!ModelState.IsValid) return View(model);

            var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token && !u.IsDeleted);
            if (user == null || !user.PasswordResetTokenExpiry.HasValue || user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired password reset token.");
                return View(model);
            }

            // Hash using BCrypt
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been reset successfully. You can now log in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var p) ? p : 0;
        }
    }
}
