using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface ICattleMedicalRecordService
    {
        Task<CattleMedicalRecord> AddRecordAsync(CattleMedicalRecordViewModel vm, int doctorUserId);
        Task<IEnumerable<CattleMedicalRecord>> GetByCattleIdAsync(int cattleId);
        Task<CattleMedicalRecord?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, CattleMedicalRecordViewModel vm, int doctorUserId);
    }
}
