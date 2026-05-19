using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public AccountController(
            IAuthService authService,
            IEmailService emailService,
            IImageService imageService,
            IUserManagementService userService,
            CattleFarmDbContext db)
        {
            _authService  = authService;
            _emailService = emailService;
            _imageService = imageService;
            _userService  = userService;
            _db           = db;
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

            var success = await _authService.RegisterAsync(model.Username, model.Email, model.Password);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return View(model);
            }

            // Save profile image if provided
            if (model.ProfileImage != null && _imageService.IsValidImage(model.ProfileImage))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    var imagePath = await _imageService.SaveImageAsync(model.ProfileImage, "avatars");
                    if (imagePath != null)
                    {
                        user.ProfileImagePath = imagePath;
                        await _db.SaveChangesAsync();
                    }
                }
            }

            // Send OTP for email verification
            await SendOtpAsync(model.Email, "EmailVerify");

            TempData["PendingEmail"] = model.Email;
            TempData["PendingName"]  = model.Username;
            TempData["SuccessMessage"] = "Account created! Check your email for a verification code.";
            return RedirectToAction(nameof(VerifyOtp), new { email = model.Email, purpose = "EmailVerify" });
        }

        // ─── LOGOUT ───────────────────────────────────────────────────────────

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        // ─── FORGOT PASSWORD ──────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("email", "Email is required.");
                return View();
            }

            var exists = await _authService.EmailExistsAsync(email);
            if (!exists)
            {
                // Security: don't reveal if email exists
                TempData["SuccessMessage"] = $"If {email} is registered, an OTP has been sent.";
                return RedirectToAction(nameof(VerifyOtp), new { email, purpose = "PasswordReset" });
            }

            await SendOtpAsync(email, "PasswordReset");
            TempData["SuccessMessage"] = $"OTP sent to {email}. Check your inbox.";
            return RedirectToAction(nameof(VerifyOtp), new { email, purpose = "PasswordReset" });
        }

        // ─── VERIFY OTP ───────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult VerifyOtp(string email, string purpose = "PasswordReset")
        {
            ViewBag.Email   = email;
            ViewBag.Purpose = purpose;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string email, string otp, string purpose)
        {
            ViewBag.Email   = email;
            ViewBag.Purpose = purpose;

            var otpRecord = await _db.OtpCodes
                .Where(o => o.Email == email && o.Purpose == purpose && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null || otpRecord.Code != otp.Trim())
            {
                ViewBag.Error = "Incorrect code. Please check and try again.";
                return View();
            }
            if (otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                ViewBag.Error = "This code has expired. Request a new one.";
                return View();
            }

            // Mark as used
            otpRecord.IsUsed = true;
            await _db.SaveChangesAsync();

            if (purpose == "EmailVerify")
            {
                // Activate account
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    user.IsEmailVerified = true;
                    await _db.SaveChangesAsync();
                    try { await _emailService.SendWelcomeEmailAsync(email, user.Username); } catch { }
                }
                TempData["SuccessMessage"] = "Email verified! You can now log in.";
                return RedirectToAction(nameof(Login));
            }

            // Password reset — generate one-time token
            var resetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            TempData["ResetToken"] = resetToken;
            TempData["ResetEmail"] = email;
            return RedirectToAction(nameof(ResetPassword), new { email, token = resetToken });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email, string purpose)
        {
            await SendOtpAsync(email, purpose);
            TempData["SuccessMessage"] = "New OTP sent to your email.";
            return RedirectToAction(nameof(VerifyOtp), new { email, purpose });
        }

        // ─── RESET PASSWORD ───────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (TempData["ResetToken"]?.ToString() != token)
                return RedirectToAction(nameof(ForgotPassword));
            TempData.Keep("ResetToken");
            TempData.Keep("ResetEmail");
            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string token, string newPassword, string confirmPassword)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }
            if (newPassword.Length < 8)
            {
                ViewBag.Error = "Password must be at least 8 characters.";
                return View();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
            user.UpdatedAt    = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // Invalidate all OTPs for this user
            var otps = _db.OtpCodes.Where(o => o.Email == email);
            await otps.ExecuteUpdateAsync(s => s.SetProperty(o => o.IsUsed, true));

            TempData["SuccessMessage"] = "Password reset successfully! You can now log in.";
            return RedirectToAction(nameof(ResetSuccess));
        }

        [HttpGet]
        public IActionResult ResetSuccess()
        {
            return View();
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

        // ─── Helpers ──────────────────────────────────────────────────────────

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var p) ? p : 0;
        }

        private async Task SendOtpAsync(string email, string purpose)
        {
            var code = new Random().Next(100000, 999999).ToString();

            // Invalidate old OTPs
            var old = _db.OtpCodes.Where(o => o.Email == email && o.Purpose == purpose && !o.IsUsed);
            await old.ExecuteUpdateAsync(s => s.SetProperty(o => o.IsUsed, true));

            _db.OtpCodes.Add(new OtpCode
            {
                Email     = email,
                Code      = code,
                Purpose   = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });
            await _db.SaveChangesAsync();

            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);
            var name = user?.Username ?? email.Split('@')[0];

            try { await _emailService.SendOtpEmailAsync(email, name, code, purpose); }
            catch (Exception ex) { /* log only — don't crash on email errors */ _ = ex; }
        }
    }
}
