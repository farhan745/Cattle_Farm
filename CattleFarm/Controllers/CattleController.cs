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
    public class CattleController : Controller
    {
        private readonly ICattleService _cattleService;
        private readonly IFarmService   _farmService;
        private readonly IAuditService  _auditService;
        private readonly IPaymentGatewayService _paymentService;
        private readonly CattleFarmDbContext     _db;
        private readonly ICurrencyService        _currencyService;
        private const int PageSize = 12;

        public CattleController(
            ICattleService cattleService, 
            IFarmService farmService, 
            IAuditService auditService,
            IPaymentGatewayService paymentService,
            CattleFarmDbContext db,
            ICurrencyService currencyService)
        {
            _cattleService = cattleService;
            _farmService   = farmService;
            _auditService  = auditService;
            _paymentService = paymentService;
            _db = db;
            _currencyService = currencyService;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index(int page = 1, string? search = null, int? farmId = null, CattleStatus? status = null)
        {
            var (items, total) = await _cattleService.GetPagedAsync(page, PageSize, search, farmId, status);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)PageSize);
            ViewData["TotalCount"]  = total;
            ViewData["Search"]      = search;
            ViewData["FarmId"]      = farmId;
            ViewData["Status"]      = status;
            var ids = items.Select(c => c.Id).ToList();
            ViewBag.LikeCounts = await _db.CattleLikes
                .Where(l => ids.Contains(l.CattleId))
                .GroupBy(l => l.CattleId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
            ViewBag.CommentCounts = await _db.CattleComments
                .Where(c => ids.Contains(c.CattleId))
                .GroupBy(c => c.CattleId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
            ViewBag.ShareCounts = await _db.CattleShares
                .Where(s => ids.Contains(s.CattleId))
                .GroupBy(s => s.CattleId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
            return View(items);
        }

        // ── DETAILS ───────────────────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var cattle = await _cattleService.GetWithDetailsAsync(id);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            ViewBag.LikeCount = await _db.CattleLikes.CountAsync(l => l.CattleId == id);
            ViewBag.HasLiked = userId.HasValue &&
                await _db.CattleLikes.AnyAsync(l => l.CattleId == id && l.UserId == userId.Value);
            ViewBag.Comments = await _db.CattleComments
                .Where(c => c.CattleId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Take(25)
                .ToListAsync();
            ViewBag.ShareUrl = Url.Action(nameof(Details), "Cattle", new { id }, Request.Scheme);

            return View(cattle);
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(int? farmId = null)
        {
            await LoadFarmsAsync();
            return View(new CattleViewModel { DateOfBirth = DateTime.Today, FarmId = farmId ?? 0 });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(CattleViewModel vm)
        {
            var farmCheck = await ValidateFarmAccessAsync(vm.FarmId);
            if (!farmCheck.Allowed)
                ModelState.AddModelError(nameof(vm.FarmId), farmCheck.Message);

            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }

            var cattle = await _cattleService.CreateAsync(vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Created cattle record: {cattle.Name} (Tag: {cattle.TagId})", "Cattle", cattle.Id);
            TempData["SuccessMessage"] = $"'{cattle.Name}' added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT ──────────────────────────────────────────────────────────────
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var cattle = await _cattleService.GetByIdAsync(id);
            if (cattle is null) return NotFound();
            await LoadFarmsAsync();
            var vm = MapToViewModel(cattle);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Edit(int id, CattleViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            var existing = await _db.Cattles.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null) return NotFound();
            var farmCheck = await ValidateFarmAccessAsync(vm.FarmId, existing.Id);
            if (!farmCheck.Allowed)
                ModelState.AddModelError(nameof(vm.FarmId), farmCheck.Message);

            if (!ModelState.IsValid) { await LoadFarmsAsync(); return View(vm); }
            await _cattleService.UpdateAsync(id, vm);
            await _auditService.LogActivityAsync(GetUserId(), $"Updated cattle record: {vm.Name}", "Cattle", id);
            TempData["SuccessMessage"] = $"'{vm.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            var cattle = await _cattleService.GetByIdAsync(id);
            if (cattle is null) return NotFound();
            return View(cattle);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cattle = await _db.Cattles.Include(c => c.Farm).FirstOrDefaultAsync(c => c.Id == id);
            if (cattle == null) return NotFound();
            if (!User.IsInRole(AppRoles.Admin) && cattle.Farm?.OwnerId != GetUserId())
                return Forbid();

            await _cattleService.DeleteAsync(id);
            await _auditService.LogActivityAsync(GetUserId(), $"Soft-deleted cattle ID {id}", "Cattle", id);
            TempData["SuccessMessage"] = "Cattle record deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Challenge();

            var cattleExists = await _db.Cattles.AnyAsync(c => c.Id == id && !c.IsDeleted);
            if (!cattleExists) return NotFound();

            var like = await _db.CattleLikes.IgnoreQueryFilters()
                .FirstOrDefaultAsync(l => l.CattleId == id && l.UserId == userId.Value);

            if (like == null)
            {
                await _db.CattleLikes.AddAsync(new CattleLike { CattleId = id, UserId = userId.Value });
            }
            else
            {
                like.IsDeleted = !like.IsDeleted;
                like.DeletedAt = like.IsDeleted ? DateTime.UtcNow : null;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, string comment)
        {
            var userId = GetUserId();
            if (!userId.HasValue) return Challenge();

            comment = (comment ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var recentSpam = await _db.CattleComments.AnyAsync(c =>
                c.CattleId == id &&
                c.UserId == userId.Value &&
                c.CreatedAt > DateTime.UtcNow.AddSeconds(-30));
            if (recentSpam)
            {
                TempData["ErrorMessage"] = "Please wait before commenting again.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await _db.CattleComments.AddAsync(new CattleComment
            {
                CattleId = id,
                UserId = userId.Value,
                Comment = comment
            });
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Share(int id, string channel = "Link")
        {
            var cattleExists = await _db.Cattles.AnyAsync(c => c.Id == id && !c.IsDeleted);
            if (!cattleExists) return NotFound();

            var url = Url.Action(nameof(Details), "Cattle", new { id }, Request.Scheme) ?? string.Empty;
            await _db.CattleShares.AddAsync(new CattleShare
            {
                CattleId = id,
                UserId = GetUserId(),
                Channel = string.IsNullOrWhiteSpace(channel) ? "Link" : channel,
                ShareUrl = url
            });
            await _db.SaveChangesAsync();

            return Redirect(url);
        }

        // ── BUY CATTLE ────────────────────────────────────────────────────────
        public async Task<IActionResult> BuyCattle(int id)
        {
            var cattle = await _cattleService.GetWithDetailsAsync(id);
            if (cattle is null) return NotFound();
            if (!cattle.IsListedForSale)
            {
                TempData["ErrorMessage"] = "This cattle is not listed for sale.";
                return RedirectToAction(nameof(Index));
            }
            return View(cattle);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyCattle(int id, string deliveryAddress, string? notes, PaymentMethod paymentMethod)
        {
            var cattle = await _cattleService.GetWithDetailsAsync(id);
            if (cattle is null) return NotFound();
            if (!cattle.IsListedForSale)
            {
                TempData["ErrorMessage"] = "This cattle is no longer for sale.";
                return RedirectToAction(nameof(Index));
            }

            var userId = GetUserId();
            if (!userId.HasValue) return Challenge();

            // Create initial pending payment
            var payment = new Payment
            {
                UserId = userId.Value,
                Amount = cattle.SalePrice ?? 0,
                Method = paymentMethod,
                Status = PaymentStatus.Pending,
                Purpose = PaymentPurpose.CattlePurchase,
                ReferenceId = cattle.Id,
                ReferenceType = "Cattle",
                Notes = $"Cattle purchase request: {cattle.Name}"
            };
            await _db.Payments.AddAsync(payment);
            await _db.SaveChangesAsync();

            if (paymentMethod == PaymentMethod.Cash)
            {
                // Mark as sold immediately for Cash on Delivery
                cattle.IsListedForSale  = false;
                cattle.SaleDate         = DateTime.Today;
                cattle.Status           = CattleStatus.Sold;
                var vm = MapToViewModel(cattle);
                await _cattleService.UpdateAsync(id, vm);

                await _auditService.LogActivityAsync(userId.Value,
                    $"Purchased cattle: {cattle.Name} ({_currencyService.Format(cattle.SalePrice)})", "Cattle", id);

                TempData["SuccessMessage"] = $"🎉 Purchase request for '{cattle.Name}' submitted (Cash)! The farm owner will contact you.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Online payment via SSLCommerz
            var user = await _db.Users.FindAsync(userId.Value);
            var txId = $"CTL-{cattle.Id}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var request = new PaymentInitRequest
            {
                UserId = userId.Value,
                UserName = user?.FullName ?? user?.Username ?? "Customer",
                UserEmail = user?.Email ?? "",
                UserPhone = user?.PhoneNumber ?? "01700000000",
                UserAddress = deliveryAddress,
                PlanName = $"Purchase Cattle: {cattle.Name}",
                Amount = cattle.SalePrice ?? 0,
                Currency = "BDT",
                TransactionId = txId,
                SuccessUrl = $"{baseUrl}/Cattle/PaymentSuccess",
                FailUrl = $"{baseUrl}/Cattle/PaymentFail",
                CancelUrl = $"{baseUrl}/Cattle/PaymentCancel",
                IpnUrl = $"{baseUrl}/Cattle/PaymentIpn"
            };

            var result = await _paymentService.InitiatePaymentAsync(request);

            if (result.Success && !string.IsNullOrEmpty(result.GatewayUrl))
            {
                return Redirect(result.GatewayUrl);
            }

            // Fallback
            cattle.IsListedForSale = false;
            cattle.SaleDate = DateTime.Today;
            cattle.Status = CattleStatus.Sold;
            await _cattleService.UpdateAsync(id, MapToViewModel(cattle));

            TempData["ErrorMessage"] = $"Payment initiation failed: {result.Error}. Falling back to Cash/Pending payment.";
            return RedirectToAction(nameof(Details), new { id = cattle.Id });
        }

        [HttpPost, AllowAnonymous, IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaymentSuccess(
            [FromForm] string? tran_id,
            [FromForm] string? val_id,
            [FromForm] string? amount,
            [FromForm] string? card_type,
            [FromForm] string? status)
        {
            if (string.IsNullOrEmpty(tran_id) || (status != "VALID" && status != "VALIDATED"))
            {
                TempData["ErrorMessage"] = "Cattle payment verification failed.";
                return RedirectToAction(nameof(Marketplace));
            }

            var parts = tran_id.Split('-');
            var cattleId = parts.Length >= 2 && int.TryParse(parts[1], out var id) ? id : 0;
            decimal parsedAmount = decimal.TryParse(amount, out var a) ? a : 0m;

            if (cattleId > 0)
            {
                var cattle = await _cattleService.GetWithDetailsAsync(cattleId);
                if (cattle != null)
                {
                    cattle.IsListedForSale = false;
                    cattle.SaleDate = DateTime.Today;
                    cattle.Status = CattleStatus.Sold;
                    await _cattleService.UpdateAsync(cattleId, MapToViewModel(cattle));

                    var pendingPayment = _db.Payments
                        .FirstOrDefault(p => p.ReferenceId == cattleId && p.ReferenceType == "Cattle" && p.Status == PaymentStatus.Pending);

                    PaymentMethod method = PaymentMethod.BankTransfer;
                    var cLower = card_type?.ToLower() ?? "";
                    if (cLower.Contains("bkash")) method = PaymentMethod.Bkash;
                    else if (cLower.Contains("nagad")) method = PaymentMethod.Nagad;
                    else if (cLower.Contains("visa")) method = PaymentMethod.Visa;
                    else if (cLower.Contains("master")) method = PaymentMethod.MasterCard;

                    if (pendingPayment != null)
                    {
                        pendingPayment.Status = PaymentStatus.Completed;
                        pendingPayment.Method = method;
                        pendingPayment.TransactionId = tran_id;
                        pendingPayment.Amount = parsedAmount;
                        pendingPayment.PaymentDate = DateTime.UtcNow;
                        pendingPayment.Notes = $"Cattle purchase completed via SSLCommerz ({card_type})";
                        _db.Payments.Update(pendingPayment);

                        await _auditService.LogActivityAsync(pendingPayment.UserId,
                            $"Cattle purchase online payment success: {cattle.Name} ({_currencyService.Format(parsedAmount)})", "Cattle", cattleId);
                    }
                }
            }

            ViewBag.TransactionId = tran_id;
            ViewBag.Amount = amount;
            ViewBag.CattleId = cattleId;
            return View();
        }

        [HttpPost, AllowAnonymous, IgnoreAntiforgeryToken]
        public IActionResult PaymentFail([FromForm] string? tran_id, [FromForm] string? error)
        {
            ViewBag.TransactionId = tran_id;
            ViewBag.Error = error ?? "Your payment could not be processed.";
            return View();
        }

        [HttpPost, AllowAnonymous, IgnoreAntiforgeryToken]
        public IActionResult PaymentCancel([FromForm] string? tran_id)
        {
            ViewBag.TransactionId = tran_id;
            return View();
        }

        [HttpPost, AllowAnonymous, IgnoreAntiforgeryToken]
        public IActionResult PaymentIpn() => Ok();

        // ── MARKETPLACE (sale listings) ───────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> Marketplace()
        {
            var listings = await _cattleService.GetListedForSaleAsync();
            return View(listings);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private async Task LoadFarmsAsync()
        {
            var farms = await _farmService.GetAllAsync();
            var userId = GetUserId();
            if (!User.IsInRole(AppRoles.Admin) && userId.HasValue)
                farms = farms.Where(f => f.OwnerId == userId.Value);

            ViewBag.Farms = farms;
        }

        private async Task<(bool Allowed, string Message)> ValidateFarmAccessAsync(int farmId, int? existingCattleId = null)
        {
            var farm = await _db.Farms
                .Include(f => f.Cattles)
                .FirstOrDefaultAsync(f => f.Id == farmId && !f.IsDeleted);

            if (farm == null)
                return (false, "Farm was not found.");

            if (!User.IsInRole(AppRoles.Admin) && farm.OwnerId != GetUserId())
                return (false, "You can only manage cattle for your own farm.");

            var cattleCount = farm.Cattles.Count(c => !c.IsDeleted && (!existingCattleId.HasValue || c.Id != existingCattleId.Value));
            if (cattleCount >= farm.MaximumCattle)
                return (false, "This farm has reached its maximum cattle limit.");

            return (true, string.Empty);
        }

        private static CattleViewModel MapToViewModel(Cattle c) => new()
        {
            Id = c.Id, TagId = c.TagId, Name = c.Name, Breed = c.Breed,
            DateOfBirth = c.DateOfBirth, Weight = c.Weight, Gender = c.Gender,
            HealthStatus = c.HealthStatus, Status = c.Status, FarmId = c.FarmId,
            PurchasePrice = c.PurchasePrice, SalePrice = c.SalePrice,
            SaleDate = c.SaleDate, PurchaseDate = c.PurchaseDate,
            Description = c.Description, IsListedForSale = c.IsListedForSale,
            IsPremiumListing = c.IsPremiumListing, ExistingImagePath = c.ImagePath
        };

        private int? GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var parsed) ? parsed : null;
        }
    }
}
