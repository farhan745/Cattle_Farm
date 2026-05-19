using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService   _orderService;
        private readonly IProductService _productService;
        private readonly IFarmService    _farmService;
        private readonly IPaymentGatewayService _paymentService;
        private readonly CattleFarmDbContext     _db;

        public OrderController(
            IOrderService order, 
            IProductService product, 
            IFarmService farm,
            IPaymentGatewayService payment,
            CattleFarmDbContext db)
        { 
            _orderService = order; 
            _productService = product; 
            _farmService = farm; 
            _paymentService = payment;
            _db = db;
        }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, OrderStatus? status = null)
        {
            var (items, total) = await _orderService.GetPagedAsync(page, 12, farmId, status);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)12);
            ViewBag.Farms  = await _farmService.GetAllAsync();
            ViewData["Status"] = status;
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetWithItemsAsync(id);
            if (order is null) return NotFound();
            return View(order);
        }

        // Customer places order
        public async Task<IActionResult> Create(int farmId, int? productId)
        {
            ViewBag.Products = await _productService.GetByFarmAsync(farmId);
            var vm = new OrderViewModel { FarmId = farmId };
            if (productId.HasValue)
            {
                vm.Items.Add(new OrderItemViewModel { ProductId = productId.Value, Quantity = 1 });
            }
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderViewModel vm)
        {
            if (!vm.Items.Any()) 
            { 
                TempData["ErrorMessage"] = "Please add at least one item."; 
                return RedirectToAction(nameof(Create), new { vm.FarmId }); 
            }

            var order = await _orderService.CreateAsync(vm, GetUserId());

            if (vm.PaymentMethod == PaymentMethod.Cash)
            {
                TempData["SuccessMessage"] = $"Order #{order.Id} placed successfully (Cash on Delivery)!";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            // Online payment initiation via SSLCommerz
            var userId = GetUserId();
            var user = await _db.Users.FindAsync(userId);
            var txId = $"ORD-{order.Id}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var request = new PaymentInitRequest
            {
                UserId = userId,
                UserName = user?.FullName ?? user?.Username ?? "Customer",
                UserEmail = user?.Email ?? "",
                UserPhone = user?.PhoneNumber ?? "01700000000",
                UserAddress = user?.Address ?? "Dhaka, Bangladesh",
                PlanName = $"Order #{order.Id}",
                Amount = order.TotalAmount,
                Currency = "BDT",
                TransactionId = txId,
                SuccessUrl = $"{baseUrl}/Order/PaymentSuccess",
                FailUrl = $"{baseUrl}/Order/PaymentFail",
                CancelUrl = $"{baseUrl}/Order/PaymentCancel",
                IpnUrl = $"{baseUrl}/Order/PaymentIpn"
            };

            var result = await _paymentService.InitiatePaymentAsync(request);

            if (result.Success && !string.IsNullOrEmpty(result.GatewayUrl))
            {
                return Redirect(result.GatewayUrl);
            }

            TempData["ErrorMessage"] = $"Payment initiation failed: {result.Error}. Falling back to pending payment.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
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
                TempData["ErrorMessage"] = "Payment verification failed.";
                return RedirectToAction(nameof(Index));
            }

            var parts = tran_id.Split('-');
            var orderId = parts.Length >= 2 && int.TryParse(parts[1], out var id) ? id : 0;
            decimal parsedAmount = decimal.TryParse(amount, out var a) ? a : 0m;

            if (orderId > 0)
            {
                await _orderService.CompletePaymentAsync(orderId, tran_id, card_type ?? "Online", parsedAmount);
            }

            ViewBag.TransactionId = tran_id;
            ViewBag.Amount = amount;
            ViewBag.OrderId = orderId;
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

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            await _orderService.UpdateStatusAsync(id, status);
            TempData["SuccessMessage"] = $"Order status updated to {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            await _orderService.CancelAsync(id);
            TempData["SuccessMessage"] = "Order cancelled and stock restored.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();

            if (order.PaymentStatus == PaymentStatus.Completed)
            {
                TempData["ErrorMessage"] = "Order is already paid.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = GetUserId();
            var user = await _db.Users.FindAsync(userId);
            var txId = $"ORD-{order.Id}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            var request = new PaymentInitRequest
            {
                UserId = userId,
                UserName = user?.FullName ?? user?.Username ?? "Customer",
                UserEmail = user?.Email ?? "",
                UserPhone = user?.PhoneNumber ?? "01700000000",
                UserAddress = user?.Address ?? "Dhaka, Bangladesh",
                PlanName = $"Order #{order.Id}",
                Amount = order.TotalAmount,
                Currency = "BDT",
                TransactionId = txId,
                SuccessUrl = $"{baseUrl}/Order/PaymentSuccess",
                FailUrl = $"{baseUrl}/Order/PaymentFail",
                CancelUrl = $"{baseUrl}/Order/PaymentCancel",
                IpnUrl = $"{baseUrl}/Order/PaymentIpn"
            };

            var result = await _paymentService.InitiatePaymentAsync(request);

            if (result.Success && !string.IsNullOrEmpty(result.GatewayUrl))
            {
                return Redirect(result.GatewayUrl);
            }

            TempData["ErrorMessage"] = $"Payment initiation failed: {result.Error}";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        private int GetUserId() { var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; return int.TryParse(id, out var p) ? p : 0; }
    }
}
