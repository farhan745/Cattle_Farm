using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IProductService _productService;
        public OrderService(IUnitOfWork uow, IProductService productService) { _uow = uow; _productService = productService; }

        public async Task<IEnumerable<Order>> GetByFarmAsync(int farmId) => await _uow.Orders.GetByFarmIdAsync(farmId);
        public async Task<Order?> GetByIdAsync(int id) => await _uow.Orders.GetByIdAsync(id);
        public async Task<Order?> GetWithItemsAsync(int orderId) => await _uow.Orders.GetWithItemsAsync(orderId);

        public async Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, OrderStatus? status = null)
            => await _uow.Orders.GetPagedAsync(page, pageSize, farmId, status);

        public async Task<Order> CreateAsync(OrderViewModel vm, int customerId)
        {
            decimal total = 0;
            var items = new List<OrderItem>();
            foreach (var item in vm.Items)
            {
                var product = await _uow.Products.GetByIdAsync(item.ProductId);
                if (product is null) continue;
                var lineTotal = product.Price * (decimal)item.Quantity;
                total += lineTotal;
                items.Add(new OrderItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = product.Price, TotalPrice = lineTotal });
                await _productService.AdjustStockAsync(item.ProductId, item.Quantity, false);
            }
            var order = new Order { CustomerId = customerId, FarmId = vm.FarmId, TotalAmount = total, DeliveryAddress = vm.DeliveryAddress, Notes = vm.Notes, OrderItems = items };
            await _uow.Orders.AddAsync(order);
            await _uow.SaveChangesAsync();

            // Create initial pending payment
            var payment = new Payment
            {
                UserId = customerId,
                Amount = total,
                Method = vm.PaymentMethod,
                Status = PaymentStatus.Pending,
                Purpose = PaymentPurpose.Order,
                OrderId = order.Id,
                ReferenceId = order.Id,
                ReferenceType = "Order",
                Notes = $"Initial payment selection: {vm.PaymentMethod}"
            };
            await _uow.Payments.AddAsync(payment);
            await _uow.SaveChangesAsync();

            return order;
        }

        public async Task<bool> CompletePaymentAsync(int orderId, string transactionId, string cardType, decimal amount)
        {
            var order = await _uow.Orders.GetByIdAsync(orderId);
            if (order is null) return false;

            order.PaymentStatus = PaymentStatus.Completed;
            order.UpdatedAt = DateTime.UtcNow;
            _uow.Orders.Update(order);

            // Update/Create the completed payment record.
            var payments = await _uow.Payments.GetByUserIdAsync(order.CustomerId);
            var existingPayment = payments.FirstOrDefault(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending);

            PaymentMethod method = PaymentMethod.BankTransfer;
            var cLower = cardType?.ToLower() ?? "";
            if (cLower.Contains("bkash")) method = PaymentMethod.Bkash;
            else if (cLower.Contains("nagad")) method = PaymentMethod.Nagad;
            else if (cLower.Contains("visa")) method = PaymentMethod.Visa;
            else if (cLower.Contains("master")) method = PaymentMethod.MasterCard;

            if (existingPayment != null)
            {
                existingPayment.Status = PaymentStatus.Completed;
                existingPayment.Method = method;
                existingPayment.TransactionId = transactionId;
                existingPayment.Amount = amount;
                existingPayment.PaymentDate = DateTime.UtcNow;
                existingPayment.Notes = $"Payment completed via SSLCommerz ({cardType})";
                _uow.Payments.Update(existingPayment);
            }
            else
            {
                var payment = new Payment
                {
                    UserId = order.CustomerId,
                    Amount = amount,
                    Method = method,
                    Status = PaymentStatus.Completed,
                    Purpose = PaymentPurpose.Order,
                    TransactionId = transactionId,
                    OrderId = order.Id,
                    ReferenceId = order.Id,
                    ReferenceType = "Order",
                    Notes = $"Payment completed via SSLCommerz ({cardType})"
                };
                await _uow.Payments.AddAsync(payment);
            }

            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _uow.Orders.GetByIdAsync(orderId);
            if (order is null) return false;
            order.OrderStatus = status;
            if (status == OrderStatus.Delivered) order.DeliveredAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int orderId)
        {
            var order = await _uow.Orders.GetWithItemsAsync(orderId);
            if (order is null) return false;
            order.OrderStatus = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            foreach (var item in order.OrderItems)
                await _productService.AdjustStockAsync(item.ProductId, item.Quantity, true);
            _uow.Orders.Update(order);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
