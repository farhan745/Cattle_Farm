using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CattleFarm.Controllers;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CattleFarm.Tests
{
    public class PayrollAuthorizationTests
    {
        [Fact]
        public async Task ExportSlipPdf_WorkerRequestsOwnSlip_ReturnsFileResult()
        {
            // Arrange
            var mockPayrollService = new Mock<IPayrollService>();
            var mockFarmService = new Mock<IFarmService>();
            var mockPdfService = new Mock<IPdfService>();

            var payroll = new Payroll { Id = 1, UserId = 42, FarmId = 2, Month = 5, Year = 2026, Worker = new Worker { FullName = "John Doe" } };
            mockPayrollService.Setup(s => s.GetPayrollEntityByIdAsync(1)).ReturnsAsync(payroll);
            mockPdfService.Setup(s => s.GeneratePayrollSlipPdf(payroll)).Returns(new byte[] { 1, 2, 3 });

            var controller = new PayrollController(mockPayrollService.Object, mockFarmService.Object, mockPdfService.Object);
            
            // Set user claims as Worker, ID = 42
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42"),
                new Claim(ClaimTypes.Role, AppRoles.Worker)
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.ExportSlipPdf(1);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }

        [Fact]
        public async Task ExportSlipPdf_WorkerRequestsOtherSlip_ReturnsForbidResult()
        {
            // Arrange
            var mockPayrollService = new Mock<IPayrollService>();
            var mockFarmService = new Mock<IFarmService>();
            var mockPdfService = new Mock<IPdfService>();

            var payroll = new Payroll { Id = 1, UserId = 99, FarmId = 2 }; // Slip belongs to user 99
            mockPayrollService.Setup(s => s.GetPayrollEntityByIdAsync(1)).ReturnsAsync(payroll);

            var controller = new PayrollController(mockPayrollService.Object, mockFarmService.Object, mockPdfService.Object);

            // Set user claims as Worker, ID = 42
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42"),
                new Claim(ClaimTypes.Role, AppRoles.Worker)
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.ExportSlipPdf(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ExportSlipPdf_OwnerRequestsSlipForOwnFarmWorker_ReturnsFileResult()
        {
            // Arrange
            var mockPayrollService = new Mock<IPayrollService>();
            var mockFarmService = new Mock<IFarmService>();
            var mockPdfService = new Mock<IPdfService>();

            var payroll = new Payroll { Id = 1, UserId = 99, FarmId = 2, Month = 5, Year = 2026, Worker = new Worker { FullName = "John Doe" } };
            mockPayrollService.Setup(s => s.GetPayrollEntityByIdAsync(1)).ReturnsAsync(payroll);
            mockPdfService.Setup(s => s.GeneratePayrollSlipPdf(payroll)).Returns(new byte[] { 1, 2, 3 });

            // Owner owns farm ID = 2
            var ownedFarms = new List<Farm> { new Farm { Id = 2, OwnerId = 10 } };
            mockFarmService.Setup(s => s.GetByOwnerAsync(10)).ReturnsAsync(ownedFarms);

            var controller = new PayrollController(mockPayrollService.Object, mockFarmService.Object, mockPdfService.Object);

            // Set user claims as Owner, ID = 10
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "10"),
                new Claim(ClaimTypes.Role, AppRoles.Owner)
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.ExportSlipPdf(1);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }

        [Fact]
        public async Task ExportSlipPdf_OwnerRequestsSlipForOtherFarmWorker_ReturnsForbidResult()
        {
            // Arrange
            var mockPayrollService = new Mock<IPayrollService>();
            var mockFarmService = new Mock<IFarmService>();
            var mockPdfService = new Mock<IPdfService>();

            var payroll = new Payroll { Id = 1, UserId = 99, FarmId = 3 }; // Slip belongs to farm 3
            mockPayrollService.Setup(s => s.GetPayrollEntityByIdAsync(1)).ReturnsAsync(payroll);

            // Owner owns farm ID = 2 (not 3)
            var ownedFarms = new List<Farm> { new Farm { Id = 2, OwnerId = 10 } };
            mockFarmService.Setup(s => s.GetByOwnerAsync(10)).ReturnsAsync(ownedFarms);

            var controller = new PayrollController(mockPayrollService.Object, mockFarmService.Object, mockPdfService.Object);

            // Set user claims as Owner, ID = 10
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "10"),
                new Claim(ClaimTypes.Role, AppRoles.Owner)
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.ExportSlipPdf(1);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }
    }
}
