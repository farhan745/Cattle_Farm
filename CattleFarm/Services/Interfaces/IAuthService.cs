using CattleFarm.Models;

namespace CattleFarm.Services.Interfaces
{
    /// <summary>
    /// Authentication service interface providing registration and login operations.
    /// Password hashing is handled internally — callers pass plain-text passwords.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Validates credentials and returns the matching User, or null if invalid.
        /// </summary>
        Task<User?> LoginAsync(string email, string password);

        /// <summary>
        /// Creates a new user account. Returns false if the email or username is taken.
        /// </summary>
        Task<bool> RegisterAsync(string username, string email, string password, string role = "User", string? fullName = null, string? phoneNumber = null);

        /// <summary>Returns true if a user with the given email already exists.</summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>Returns true if a user with the given username already exists.</summary>
        Task<bool> UsernameExistsAsync(string username);
    }
}
