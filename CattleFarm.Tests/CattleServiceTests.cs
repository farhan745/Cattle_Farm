using System;
using System.Threading.Tasks;
using CattleFarm.Models;
using CattleFarm.Services.Implementations;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.Repositories.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace CattleFarm.Tests
{
    public class CattleServiceTests
    {
        [Fact]
        public async Task CreateAsync_SavesCattleWithPropertiesAndReturnsIt()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockCattleRepo = new Mock<ICattleRepository>();
            var mockImageService = new Mock<IImageService>();

            mockCattleRepo.Setup(r => r.AddAsync(It.IsAny<Cattle>())).Returns(Task.CompletedTask);
            mockUow.Setup(u => u.Cattles).Returns(mockCattleRepo.Object);

            var mockFile = new Mock<IFormFile>();
            mockImageService.Setup(s => s.SaveImageAsync(mockFile.Object, "cattle")).ReturnsAsync("/uploads/cattle/test.jpg");

            var service = new CattleService(mockUow.Object, mockImageService.Object);

            var vm = new CattleViewModel
            {
                TagId = "TAG-001",
                Name = "Molly",
                Breed = "Jersey",
                DateOfBirth = DateTime.Today.AddYears(-2),
                Weight = 350.5,
                Gender = Gender.Female,
                HealthStatus = HealthStatus.Healthy,
                Status = CattleStatus.Active,
                FarmId = 1,
                PurchasePrice = 80000,
                ImageFile = mockFile.Object
            };

            // Act
            var result = await service.CreateAsync(vm);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TAG-001", result.TagId);
            Assert.Equal("Molly", result.Name);
            Assert.Equal("Jersey", result.Breed);
            Assert.Equal(350.5, result.Weight);
            Assert.Equal("/uploads/cattle/test.jpg", result.ImagePath);
            
            mockCattleRepo.Verify(r => r.AddAsync(It.Is<Cattle>(c => c.TagId == "TAG-001")), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_SetsIsDeletedToTrueAndSaves()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockCattleRepo = new Mock<ICattleRepository>();
            var mockImageService = new Mock<IImageService>();

            var cattle = new Cattle { Id = 1, TagId = "TAG-001", IsDeleted = false };
            mockCattleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cattle);
            mockUow.Setup(u => u.Cattles).Returns(mockCattleRepo.Object);

            var service = new CattleService(mockUow.Object, mockImageService.Object);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.True(result);
            Assert.True(cattle.IsDeleted);
            Assert.NotNull(cattle.DeletedAt);
            mockCattleRepo.Verify(r => r.Update(cattle), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateHealthStatusAsync_UpdatesCattleHealthStatusFromLatestRecord()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockCattleRepo = new Mock<ICattleRepository>();
            var mockHealthRepo = new Mock<IHealthRecordRepository>();

            var cattle = new Cattle { Id = 1, TagId = "TAG-001", HealthStatus = HealthStatus.Healthy };
            var healthRecord = new HealthRecord { CattleId = 1, HealthStatus = HealthStatus.Sick };

            mockCattleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(cattle);
            mockHealthRepo.Setup(r => r.GetLatestByCattleIdAsync(1)).ReturnsAsync(healthRecord);

            mockUow.Setup(u => u.Cattles).Returns(mockCattleRepo.Object);
            mockUow.Setup(u => u.HealthRecords).Returns(mockHealthRepo.Object);

            var service = new CattleService(mockUow.Object, null!);

            // Act
            await service.UpdateHealthStatusAsync(1);

            // Assert
            Assert.Equal(HealthStatus.Sick, cattle.HealthStatus);
            mockCattleRepo.Verify(r => r.Update(cattle), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
