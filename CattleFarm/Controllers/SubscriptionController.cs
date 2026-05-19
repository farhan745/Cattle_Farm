using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        private readonly ISubscriptionService   _subscriptionService;
        private readonly IPaymentGatewayService _paymentService;
        private readonly IEmailService          _emailService;
        private readonly CattleFarmDbContext     _db;
        private readonly IConfiguration         _config;

        public SubscriptionController(
            ISubscriptionService sub,
            IPaymentGatewayService payment,
            IEmailService email,
            CattleFarmDbContext db,
            IConfiguration config)
        {
            _subscriptionService = sub;
            _paymentService      = payment;
            _emailService        = email;
            _db                  = db;
            _config              = config;
        }

        // ─── INDEX: Show plans ────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var sub = await _subscriptionService.GetActiveAsync(GetUserId());
            return View(sub);
        }

        // ─── INITIATE PAYMENT via SSLCommerz ─────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> InitiatePayment(string plan)
        {
            if (!Enum.TryParse<SubscriptionPlan>(plan, true, out var parsedPlan) || parsedPlan == SubscriptionPlan.Free)
            {
                TempData["SuccessMessage"] = "You are already on the Free plan!";
                return RedirectToAction(nameof(Index));
            }

            decimal price = parsedPlan switch
            {
                SubscriptionPlan.Member     => 999m,
                SubscriptionPlan.Owner      => 2999m,
                SubscriptionPlan.Enterprise => 5000m,
                _                           => 0m
            };

            string displayName = parsedPlan switch
            {
                SubscriptionPlan.Member     => "Basic",
                SubscriptionPlan.Owner      => "Pro",
                SubscriptionPlan.Enterprise => "Enterprise",
                _                           => parsedPlan.ToString()
            };

            var userId    = GetUserId();
            var user      = await _db.Users.FindAsync(userId);
            var txId      = $"CF-{userId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var baseUrl   = $"{Request.Scheme}://{Request.Host}";

            // Store intent in TempData for post-payment restoration
            TempData["PendingPlan"]  = plan;
            TempData["PendingTxId"]  = txId;
            TempData["PendingPrice"] = price.ToString();

            var request = new PaymentInitRequest
            {
                UserId       = userId,
                UserName     = user?.FullName  ?? user?.Username ?? "Customer",
                UserEmail    = user?.Email     ?? "",
                UserPhone    = user?.PhoneNumber ?? "01700000000",
                UserAddress  = user?.Address   ?? "Dhaka, Bangladesh",
                PlanName     = displayName,
                Amount       = price,
                Currency     = "BDT",
                TransactionId = txId,
                SuccessUrl   = $"{baseUrl}/Subscription/PaymentSuccess",
                FailUrl      = $"{baseUrl}/Subscription/PaymentFail",
                CancelUrl    = $"{baseUrl}/Subscription/PaymentCancel",
                IpnUrl       = $"{baseUrl}/Subscription/PaymentIpn"
            };

            var result = await _paymentService.InitiatePaymentAsync(request);

            if (result.Success && !string.IsNullOrEmpty(result.GatewayUrl))
            {
                TempData["SessionKey"] = result.SessionKey;
                return Redirect(result.GatewayUrl);
            }

            TempData["ErrorMessage"] = $"Payment initiation failed: {result.Error}";
            return RedirectToAction(nameof(Index));
        }

        // ─── PAYMENT CALLBACKS ────────────────────────────────────────────────

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess(
            [FromForm] string? tran_id,
            [FromForm] string? val_id,
            [FromForm] string? amount,
            [FromForm] string? card_type,
            [FromForm] string? store_amount,
            [FromForm] string? status)
        {
            if (string.IsNullOrEmpty(tran_id) || status != "VALID" && status != "VALIDATED")
            {
                TempData["ErrorMessage"] = "Payment verification failed.";
                return RedirectToAction(nameof(PaymentFail));
            }

            // Parse userId from transaction id: CF-{userId}-{timestamp}
            var parts  = tran_id.Split('-');
            var userId = parts.Length >= 2 && int.TryParse(parts[1], out var id) ? id : 0;

            // Determine plan from DB pending — fallback to Member
            SubscriptionPlan planEnum = SubscriptionPlan.Member;
            decimal price = 999m;
            string displayName = "Basic";

            if (userId > 0)
            {
                // Find and create subscription
                var txRef = $"SSLCommerz-{tran_id}";
                var sub   = await _subscriptionService.CreateAsync(userId, planEnum, price, txRef);

                // Store payment record
                _db.Payments.Add(new Payment
                {
                    UserId        = userId,
                    Amount        = price,
                    Method        = card_type?.ToLower().Contains("bkash") == true ? PaymentMethod.Bkash :
                                    card_type?.ToLower().Contains("nagad") == true ? PaymentMethod.Nagad :
                                    card_type?.ToLower().Contains("visa")  == true ? PaymentMethod.Visa  :
                                    PaymentMethod.MasterCard,
                    Status        = PaymentStatus.Completed,
                    Purpose       = PaymentPurpose.Subscription,
                    TransactionId = tran_id,
                    ReferenceId   = sub.Id,
                    ReferenceType = "Subscription",
                    Notes         = $"{displayName} plan — SSLCommerz"
                });
                await _db.SaveChangesAsync();

                // Send confirmation email
                var user = await _db.Users.FindAsync(userId);
                if (user != null)
                {
                    try { await _emailService.SendPaymentConfirmationAsync(user.Email, user.FullName, displayName, tran_id, price); }
                    catch { /* non-blocking */ }
                }
            }

            ViewBag.TransactionId = tran_id;
            ViewBag.Amount        = amount;
            ViewBag.PlanName      = displayName;
            ViewBag.CardType      = card_type;
            return View();
        }

        [HttpPost, AllowAnonymous]
        public IActionResult PaymentFail([FromForm] string? tran_id, [FromForm] string? error)
        {
            ViewBag.TransactionId = tran_id;
            ViewBag.Error         = error ?? "Your payment could not be processed.";
            return View();
        }

        [HttpPost, AllowAnonymous]
        public IActionResult PaymentCancel([FromForm] string? tran_id)
        {
            ViewBag.TransactionId = tran_id;
            return View();
        }

        [HttpPost, AllowAnonymous]
        public IActionResult PaymentIpn() => Ok(); // IPN acknowledgement

        // ─── CANCEL SUBSCRIPTION ──────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel()
        {
            await _subscriptionService.CancelAsync(GetUserId());
            TempData["SuccessMessage"] = "Subscription cancelled.";
            return RedirectToAction(nameof(Index));
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var p) ? p : 0;
        }
    }
}
