using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<Order?>             GetWithItemsAsync(int orderId);
        Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, OrderStatus? status = null);
        Task<decimal> GetTotalRevenueAsync(int farmId, DateTime? from = null, DateTime? to = null);
    }
}
