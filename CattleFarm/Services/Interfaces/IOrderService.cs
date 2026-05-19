using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetByFarmAsync(int farmId);
        Task<Order?>  GetByIdAsync(int id);
        Task<Order?>  GetWithItemsAsync(int orderId);
        Task<Order>   CreateAsync(OrderViewModel vm, int customerId);
        Task<bool>    UpdateStatusAsync(int orderId, OrderStatus status);
        Task<bool>    CancelAsync(int orderId);
        Task<bool>    CompletePaymentAsync(int orderId, string transactionId, string cardType, decimal amount);
        Task<(IEnumerable<Order> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, OrderStatus? status = null);
    }
}
