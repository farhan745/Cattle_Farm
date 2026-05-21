using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;

namespace CattleFarm.Services.Implementations
{
    /// <summary>
    /// Handles user registration and login with BCrypt password hashing.
    /// Never exposes plain-text passwords beyond this boundary.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user is null) return null;

            // BCrypt.Verify compares plain-text against stored hash
            bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isValid ? user : null;
        }

        /// <inheritdoc/>
        public async Task<bool> RegisterAsync(string username, string email, string password, string role = "User")
        {
            // Reject duplicate email or username
            if (await _unitOfWork.Users.EmailExistsAsync(email))     return false;
            if (await _unitOfWork.Users.UsernameExistsAsync(username)) return false;

            var user = new User
            {
                Username     = username,
                Email        = email,
                // Work factor 12 — good balance of security vs. performance
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
                Role         = role,
                IsEmailVerified = true,
                CreatedAt    = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> EmailExistsAsync(string email)
            => await _unitOfWork.Users.EmailExistsAsync(email);

        /// <inheritdoc/>
        public async Task<bool> UsernameExistsAsync(string username)
            => await _unitOfWork.Users.UsernameExistsAsync(username);
    }
}
