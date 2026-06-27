using System.Collections.Generic;
using System.Threading.Tasks;
using CattleFarm.Models;
using CattleFarm.Services.Implementations;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.Repositories.Interfaces;
using CattleFarm.ViewModels;
using Moq;
using Xunit;

namespace CattleFarm.Tests
{
    public class OrderWorkflowTests
    {
        [Fact]
        public async Task CreateAsync_ValidOrder_CreatesOrderAndInitialPayment()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockProductService = new Mock<IProductService>();
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockProductRepo = new Mock<IProductRepository>();
            var mockPaymentRepo = new Mock<IPaymentRepository>();

            var product = new Product { Id = 1, Price = 100, StockQuantity = 10 };
            mockProductRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            
            mockUow.Setup(u => u.Orders).Returns(mockOrderRepo.Object);
            mockUow.Setup(u => u.Products).Returns(mockProductRepo.Object);
            mockUow.Setup(u => u.Payments).Returns(mockPaymentRepo.Object);

            var service = new OrderService(mockUow.Object, mockProductService.Object);
            
            var vm = new OrderViewModel
            {
                FarmId = 1,
                DeliveryAddress = "Dhaka",
                Notes = "Deliver ASAP",
                PaymentMethod = PaymentMethod.Bkash,
                Items = new List<OrderItemViewModel>
                {
                    new OrderItemViewModel { ProductId = 1, Quantity = 2 }
                }
            };

            // Act
            var result = await service.CreateAsync(vm, 42); // Customer ID 42

            // Assert
            Assert.NotNull(result);
            Assert.Equal(42, result.CustomerId);
            Assert.Equal(200, result.TotalAmount); // 100 * 2
            Assert.Single(result.OrderItems);
            
            // Verify stock adjustment call
            mockProductService.Verify(s => s.AdjustStockAsync(1, 2, false), Times.Once);
            
            // Verify order saved
            mockOrderRepo.Verify(r => r.AddAsync(It.Is<Order>(o => o.CustomerId == 42)), Times.Once);

            // Verify payment saved
            mockPaymentRepo.Verify(r => r.AddAsync(It.Is<Payment>(p => 
                p.UserId == 42 && 
                p.Amount == 200 && 
                p.Method == PaymentMethod.Bkash && 
                p.Status == PaymentStatus.Pending
            )), Times.Once);

            mockUow.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task CompletePaymentAsync_ValidDetails_UpdatesOrderAndPayment()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockPaymentRepo = new Mock<IPaymentRepository>();

            var order = new Order { Id = 5, CustomerId = 42, PaymentStatus = PaymentStatus.Pending };
            mockOrderRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(order);

            var pendingPayment = new Payment { OrderId = 5, UserId = 42, Status = PaymentStatus.Pending };
            var paymentsList = new List<Payment> { pendingPayment };
            mockPaymentRepo.Setup(r => r.GetByUserIdAsync(42)).ReturnsAsync(paymentsList);

            mockUow.Setup(u => u.Orders).Returns(mockOrderRepo.Object);
            mockUow.Setup(u => u.Payments).Returns(mockPaymentRepo.Object);

            var service = new OrderService(mockUow.Object, null!);

            // Act
            var result = await service.CompletePaymentAsync(5, "TXN-12345", "bkash", 200);

            // Assert
            Assert.True(result);
            Assert.Equal(PaymentStatus.Completed, order.PaymentStatus);
            Assert.Equal(PaymentStatus.Completed, pendingPayment.Status);
            Assert.Equal(PaymentMethod.Bkash, pendingPayment.Method);
            Assert.Equal("TXN-12345", pendingPayment.TransactionId);

            mockOrderRepo.Verify(r => r.Update(order), Times.Once);
            mockPaymentRepo.Verify(r => r.Update(pendingPayment), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
