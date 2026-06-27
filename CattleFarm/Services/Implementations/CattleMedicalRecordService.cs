using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class CattleMedicalRecordService : ICattleMedicalRecordService
    {
        private readonly IUnitOfWork _uow;

        public CattleMedicalRecordService(IUnitOfWork uow) => _uow = uow;

        public async Task<CattleMedicalRecord> AddRecordAsync(CattleMedicalRecordViewModel vm, int doctorUserId)
        {
            var record = MapToEntity(vm, doctorUserId);
            await _uow.CattleMedicalRecords.AddAsync(record);
            await _uow.SaveChangesAsync();
            return record;
        }

        public async Task<IEnumerable<CattleMedicalRecord>> GetByCattleIdAsync(int cattleId)
            => await _uow.CattleMedicalRecords.GetByCattleIdAsync(cattleId);

        public async Task<CattleMedicalRecord?> GetByIdAsync(int id)
            => await _uow.CattleMedicalRecords.GetByIdWithDetailsAsync(id);

        public async Task<bool> UpdateAsync(int id, CattleMedicalRecordViewModel vm, int doctorUserId)
        {
            var record = await _uow.CattleMedicalRecords.GetByIdAsync(id);
            if (record is null || record.DoctorId != doctorUserId)
                return false;

            record.ExaminationDate  = vm.ExaminationDate;
            record.ChiefComplaint   = vm.ChiefComplaint.Trim();
            record.Diagnosis        = vm.Diagnosis.Trim();
            record.Prescription     = vm.Prescription?.Trim();
            record.MedicineName     = vm.MedicineName?.Trim();
            record.MedicineDose     = vm.MedicineDose?.Trim();
            record.DoseFrequency    = vm.DoseFrequency?.Trim();
            record.DoseDurationDays = vm.DoseDurationDays;
            record.NextVisitDate    = vm.NextVisitDate;
            record.Notes            = vm.Notes?.Trim();
            record.UpdatedAt        = DateTime.UtcNow;

            _uow.CattleMedicalRecords.Update(record);
            await _uow.SaveChangesAsync();
            return true;
        }

        private static CattleMedicalRecord MapToEntity(CattleMedicalRecordViewModel vm, int doctorUserId) => new()
        {
            CattleId          = vm.CattleId,
            DoctorId          = doctorUserId,
            ExaminationDate   = vm.ExaminationDate,
            ChiefComplaint    = vm.ChiefComplaint.Trim(),
            Diagnosis         = vm.Diagnosis.Trim(),
            Prescription      = vm.Prescription?.Trim(),
            MedicineName      = vm.MedicineName?.Trim(),
            MedicineDose      = vm.MedicineDose?.Trim(),
            DoseFrequency     = vm.DoseFrequency?.Trim(),
            DoseDurationDays  = vm.DoseDurationDays,
            NextVisitDate     = vm.NextVisitDate,
            Notes             = vm.Notes?.Trim(),
            CreatedAt         = DateTime.UtcNow
        };
    }
}
