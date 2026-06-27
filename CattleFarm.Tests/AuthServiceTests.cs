using System.Threading.Tasks;
using CattleFarm.Models;
using CattleFarm.Services.Implementations;
using CattleFarm.UnitOfWork;
using CattleFarm.Repositories.Interfaces;
using Moq;
using Xunit;

namespace CattleFarm.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_ValidActiveUser_ReturnsUser()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockUserRepo = new Mock<IUserRepository>();
            
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                IsActive = true
            };

            mockUserRepo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);
            mockUow.Setup(u => u.Users).Returns(mockUserRepo.Object);

            var authService = new AuthService(mockUow.Object);

            // Act
            var result = await authService.LoginAsync("test@example.com", "Password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task LoginAsync_InvalidPassword_ReturnsNull()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                IsActive = true
            };

            mockUserRepo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);
            mockUow.Setup(u => u.Users).Returns(mockUserRepo.Object);

            var authService = new AuthService(mockUow.Object);

            // Act
            var result = await authService.LoginAsync("test@example.com", "WrongPassword");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_InactiveUser_ReturnsNull()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockUserRepo = new Mock<IUserRepository>();

            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                IsActive = false
            };

            mockUserRepo.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);
            mockUow.Setup(u => u.Users).Returns(mockUserRepo.Object);

            var authService = new AuthService(mockUow.Object);

            // Act
            var result = await authService.LoginAsync("test@example.com", "Password123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RegisterAsync_ValidDetails_CreatesUserAndReturnsTrue()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockUserRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockUserRepo.Setup(r => r.UsernameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockUow.Setup(u => u.Users).Returns(mockUserRepo.Object);

            var authService = new AuthService(mockUow.Object);

            // Act
            var result = await authService.RegisterAsync("testuser", "test@example.com", "Password123", "User", "Test User", "01700000000");

            // Assert
            Assert.True(result);
            mockUserRepo.Verify(r => r.AddAsync(It.Is<User>(u => u.Username == "testuser" && u.Email == "test@example.com")), Times.Once);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateEmail_ReturnsFalse()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockUserRepo.Setup(r => r.EmailExistsAsync("test@example.com")).ReturnsAsync(true);
            mockUow.Setup(u => u.Users).Returns(mockUserRepo.Object);

            var authService = new AuthService(mockUow.Object);

            // Act
            var result = await authService.RegisterAsync("testuser", "test@example.com", "Password123");

            // Assert
            Assert.False(result);
            mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateUsername_ReturnsFalse()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();
            var mockUserRepo = new Mock<IUserRepository>();

            mockUserRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockUserRepo.Setup(r => r.UsernameExistsAsync("testuser")).ReturnsAsync(true);
            mockUow.Setup(u => u.Users).Returns(mockUserRepo.Object);

            var authService = new AuthService(mockUow.Object);

            // Act
            var result = await authService.RegisterAsync("testuser", "test@example.com", "Password123");

            // Assert
            Assert.False(result);
            mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
            mockUow.Verify(u => u.SaveChangesAsync(), Times.Never);
        }
    }
}
