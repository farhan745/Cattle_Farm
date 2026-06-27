using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using ClosedXML.Excel;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class CattleController : Controller
    {
        private readonly ICattleService _cattleService;
        private readonly IFarmService   _farmService;
        private readonly IFarmAccessService _farmAccess;
        private readonly IAuditService  _auditService;
        private readonly IPaymentGatewayService _paymentService;
        private readonly CattleFarmDbContext     _db;
        private readonly ICurrencyService        _currencyService;
        private readonly IPdfService             _pdfService;
        private const int PageSize = 12;

        public CattleController(
            ICattleService cattleService, 
            IFarmService farmService,
            IFarmAccessService farmAccess,
            IAuditService auditService,
            IPaymentGatewayService paymentService,
            CattleFarmDbContext db,
            ICurrencyService currencyService,
            IPdfService pdfService)
        {
            _cattleService = cattleService;
            _farmAccess = farmAccess;
            _farmService   = farmService;
            _auditService  = auditService;
            _paymentService = paymentService;
            _db = db;
            _currencyService = currencyService;
            _pdfService = pdfService;
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
            ViewBag.CurrentUserId = GetUserId();
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

                TempData["SuccessMessage"] = $"Purchase request for '{cattle.Name}' submitted (Cash)! The farm owner will contact you.";
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

        // ── SELL CATTLE ───────────────────────────────────────────────────────
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> SellCattle(int id)
        {
            var cattle = await _db.Cattles
                .Include(c => c.Farm)
                .Include(c => c.CattleExpenses)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (cattle is null) return NotFound();

            // Only farm owner or Admin can sell
            var userId = GetUserId() ?? 0;
            if (!User.IsInRole(AppRoles.Admin) && cattle.Farm?.OwnerId != userId)
                return Forbid();

            if (cattle.Status == CattleStatus.Sold)
            {
                TempData["ErrorMessage"] = $"'{cattle.Name}' is already sold.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var expenses = cattle.CattleExpenses.Where(e => !e.IsDeleted).ToList();
            var vm = new CattleSellViewModel
            {
                CattleId        = cattle.Id,
                CattleName      = cattle.Name,
                TagId           = cattle.TagId,
                Breed           = cattle.Breed,
                ImagePath       = cattle.ImagePath,
                FarmName        = cattle.Farm?.Name,
                IsListedForSale = cattle.IsListedForSale,
                IsPremiumListing= cattle.IsPremiumListing,
                PurchasePrice   = cattle.PurchasePrice,
                SalePrice       = cattle.SalePrice,
                TotalCostAmount = expenses.Sum(e => e.Amount),
                CattleExpenses  = expenses,
                SaleDate        = DateTime.Today,
                NewExpense      = new CattleExpenseViewModel { CattleId = id, Date = DateTime.Today }
            };
            return View(vm);
        }

        // ── ADD CATTLE EXPENSE (AJAX / form post) ─────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> AddCattleExpense(CattleExpenseViewModel vm, string? returnUrl = null)
        {
            var cattle = await _db.Cattles.Include(c => c.Farm)
                .FirstOrDefaultAsync(c => c.Id == vm.CattleId && !c.IsDeleted);
            if (cattle is null) return NotFound();

            var userId = GetUserId() ?? 0;
            if (!User.IsInRole(AppRoles.Admin) && cattle.Farm?.OwnerId != userId)
                return Forbid();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid expense data. Please check the form.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction(nameof(SellCattle), new { id = vm.CattleId });
            }

            var expense = new CattleExpense
            {
                CattleId          = vm.CattleId,
                Category          = vm.Category,
                Amount            = vm.Amount,
                Date              = vm.Date,
                Description       = vm.Description,
                CreatedByUserId   = userId,
                CreatedAt         = DateTime.UtcNow
            };
            await _db.CattleExpenses.AddAsync(expense);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense added successfully.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(SellCattle), new { id = vm.CattleId });
        }

        // ── DELETE CATTLE EXPENSE ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> DeleteCattleExpense(int id, int cattleId, string? returnUrl = null)
        {
            var expense = await _db.CattleExpenses
                .Include(e => e.Cattle).ThenInclude(c => c!.Farm)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (expense is null) return NotFound();

            var userId = GetUserId() ?? 0;
            if (!User.IsInRole(AppRoles.Admin) && expense.Cattle?.Farm?.OwnerId != userId)
                return Forbid();

            expense.IsDeleted = true;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Expense removed.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(SellCattle), new { id = cattleId });
        }

        // ── LIST FOR SALE (Marketplace) ───────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> ListForSale(int cattleId, decimal salePrice, bool isPremium = false)
        {
            var cattle = await _db.Cattles
                .Include(c => c.Farm)
                .FirstOrDefaultAsync(c => c.Id == cattleId && !c.IsDeleted);

            if (cattle is null) return NotFound();

            var userId = GetUserId() ?? 0;
            if (!User.IsInRole(AppRoles.Admin) && cattle.Farm?.OwnerId != userId)
                return Forbid();

            if (cattle.Status == CattleStatus.Sold)
            {
                TempData["ErrorMessage"] = $"'{cattle.Name}' is already sold.";
                return RedirectToAction(nameof(SellCattle), new { id = cattleId });
            }

            cattle.IsListedForSale  = true;
            cattle.IsPremiumListing = isPremium;
            cattle.SalePrice        = salePrice;
            cattle.UpdatedAt        = DateTime.UtcNow;
            _db.Cattles.Update(cattle);
            await _db.SaveChangesAsync();

            await _auditService.LogActivityAsync(userId,
                $"Listed cattle for sale: {cattle.Name} (Tag:{cattle.TagId}) at {_currencyService.Format(salePrice)}",
                "Cattle", cattle.Id);

            TempData["SuccessMessage"] = $"'{cattle.Name}' has been listed on the Marketplace for {_currencyService.Format(salePrice)}!";
            return RedirectToAction(nameof(SellCattle), new { id = cattleId });
        }

        // ── REMOVE FROM MARKETPLACE ───────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> RemoveFromMarketplace(int cattleId, string? returnUrl = null)
        {
            var cattle = await _db.Cattles
                .Include(c => c.Farm)
                .FirstOrDefaultAsync(c => c.Id == cattleId && !c.IsDeleted);

            if (cattle is null) return NotFound();

            var userId = GetUserId() ?? 0;
            if (!User.IsInRole(AppRoles.Admin) && cattle.Farm?.OwnerId != userId)
                return Forbid();

            cattle.IsListedForSale  = false;
            cattle.IsPremiumListing = false;
            cattle.UpdatedAt        = DateTime.UtcNow;
            _db.Cattles.Update(cattle);
            await _db.SaveChangesAsync();

            await _auditService.LogActivityAsync(userId,
                $"Removed cattle from Marketplace: {cattle.Name} (Tag:{cattle.TagId})",
                "Cattle", cattle.Id);

            TempData["SuccessMessage"] = $"'{cattle.Name}' has been removed from the Marketplace.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(SellCattle), new { id = cattleId });
        }

        // ── CONFIRM SALE ──────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> ConfirmSale(CattleSellViewModel vm)
        {
            var cattle = await _db.Cattles
                .Include(c => c.Farm)
                .Include(c => c.CattleExpenses)
                .FirstOrDefaultAsync(c => c.Id == vm.CattleId && !c.IsDeleted);

            if (cattle is null) return NotFound();

            var userId = GetUserId() ?? 0;
            if (!User.IsInRole(AppRoles.Admin) && cattle.Farm?.OwnerId != userId)
                return Forbid();

            if (cattle.Status == CattleStatus.Sold)
            {
                TempData["ErrorMessage"] = $"'{cattle.Name}' is already sold.";
                return RedirectToAction(nameof(Details), new { id = vm.CattleId });
            }

            // Calculate final sell price
            var totalExpenses = cattle.CattleExpenses
                .Where(e => !e.IsDeleted)
                .Sum(e => e.Amount);

            var desiredProfit = vm.DesiredProfit;
            var sellPrice     = cattle.PurchasePrice + totalExpenses + desiredProfit;

            // Update cattle
            cattle.Status          = CattleStatus.Sold;
            cattle.SalePrice       = sellPrice;
            cattle.SaleDate        = vm.SaleDate;
            cattle.IsListedForSale = false;
            cattle.UpdatedAt       = DateTime.UtcNow;
            _db.Cattles.Update(cattle);

            // Create Revenue record so Financial module shows the sale
            var revenue = new Revenue
            {
                Source         = RevenueSource.BeefSales,
                Amount         = sellPrice,
                Date           = vm.SaleDate,
                FarmId         = cattle.FarmId,
                CreatedByUserId= userId,
                Description    = $"Cattle sold: {cattle.Name} (Tag: {cattle.TagId})" +
                                 (string.IsNullOrWhiteSpace(vm.BuyerName) ? "" : $" — Buyer: {vm.BuyerName}") +
                                 (string.IsNullOrWhiteSpace(vm.Notes) ? "" : $" — Note: {vm.Notes}"),
                CreatedAt      = DateTime.UtcNow
            };
            await _db.Revenues.AddAsync(revenue);
            await _db.SaveChangesAsync();

            await _auditService.LogActivityAsync(userId,
                $"Cattle sold: {cattle.Name} (Tag:{cattle.TagId}) for {_currencyService.Format(sellPrice)} " +
                $"[Buy:{_currencyService.Format(cattle.PurchasePrice)} + Costs:{_currencyService.Format(totalExpenses)} + Profit:{_currencyService.Format(desiredProfit)}]",
                "Cattle", cattle.Id);

            TempData["SuccessMessage"] = $"'{cattle.Name}' has been successfully sold for {_currencyService.Format(sellPrice)}!";
            return RedirectToAction(nameof(Details), new { id = vm.CattleId });
        }

        // ── MARKETPLACE (sale listings) ───────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> Marketplace()
        {
            var listings = await _cattleService.GetListedForSaleAsync();
            // Tell the view which user is logged in so it can hide Buy for own cattle
            ViewBag.CurrentUserId = GetUserId();
            return View(listings);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private async Task LoadFarmsAsync()
        {
            var userId = GetUserId() ?? 0;
            var role = GetUserRole();
            ViewBag.Farms = await _farmAccess.GetAccessibleFarmsAsync(userId, role);
        }

        private string? GetUserRole()
        {
            if (User.IsInRole(AppRoles.Admin)) return AppRoles.Admin;
            if (User.IsInRole(AppRoles.Owner)) return AppRoles.Owner;
            if (User.IsInRole(AppRoles.Manager)) return AppRoles.Manager;
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        private async Task<(bool Allowed, string Message)> ValidateFarmAccessAsync(int farmId, int? existingCattleId = null)
        {
            var farm = await _db.Farms
                .Include(f => f.Cattles)
                .FirstOrDefaultAsync(f => f.Id == farmId && !f.IsDeleted);

            if (farm == null)
                return (false, "Farm was not found.");

            var userId = GetUserId() ?? 0;
            if (!await _farmAccess.CanOperateFarmAsync(farmId, userId, GetUserRole()))
                return (false, "You do not have access to manage cattle on this farm.");

            var cattleCount = farm.Cattles.Count(c => !c.IsDeleted && (!existingCattleId.HasValue || c.Id != existingCattleId.Value));
            if (cattleCount >= farm.MaximumCattle)
                return (false, "This farm has reached its maximum cattle limit.");

            return (true, string.Empty);
        }

        private static CattleViewModel MapToViewModel(Cattle c) => new()
        {
            Id = c.Id, TagId = c.TagId, Name = c.Name, Breed = c.Breed,
            DateOfBirth = c.DateOfBirth, Weight = c.Weight, Gender = c.Gender,
            Category = c.Category, HealthStatus = c.HealthStatus,
            Status = c.Status, FarmId = c.FarmId,
            PurchasePrice = c.PurchasePrice, SalePrice = c.SalePrice,
            SaleDate = c.SaleDate, PurchaseDate = c.PurchaseDate,
            Description = c.Description, Origin = c.Origin,
            IsListedForSale = c.IsListedForSale,
            IsPremiumListing = c.IsPremiumListing, ExistingImagePath = c.ImagePath
        };

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var cattle = await _cattleService.GetWithDetailsAsync(id);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            var pdfBytes = _pdfService.GenerateCattleProfilePdf(cattle);
            var fileName = $"cattle-profile-{cattle.TagId}-{cattle.Name.Replace(" ", "_")}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportProfileExcel(int id)
        {
            var cattle = await _cattleService.GetWithDetailsAsync(id);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Cattle Profile");
            sheet.Cell(1, 1).Value = $"Cattle Profile: {cattle.Name} ({cattle.TagId})";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 14;

            sheet.Cell(3, 1).Value = "Field";
            sheet.Cell(3, 2).Value = "Value";
            sheet.Range(3, 1, 3, 2).Style.Font.Bold = true;

            sheet.Cell(4, 1).Value = "Tag ID";
            sheet.Cell(4, 2).Value = cattle.TagId;

            sheet.Cell(5, 1).Value = "Name";
            sheet.Cell(5, 2).Value = cattle.Name;

            sheet.Cell(6, 1).Value = "Breed";
            sheet.Cell(6, 2).Value = cattle.Breed;

            sheet.Cell(7, 1).Value = "Gender";
            sheet.Cell(7, 2).Value = cattle.Gender.ToString();

            sheet.Cell(8, 1).Value = "Date of Birth";
            sheet.Cell(8, 2).Value = cattle.DateOfBirth.ToString("yyyy-MM-dd");

            sheet.Cell(9, 1).Value = "Weight (kg)";
            sheet.Cell(9, 2).Value = cattle.Weight;

            sheet.Cell(10, 1).Value = "Health Status";
            sheet.Cell(10, 2).Value = cattle.HealthStatus.ToString();

            sheet.Cell(11, 1).Value = "Status";
            sheet.Cell(11, 2).Value = cattle.Status.ToString();

            sheet.Cell(12, 1).Value = "Purchase Price";
            sheet.Cell(12, 2).Value = cattle.PurchasePrice;

            sheet.Cell(13, 1).Value = "Purchase Date";
            sheet.Cell(13, 2).Value = cattle.PurchaseDate?.ToString("yyyy-MM-dd") ?? "N/A";

            sheet.Cell(14, 1).Value = "Sale Price";
            sheet.Cell(14, 2).Value = cattle.SalePrice.HasValue ? cattle.SalePrice.Value.ToString("N2") : "N/A";

            sheet.Cell(15, 1).Value = "Description";
            sheet.Cell(15, 2).Value = cattle.Description ?? "";

            // Add vaccinations if any
            if (cattle.Vaccinations != null && cattle.Vaccinations.Count > 0)
            {
                var rowStart = 17;
                sheet.Cell(rowStart, 1).Value = "Vaccination History";
                sheet.Cell(rowStart, 1).Style.Font.Bold = true;
                sheet.Cell(rowStart, 1).Style.Font.FontSize = 12;

                sheet.Cell(rowStart + 1, 1).Value = "Vaccine Name";
                sheet.Cell(rowStart + 1, 2).Value = "Date Given";
                sheet.Cell(rowStart + 1, 3).Value = "Next Due Date";
                sheet.Range(rowStart + 1, 1, rowStart + 1, 3).Style.Font.Bold = true;

                for (var i = 0; i < cattle.Vaccinations.Count; i++)
                {
                    var v = cattle.Vaccinations.ToList()[i];
                    var r = rowStart + 2 + i;
                    sheet.Cell(r, 1).Value = v.VaccineName;
                    sheet.Cell(r, 2).Value = v.VaccinationDate.ToString("yyyy-MM-dd");
                    sheet.Cell(r, 3).Value = v.NextDueDate?.ToString("yyyy-MM-dd") ?? "";
                }
            }

            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"cattle-profile-{cattle.TagId}-{cattle.Name.Replace(" ", "_")}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(int? farmId = null)
        {
            var userId = GetUserId() ?? 0;
            var role = GetUserRole();
            var farms = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();

            var selectedFarm = farmId.HasValue
                ? farms.FirstOrDefault(f => f.Id == farmId.Value)
                : farms.FirstOrDefault();

            if (selectedFarm == null)
                return NotFound("No accessible farm was found for export.");

            var cattleList = await _db.Cattles
                .Include(c => c.Farm)
                .Where(c => c.FarmId == selectedFarm.Id && !c.IsDeleted)
                .OrderBy(c => c.TagId)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Cattle List");
            sheet.Cell(1, 1).Value = $"Cattle Inventory - {selectedFarm.Name}";
            
            sheet.Cell(3, 1).Value = "Tag ID";
            sheet.Cell(3, 2).Value = "Name";
            sheet.Cell(3, 3).Value = "Breed";
            sheet.Cell(3, 4).Value = "Gender";
            sheet.Cell(3, 5).Value = "Date of Birth";
            sheet.Cell(3, 6).Value = "Weight (kg)";
            sheet.Cell(3, 7).Value = "Health Status";
            sheet.Cell(3, 8).Value = "Status";
            sheet.Cell(3, 9).Value = "Purchase Price";
            sheet.Cell(3, 10).Value = "Sale Price";

            for (var i = 0; i < cattleList.Count; i++)
            {
                var c = cattleList[i];
                var row = 4 + i;
                sheet.Cell(row, 1).Value = c.TagId;
                sheet.Cell(row, 2).Value = c.Name;
                sheet.Cell(row, 3).Value = c.Breed;
                sheet.Cell(row, 4).Value = c.Gender.ToString();
                sheet.Cell(row, 5).Value = c.DateOfBirth.ToString("yyyy-MM-dd");
                sheet.Cell(row, 6).Value = c.Weight;
                sheet.Cell(row, 7).Value = c.HealthStatus.ToString();
                sheet.Cell(row, 8).Value = c.Status.ToString();
                sheet.Cell(row, 9).Value = c.PurchasePrice;
                sheet.Cell(row, 10).Value = c.SalePrice ?? 0m;
            }

            sheet.Range(3, 1, 3, 10).Style.Font.Bold = true;
            sheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var fileName = $"cattle-inventory-{selectedFarm.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ByFarm(int farmId, string? gender = null)
        {
            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(farmId, userId ?? 0, role))
            {
                return Forbid();
            }

            var query = _db.Cattles
                .Where(c => c.FarmId == farmId && !c.IsDeleted);

            if (Enum.TryParse<Gender>(gender, true, out var parsedGender))
            {
                query = query.Where(c => c.Gender == parsedGender);
            }

            var cattle = await query
                .OrderBy(c => c.Name)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.TagId,
                    Gender = c.Gender.ToString()
                })
                .ToListAsync();

            return Json(cattle);
        }

        // ── TRANSFER CATTLE ──────────────────────────────────────────────────
        [HttpGet]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Transfer(int id)
        {
            var cattle = await _cattleService.GetByIdAsync(id);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            var vm = new CattleTransferViewModel
            {
                CattleId = cattle.Id,
                CattleName = cattle.Name,
                TagId = cattle.TagId,
                TransferDate = DateTime.Today
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Transfer(CattleTransferViewModel vm)
        {
            var cattle = await _db.Cattles.FirstOrDefaultAsync(c => c.Id == vm.CattleId && !c.IsDeleted);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            cattle.Status = CattleStatus.Transferred;
            cattle.TransferredTo = vm.TransferredTo;
            cattle.TransferDate = vm.TransferDate;
            cattle.UpdatedAt = DateTime.UtcNow;

            _db.Cattles.Update(cattle);
            await _db.SaveChangesAsync();

            await _auditService.LogActivityAsync(userId ?? 0, $"Transferred cattle: {cattle.Name} (Tag: {cattle.TagId}) to {vm.TransferredTo}", "Cattle", cattle.Id);

            TempData["SuccessMessage"] = $"'{cattle.Name}' has been marked as transferred.";
            return RedirectToAction(nameof(Details), new { id = cattle.Id });
        }

        // ── CATTLE PROFILE REPORT (HTML PRINTABLE) ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> CattleReport(int id)
        {
            var cattle = await _cattleService.GetWithDetailsAsync(id);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            return View(cattle);
        }

        // ── WEIGHT HISTORY (GROWTH CHART) ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> WeightHistory(int? id, int? cattleId)
        {
            id ??= cattleId;
            if (!id.HasValue) return BadRequest("A cattle id is required.");

            var cattle = await _cattleService.GetByIdAsync(id.Value);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            var weightRecords = await _db.WeightRecords
                .Include(w => w.RecordedByUser)
                .Where(w => w.CattleId == id.Value)
                .OrderByDescending(w => w.MeasuredAt)
                .ToListAsync();

            ViewBag.Cattle = cattle;
            return View(weightRecords);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWeight(int cattleId, decimal weightKg, DateTime measuredAt, string? bodyConditionScore, string? notes)
        {
            var cattle = await _db.Cattles.FirstOrDefaultAsync(c => c.Id == cattleId && !c.IsDeleted);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            var record = new WeightRecord
            {
                CattleId = cattleId,
                FarmId = cattle.FarmId,
                WeightKg = weightKg,
                MeasuredAt = measuredAt,
                BodyConditionScore = bodyConditionScore,
                Notes = notes,
                RecordedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.WeightRecords.AddAsync(record);

            cattle.Weight = (double)weightKg;
            cattle.UpdatedAt = DateTime.UtcNow;
            _db.Cattles.Update(cattle);

            await _db.SaveChangesAsync();

            await _auditService.LogActivityAsync(userId ?? 0, $"Added weight record for {cattle.Name} (Tag: {cattle.TagId}): {weightKg} kg", "Cattle", cattle.Id);

            TempData["SuccessMessage"] = "Weight record added successfully.";
            return RedirectToAction(nameof(WeightHistory), new { id = cattleId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> DeleteWeight(int id, int cattleId)
        {
            var record = await _db.WeightRecords.FirstOrDefaultAsync(w => w.Id == id && w.CattleId == cattleId);
            if (record is null) return NotFound();

            var cattle = await _db.Cattles.FirstOrDefaultAsync(c => c.Id == cattleId && !c.IsDeleted);
            if (cattle is null) return NotFound();

            var userId = GetUserId();
            var role = GetUserRole();
            if (!User.IsInRole(AppRoles.Admin) && !await _farmAccess.CanOperateFarmAsync(cattle.FarmId, userId ?? 0, role))
            {
                return Forbid();
            }

            _db.WeightRecords.Remove(record);

            var latestWeight = await _db.WeightRecords
                .Where(w => w.CattleId == cattleId && w.Id != id)
                .OrderByDescending(w => w.MeasuredAt)
                .ThenByDescending(w => w.Id)
                .FirstOrDefaultAsync();

            cattle.Weight = latestWeight != null ? (double)latestWeight.WeightKg : 0.0;
            cattle.UpdatedAt = DateTime.UtcNow;
            _db.Cattles.Update(cattle);

            await _db.SaveChangesAsync();

            await _auditService.LogActivityAsync(userId ?? 0, $"Deleted weight record ID {id} for {cattle.Name} (Tag: {cattle.TagId})", "Cattle", cattle.Id);

            TempData["SuccessMessage"] = "Weight record deleted successfully.";
            return RedirectToAction(nameof(WeightHistory), new { id = cattleId });
        }

        private int? GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var parsed) ? parsed : null;
        }
    }
}
