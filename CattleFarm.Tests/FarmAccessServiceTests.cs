using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CattleFarm.Models;
using CattleFarm.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CattleFarm.Tests
{
    public class FarmAccessServiceTests
    {
        private CattleFarmDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<CattleFarmDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new CattleFarmDbContext(options);
        }

        [Fact]
        public async Task IsFarmOwnerAsync_OwnerMatches_ReturnsTrue()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, OwnerId = 10, Name = "Test Farm", IsDeleted = false };
            await db.Farms.AddAsync(farm);
            await db.SaveChangesAsync();

            var service = new FarmAccessService(db);

            // Act
            var isOwner = await service.IsFarmOwnerAsync(1, 10);
            var isNotOwner = await service.IsFarmOwnerAsync(1, 11);

            // Assert
            Assert.True(isOwner);
            Assert.False(isNotOwner);
        }

        [Fact]
        public async Task IsAssignedManagerAsync_ActiveManagerMatches_ReturnsTrue()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var manager = new FarmManager { Id = 1, FarmId = 1, ManagerUserId = 20, IsActive = true, IsDeleted = false };
            await db.FarmManagers.AddAsync(manager);
            await db.SaveChangesAsync();

            var service = new FarmAccessService(db);

            // Act
            var isManager = await service.IsAssignedManagerAsync(1, 20);
            var isNotManager = await service.IsAssignedManagerAsync(1, 21);

            // Assert
            Assert.True(isManager);
            Assert.False(isNotManager);
        }

        [Fact]
        public async Task CanOperateFarmAsync_AdminRole_ReturnsTrue()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var service = new FarmAccessService(db);

            // Act
            var canOperate = await service.CanOperateFarmAsync(1, 99, "Admin");

            // Assert
            Assert.True(canOperate);
        }

        [Fact]
        public async Task CanOperateFarmAsync_OwnerRole_ChecksOwnership()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var farm = new Farm { Id = 1, OwnerId = 10, Name = "Test Farm" };
            await db.Farms.AddAsync(farm);
            await db.SaveChangesAsync();

            var service = new FarmAccessService(db);

            // Act
            var canOperateOwner = await service.CanOperateFarmAsync(1, 10, "Owner");
            var cannotOperateOwner = await service.CanOperateFarmAsync(1, 11, "Owner");

            // Assert
            Assert.True(canOperateOwner);
            Assert.False(cannotOperateOwner);
        }

        [Fact]
        public async Task CanOperateFarmAsync_ManagerRole_ChecksAssignment()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var manager = new FarmManager { Id = 1, FarmId = 1, ManagerUserId = 20, IsActive = true, IsDeleted = false };
            await db.FarmManagers.AddAsync(manager);
            await db.SaveChangesAsync();

            var service = new FarmAccessService(db);

            // Act
            var canOperateManager = await service.CanOperateFarmAsync(1, 20, "Manager");
            var cannotOperateManager = await service.CanOperateFarmAsync(1, 21, "Manager");

            // Assert
            Assert.True(canOperateManager);
            Assert.False(cannotOperateManager);
        }

        [Fact]
        public async Task GetAccessibleFarmIdsAsync_OwnerRole_ReturnsOnlyOwnedFarms()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            await db.Farms.AddRangeAsync(
                new Farm { Id = 1, OwnerId = 10, Name = "Farm 1", IsDeleted = false },
                new Farm { Id = 2, OwnerId = 10, Name = "Farm 2", IsDeleted = false },
                new Farm { Id = 3, OwnerId = 11, Name = "Farm 3", IsDeleted = false },
                new Farm { Id = 4, OwnerId = 10, Name = "Farm 4", IsDeleted = true } // Deleted
            );
            await db.SaveChangesAsync();

            var service = new FarmAccessService(db);

            // Act
            var ids = await service.GetAccessibleFarmIdsAsync(10, "Owner");

            // Assert
            Assert.Equal(2, ids.Count);
            Assert.Contains(1, ids);
            Assert.Contains(2, ids);
            Assert.DoesNotContain(3, ids);
            Assert.DoesNotContain(4, ids);
        }

        [Fact]
        public async Task GetAccessibleFarmsAsync_ManagerRole_ReturnsActiveManagerFarm()
        {
            // Arrange
            using var db = GetInMemoryDbContext();
            var owner = new User { Id = 5, Username = "owner", FullName = "Owner", Email = "owner@example.com", PasswordHash = "", Role = "Owner" };
            await db.Users.AddAsync(owner);
            await db.Farms.AddRangeAsync(
                new Farm { Id = 1, OwnerId = 5, Name = "Farm 1", IsDeleted = false, Owner = owner },
                new Farm { Id = 2, OwnerId = 5, Name = "Farm 2", IsDeleted = false, Owner = owner }
            );
            await db.FarmManagers.AddAsync(
                new FarmManager { Id = 1, FarmId = 1, ManagerUserId = 20, IsActive = true, IsDeleted = false }
            );
            await db.SaveChangesAsync();

            var service = new FarmAccessService(db);

            // Act
            var farms = (await service.GetAccessibleFarmsAsync(20, "Manager")).ToList();

            // Assert
            Assert.Single(farms);
            Assert.Equal("Farm 1", farms[0].Name);
        }
    }
}
