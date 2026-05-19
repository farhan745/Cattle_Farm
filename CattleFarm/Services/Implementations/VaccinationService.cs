using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class VaccinationService : IVaccinationService
    {
        private readonly IUnitOfWork _uow;
        public VaccinationService(IUnitOfWork uow) { _uow = uow; }

        public async Task<IEnumerable<Vaccination>> GetByCattleAsync(int cattleId) => await _uow.Vaccinations.GetByCattleIdAsync(cattleId);
        public async Task<IEnumerable<Vaccination>> GetUpcomingAsync(int daysAhead = 30) => await _uow.Vaccinations.GetUpcomingAsync(daysAhead);
        public async Task<IEnumerable<Vaccination>> GetOverdueAsync() => await _uow.Vaccinations.GetOverdueAsync();
        public async Task<Vaccination?> GetByIdAsync(int id) => await _uow.Vaccinations.GetByIdAsync(id);

        public async Task<Vaccination> CreateAsync(VaccinationViewModel vm)
        {
            var v = new Vaccination { CattleId = vm.CattleId, DoctorId = vm.DoctorId, VaccineName = vm.VaccineName, VaccinationDate = vm.VaccinationDate, NextDueDate = vm.NextDueDate, AdministeredBy = vm.AdministeredBy, DoseNumber = vm.DoseNumber, Notes = vm.Notes, BatchNumber = vm.BatchNumber };
            await _uow.Vaccinations.AddAsync(v);
            await _uow.SaveChangesAsync();
            return v;
        }

        public async Task<bool> UpdateAsync(int id, VaccinationViewModel vm)
        {
            var v = await _uow.Vaccinations.GetByIdAsync(id);
            if (v is null) return false;
            v.VaccineName = vm.VaccineName; v.VaccinationDate = vm.VaccinationDate; v.NextDueDate = vm.NextDueDate; v.AdministeredBy = vm.AdministeredBy; v.DoseNumber = vm.DoseNumber; v.Notes = vm.Notes; v.BatchNumber = vm.BatchNumber;
            _uow.Vaccinations.Update(v);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var v = await _uow.Vaccinations.GetByIdAsync(id);
            if (v is null) return false;
            _uow.Vaccinations.Delete(v);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
