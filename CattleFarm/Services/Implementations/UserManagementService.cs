using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Services.Implementations
{
    /// <summary>
    /// Implements user-management operations used by the Admin panel.
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserManagementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<User>> GetAllUsersAsync()
            => await _unitOfWork.Users.GetAllAsync();

        /// <inheritdoc/>
        public async Task<User?> GetByIdAsync(int userId)
            => await _unitOfWork.Users.GetByIdAsync(userId);

        /// <inheritdoc/>
        public async Task<bool> ChangeRoleAsync(int userId, string newRole, int callerUserId)
        {
            // Prevent an admin from changing their own role (accidental self-lockout)
            if (userId == callerUserId) return false;

            // Validate the role is one we recognise
            if (!AppRoles.All.Contains(newRole)) return false;

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user is null) return false;

            user.Role = newRole;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(User user)
        {
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
