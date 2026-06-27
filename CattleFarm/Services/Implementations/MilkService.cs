using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class MilkService : IMilkService
    {
        private readonly IUnitOfWork _uow;
        public MilkService(IUnitOfWork uow) { _uow = uow; }

        public async Task<IEnumerable<MilkProduction>> GetByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null) => await _uow.MilkProductions.GetByFarmIdAsync(farmId, from, to);
        public async Task<IEnumerable<MilkProduction>> GetByCattleAsync(int cattleId, DateTime? from = null, DateTime? to = null) => await _uow.MilkProductions.GetByCattleIdAsync(cattleId, from, to);
        public async Task<MilkProduction?> GetByIdAsync(int id) => await _uow.MilkProductions.GetByIdAsync(id);
        public async Task<double> GetTotalYieldByFarmAsync(int farmId, DateTime? from, DateTime? to) => await _uow.MilkProductions.GetTotalYieldByFarmAsync(farmId, from, to);

        public async Task<MilkProduction> CreateAsync(MilkProductionViewModel vm)
        {
            var m = new MilkProduction 
            { 
                CattleId = vm.CattleId, 
                FarmId = vm.FarmId, 
                RecordedByWorkerId = vm.RecordedByWorkerId, 
                Date = vm.Date, 
                MorningYieldLiters = vm.MorningYieldLiters, 
                EveningYieldLiters = vm.EveningYieldLiters, 
                Notes = vm.Notes,
                FatPercentage = vm.FatPercentage,
                ProteinLevel = vm.ProteinLevel,
                SolidNotFat = vm.SolidNotFat,
                MilkQualityGrade = vm.MilkQualityGrade
            };

            // Auto grade calculation if not specified
            if (string.IsNullOrWhiteSpace(m.MilkQualityGrade) && m.FatPercentage.HasValue && m.ProteinLevel.HasValue)
            {
                if (m.FatPercentage >= 4.0m && m.ProteinLevel >= 3.5m) m.MilkQualityGrade = "A";
                else if (m.FatPercentage >= 3.2m && m.ProteinLevel >= 3.0m) m.MilkQualityGrade = "B";
                else m.MilkQualityGrade = "C";
            }

            await _uow.MilkProductions.AddAsync(m);
            await _uow.SaveChangesAsync();
            return m;
        }

        public async Task<bool> UpdateAsync(int id, MilkProductionViewModel vm)
        {
            var m = await _uow.MilkProductions.GetByIdAsync(id);
            if (m is null) return false;
            
            m.Date = vm.Date; 
            m.MorningYieldLiters = vm.MorningYieldLiters; 
            m.EveningYieldLiters = vm.EveningYieldLiters; 
            m.Notes = vm.Notes;
            m.FatPercentage = vm.FatPercentage;
            m.ProteinLevel = vm.ProteinLevel;
            m.SolidNotFat = vm.SolidNotFat;
            m.MilkQualityGrade = vm.MilkQualityGrade;

            if (string.IsNullOrWhiteSpace(m.MilkQualityGrade) && m.FatPercentage.HasValue && m.ProteinLevel.HasValue)
            {
                if (m.FatPercentage >= 4.0m && m.ProteinLevel >= 3.5m) m.MilkQualityGrade = "A";
                else if (m.FatPercentage >= 3.2m && m.ProteinLevel >= 3.0m) m.MilkQualityGrade = "B";
                else m.MilkQualityGrade = "C";
            }

            _uow.MilkProductions.Update(m);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var m = await _uow.MilkProductions.GetByIdAsync(id);
            if (m is null) return false;
            _uow.MilkProductions.Delete(m);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MilkDropAlertViewModel>> DetectYieldDropsAsync(int farmId)
        {
            var today = DateTime.Today;
            var threeDaysAgo = today.AddDays(-3);
            var tenDaysAgo = today.AddDays(-10);

            // Fetch records from UoW
            var records = await _uow.MilkProductions.GetByFarmIdAsync(farmId, tenDaysAgo, today);
            var allRecords = records.ToList();

            var grouped = allRecords.GroupBy(r => r.CattleId);
            var alerts = new List<MilkDropAlertViewModel>();

            foreach (var group in grouped)
            {
                var cattle = group.First().Cattle;
                if (cattle == null) continue;

                var recentRecords = group.Where(r => r.Date >= threeDaysAgo).ToList();
                var baselineRecords = group.Where(r => r.Date < threeDaysAgo).ToList();

                if (!recentRecords.Any() || !baselineRecords.Any()) continue;

                var recentAvg = recentRecords.Average(r => r.MorningYieldLiters + r.EveningYieldLiters);
                var baselineAvg = baselineRecords.Average(r => r.MorningYieldLiters + r.EveningYieldLiters);

                if (baselineAvg > 0)
                {
                    var dropPercent = ((baselineAvg - recentAvg) / baselineAvg) * 100;
                    if (dropPercent >= 20)
                    {
                        alerts.Add(new MilkDropAlertViewModel
                        {
                            CattleId = cattle.Id,
                            TagId = cattle.TagId,
                            Name = cattle.Name,
                            Breed = cattle.Breed,
                            BaselineAverage = (decimal)baselineAvg,
                            RecentAverage = (decimal)recentAvg,
                            DropPercentage = (decimal)dropPercent,
                            LastRecordedDate = recentRecords.Max(r => r.Date)
                        });
                    }
                }
            }

            return alerts;
        }
    }
}
