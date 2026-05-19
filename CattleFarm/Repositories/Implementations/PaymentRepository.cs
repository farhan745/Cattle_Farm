using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(int userId)
            => await _dbSet.Where(p => p.UserId == userId).OrderByDescending(p => p.PaymentDate).ToListAsync();

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
            => await _dbSet.Where(p => p.Status == status).Include(p => p.User).ToListAsync();

        public async Task<(IEnumerable<Payment> Items, int Total)> GetPagedAsync(int page, int pageSize, int? userId = null)
        {
            var q = _dbSet.Include(p => p.User).AsQueryable();
            if (userId.HasValue) q = q.Where(p => p.UserId == userId.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(p => p.PaymentDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
