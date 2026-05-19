using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Order>> GetByFarmIdAsync(int farmId)
            => await _dbSet.Where(o => o.FarmId == farmId).Include(o => o.Customer)
                           .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                           .OrderByDescending(o => o.OrderDate).ToListAsync();

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId)
            => await _dbSet.Where(o => o.CustomerId == customerId)
                           .Include(o => o.Farm).Include(o => o.OrderItems)
                           .OrderByDescending(o => o.OrderDate).ToListAsync();

        public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
            => await _dbSet.Where(o => o.OrderStatus == status)
                           .Include(o => o.Customer).Include(o => o.Farm).ToListAsync();

        public async Task<Order?> GetWithItemsAsync(int orderId)
            => await _dbSet.Where(o => o.Id == orderId)
                           .Include(o => o.Customer).Include(o => o.Farm)
                           .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                           .Include(o => o.Payments).FirstOrDefaultAsync();

        public async Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, OrderStatus? status = null)
        {
            var q = _dbSet.Include(o => o.Customer).Include(o => o.Farm).AsQueryable();
            if (farmId.HasValue) q = q.Where(o => o.FarmId == farmId.Value);
            if (status.HasValue) q = q.Where(o => o.OrderStatus == status.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(o => o.OrderDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<decimal> GetTotalRevenueAsync(int farmId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(o => o.FarmId == farmId && o.PaymentStatus == PaymentStatus.Completed).AsQueryable();
            if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value);
            if (to.HasValue)   q = q.Where(o => o.OrderDate <= to.Value);
            return await q.SumAsync(o => o.TotalAmount);
        }
    }
}
