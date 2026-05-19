namespace CattleFarm.Services.Interfaces
{
    /// <summary>
    /// Service for Admin user-management operations: listing users and changing roles.
    /// </summary>
    public interface IUserManagementService
    {
        /// <summary>Returns all registered users ordered by creation date.</summary>
        Task<IEnumerable<Models.User>> GetAllUsersAsync();

        /// <summary>
        /// Changes a user's role. Returns false if the userId is invalid or
        /// the caller is trying to change their own role.
        /// </summary>
        Task<bool> ChangeRoleAsync(int userId, string newRole, int callerUserId);

        /// <summary>Returns a single user by id, or null if not found.</summary>
        Task<Models.User?> GetByIdAsync(int userId);

        /// <summary>Persists changes to an existing user entity.</summary>
        Task UpdateAsync(Models.User user);
    }
}
