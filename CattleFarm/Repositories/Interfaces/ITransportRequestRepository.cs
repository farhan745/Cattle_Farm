using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface ITransportRequestRepository : IRepository<TransportRequest>
    {
        Task<IEnumerable<TransportRequest>> GetByStatusAsync(TripStatus status);
        Task<IEnumerable<TransportRequest>> GetByFarmIdAsync(int farmId);
        Task<TransportRequest?> GetWithDetailsAsync(int id);
        Task<(IEnumerable<TransportRequest> Items, int Total)> GetPagedAsync(
            int page, int pageSize, TripStatus? status = null,
            TransportType? type = null, int? farmId = null, string? search = null);
        Task<IEnumerable<TransportRequest>> GetPendingAsync();
        Task<int> CountByStatusAsync(TripStatus status);
    }
}
