using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool>  EmailExistsAsync(string email);
        Task<bool>  UsernameExistsAsync(string username);
        Task<IEnumerable<User>> GetByRoleAsync(string role);
        Task<(IEnumerable<User> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, string? role = null);
    }
}
