using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface ICattleMedicalRecordRepository : IRepository<CattleMedicalRecord>
    {
        Task<IEnumerable<CattleMedicalRecord>> GetByCattleIdAsync(int cattleId);
        Task<CattleMedicalRecord?> GetByIdWithDetailsAsync(int id);
    }
}
