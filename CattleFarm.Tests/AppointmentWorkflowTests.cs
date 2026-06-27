using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AppointmentWorkflowTests
    {
        [Fact]
        public async Task CreateAsync_ScheduledInPast_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockFarmRepo = new Mock<IFarmRepository>();
            var farm = new Farm { Id = 1, OwnerId = 10 };
            mockFarmRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(farm);
            mockUow.Setup(u => u.Farms).Returns(mockFarmRepo.Object);

            var service = new AppointmentService(mockUow.Object, null!, null!);
            var vm = new AppointmentViewModel
            {
                FarmId = 1,
                ScheduledAt = DateTime.Now.AddHours(-1) // Past
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(vm, 10, AppRoles.Owner));
            Assert.Equal("Appointment must be scheduled in the future.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_DoctorNotApproved_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockFarmRepo = new Mock<IFarmRepository>();
            var mockDoctorRepo = new Mock<IDoctorRepository>();

            var farm = new Farm { Id = 1, OwnerId = 10 };
            var doctor = new Doctor { Id = 5, ApprovalStatus = ApprovalStatus.Pending }; // Pending approval

            mockFarmRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(farm);
            mockDoctorRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(doctor);

            mockUow.Setup(u => u.Farms).Returns(mockFarmRepo.Object);
            mockUow.Setup(u => u.Doctors).Returns(mockDoctorRepo.Object);

            var service = new AppointmentService(mockUow.Object, null!, null!);
            var vm = new AppointmentViewModel
            {
                FarmId = 1,
                DoctorId = 5,
                ScheduledAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(vm, 10, AppRoles.Owner));
            Assert.Equal("This veterinarian is not available for booking.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_OverlappingAppointment_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockFarmRepo = new Mock<IFarmRepository>();
            var mockDoctorRepo = new Mock<IDoctorRepository>();
            var mockApptRepo = new Mock<IAppointmentRepository>();

            var farm = new Farm { Id = 1, OwnerId = 10 };
            var doctor = new Doctor { Id = 5, ApprovalStatus = ApprovalStatus.Approved };
            var targetTime = DateTime.Now.AddDays(1);

            // Existing appointment overlaps (30 mins before)
            var existing = new List<Appointment>
            {
                new Appointment { DoctorId = 5, Status = AppointmentStatus.Accepted, ScheduledAt = targetTime.AddMinutes(-30) }
            };

            mockFarmRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(farm);
            mockDoctorRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(doctor);
            mockApptRepo.Setup(r => r.GetByDoctorIdAsync(5)).ReturnsAsync(existing);

            mockUow.Setup(u => u.Farms).Returns(mockFarmRepo.Object);
            mockUow.Setup(u => u.Doctors).Returns(mockDoctorRepo.Object);
            mockUow.Setup(u => u.Appointments).Returns(mockApptRepo.Object);

            var service = new AppointmentService(mockUow.Object, null!, null!);
            var vm = new AppointmentViewModel
            {
                FarmId = 1,
                DoctorId = 5,
                ScheduledAt = targetTime
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(vm, 10, AppRoles.Owner));
            Assert.Equal("This veterinarian is already booked for another appointment at or near this time slot.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_CattleNotBelongingToSelectedFarm_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockFarmRepo = new Mock<IFarmRepository>();
            var mockDoctorRepo = new Mock<IDoctorRepository>();
            var mockApptRepo = new Mock<IAppointmentRepository>();
            var mockCattleRepo = new Mock<ICattleRepository>();

            var farm = new Farm { Id = 1, OwnerId = 10 };
            var doctor = new Doctor { Id = 5, ApprovalStatus = ApprovalStatus.Approved };
            var cattle = new Cattle { Id = 20, FarmId = 2 }; // Belongs to farm 2

            mockFarmRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(farm);
            mockDoctorRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(doctor);
            mockApptRepo.Setup(r => r.GetByDoctorIdAsync(5)).ReturnsAsync(new List<Appointment>());
            mockCattleRepo.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(cattle);

            mockUow.Setup(u => u.Farms).Returns(mockFarmRepo.Object);
            mockUow.Setup(u => u.Doctors).Returns(mockDoctorRepo.Object);
            mockUow.Setup(u => u.Appointments).Returns(mockApptRepo.Object);
            mockUow.Setup(u => u.Cattles).Returns(mockCattleRepo.Object);

            var service = new AppointmentService(mockUow.Object, null!, null!);
            var vm = new AppointmentViewModel
            {
                FarmId = 1,
                DoctorId = 5,
                CattleId = 20,
                ScheduledAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAsync(vm, 10, AppRoles.Owner));
            Assert.Equal("Cattle does not belong to the selected farm.", ex.Message);
        }

        [Fact]
        public async Task CreateAsync_OwnerBookForAnotherOwnerFarm_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockFarmRepo = new Mock<IFarmRepository>();
            var farm = new Farm { Id = 1, OwnerId = 11 }; // Owned by user 11
            mockFarmRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(farm);
            mockUow.Setup(u => u.Farms).Returns(mockFarmRepo.Object);

            var service = new AppointmentService(mockUow.Object, null!, null!);
            var vm = new AppointmentViewModel
            {
                FarmId = 1,
                ScheduledAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.CreateAsync(vm, 10, AppRoles.Owner)); // User 10 tries to create
            Assert.Equal("You can only book for your own farms.", ex.Message);
        }
    }
}
