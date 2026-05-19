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
            var m = new MilkProduction { CattleId = vm.CattleId, FarmId = vm.FarmId, RecordedByWorkerId = vm.RecordedByWorkerId, Date = vm.Date, MorningYieldLiters = vm.MorningYieldLiters, EveningYieldLiters = vm.EveningYieldLiters, Notes = vm.Notes };
            await _uow.MilkProductions.AddAsync(m);
            await _uow.SaveChangesAsync();
            return m;
        }

        public async Task<bool> UpdateAsync(int id, MilkProductionViewModel vm)
        {
            var m = await _uow.MilkProductions.GetByIdAsync(id);
            if (m is null) return false;
            m.Date = vm.Date; m.MorningYieldLiters = vm.MorningYieldLiters; m.EveningYieldLiters = vm.EveningYieldLiters; m.Notes = vm.Notes;
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
    }
}
