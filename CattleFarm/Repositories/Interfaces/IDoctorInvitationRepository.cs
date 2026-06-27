using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IDoctorInvitationRepository : IRepository<DoctorInvitation>
    {
        Task<DoctorInvitation?> GetByTokenAsync(string token);
        Task<IEnumerable<DoctorInvitation>> GetByCreatorAsync(int userId);
        Task<(IEnumerable<DoctorInvitation> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null);
        Task<int> CountAsync();
        Task<bool> IsEmailAlreadyInvitedAsync(string email);
    }
}
