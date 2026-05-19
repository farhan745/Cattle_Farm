using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
            => await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<User?> GetByUsernameAsync(string username)
            => await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        public async Task<bool> EmailExistsAsync(string email)
            => await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<bool> UsernameExistsAsync(string username)
            => await _dbSet.AnyAsync(u => u.Username.ToLower() == username.ToLower());

        public async Task<IEnumerable<User>> GetByRoleAsync(string role)
            => await _dbSet.Where(u => u.Role == role).ToListAsync();

        public async Task<(IEnumerable<User> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, string? role = null)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u => u.Username.Contains(search) || u.Email.Contains(search) || u.FullName.Contains(search));
            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role == role);
            int total = await query.CountAsync();
            var items = await query.OrderByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
