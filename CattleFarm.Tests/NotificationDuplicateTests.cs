using System.Threading.Tasks;
using CattleFarm.Hubs;
using CattleFarm.Models;
using CattleFarm.Services.Implementations;
using CattleFarm.UnitOfWork;
using CattleFarm.Repositories.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace CattleFarm.Tests
{
    public class NotificationDuplicateTests
    {
        [Fact]
        public async Task SendAsync_WhenUnreadExists_DoesNotCreateDuplicate()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockNotificationRepo = new Mock<INotificationRepository>();
            var mockHubContext = new Mock<IHubContext<FarmDashboardHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            // Setup HasUnreadAsync to return true (duplicate found)
            mockNotificationRepo.Setup(r => r.HasUnreadAsync(
                It.IsAny<int>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<int?>()
            )).ReturnsAsync(true);

            mockUow.Setup(u => u.Notifications).Returns(mockNotificationRepo.Object);

            var service = new NotificationService(mockUow.Object, mockHubContext.Object, null!);

            // Act
            await service.SendAsync(1, "Test Alert", "Test Message", NotificationType.LowFeedStock, "FeedInventory", 1);

            // Assert
            // Verify AddAsync was never called because it was detected as a duplicate
            mockNotificationRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Never);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task SendAsync_WhenNoUnreadExists_CreatesNotification()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockNotificationRepo = new Mock<INotificationRepository>();
            var mockHubContext = new Mock<IHubContext<FarmDashboardHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            // Setup HasUnreadAsync to return false (no duplicate)
            mockNotificationRepo.Setup(r => r.HasUnreadAsync(
                It.IsAny<int>(),
                It.IsAny<NotificationType>(),
                It.IsAny<string>(),
                It.IsAny<int?>()
            )).ReturnsAsync(false);

            mockUow.Setup(u => u.Notifications).Returns(mockNotificationRepo.Object);

            // Setup SignalR Hub Mocks to prevent null reference on notification push
            mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var service = new NotificationService(mockUow.Object, mockHubContext.Object, null!);

            // Act
            await service.SendAsync(1, "Test Alert", "Test Message", NotificationType.LowFeedStock, "FeedInventory", 1);

            // Assert
            // Verify AddAsync and SaveChangesAsync were called since it is a new unique notification
            mockNotificationRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
