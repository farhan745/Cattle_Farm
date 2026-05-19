using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class HealthService : IHealthService
    {
        private readonly IUnitOfWork _uow;
        public HealthService(IUnitOfWork uow) { _uow = uow; }

        public async Task<IEnumerable<HealthRecord>> GetByCattleAsync(int cattleId) => await _uow.HealthRecords.GetByCattleIdAsync(cattleId);
        public async Task<IEnumerable<HealthRecord>> GetHighRiskAsync(int farmId) => await _uow.HealthRecords.GetHighRiskAsync(farmId);
        public async Task<HealthRecord?> GetByIdAsync(int id) => await _uow.HealthRecords.GetByIdAsync(id);

        public async Task<(IEnumerable<HealthRecord> Items, int Total)> GetPagedAsync(int page, int pageSize, int? cattleId = null, RiskLevel? risk = null)
            => await _uow.HealthRecords.GetPagedAsync(page, pageSize, cattleId, risk);

        public async Task<HealthRecord> CreateAsync(HealthRecordViewModel vm)
        {
            var record = new HealthRecord
            {
                CattleId = vm.CattleId, DoctorId = vm.DoctorId, RecordDate = vm.RecordDate,
                Temperature = vm.Temperature, Weight = vm.Weight, HealthStatus = vm.HealthStatus,
                RiskLevel = vm.RiskLevel, Symptoms = vm.Symptoms, Notes = vm.Notes,
                VetRecommendation = vm.VetRecommendation
            };
            await _uow.HealthRecords.AddAsync(record);
            await _uow.SaveChangesAsync();
            return record;
        }

        public async Task<bool> UpdateAsync(int id, HealthRecordViewModel vm)
        {
            var record = await _uow.HealthRecords.GetByIdAsync(id);
            if (record is null) return false;
            record.DoctorId = vm.DoctorId; record.RecordDate = vm.RecordDate;
            record.Temperature = vm.Temperature; record.Weight = vm.Weight;
            record.HealthStatus = vm.HealthStatus; record.RiskLevel = vm.RiskLevel;
            record.Symptoms = vm.Symptoms; record.Notes = vm.Notes;
            record.VetRecommendation = vm.VetRecommendation;
            _uow.HealthRecords.Update(record);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _uow.HealthRecords.GetByIdAsync(id);
            if (record is null) return false;
            record.IsDeleted = true; record.DeletedAt = DateTime.UtcNow;
            _uow.HealthRecords.Update(record);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<RiskLevel> CalculateRiskAsync(int cattleId)
        {
            var latest = await _uow.HealthRecords.GetLatestByCattleIdAsync(cattleId);
            if (latest is null) return RiskLevel.Low;
            if (latest.HealthStatus == HealthStatus.Critical || latest.HealthStatus == HealthStatus.Sick) return RiskLevel.High;
            if (latest.HealthStatus == HealthStatus.AtRisk) return RiskLevel.Medium;
            return RiskLevel.Low;
        }
    }
}
