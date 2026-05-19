using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<(IEnumerable<Payment> Items, int Total)> GetPagedAsync(int page, int pageSize, int? userId = null);
    }
}
