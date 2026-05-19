using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class CattleService : ICattleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _img;
        public CattleService(IUnitOfWork uow, IImageService img) { _uow = uow; _img = img; }

        public async Task<(IEnumerable<Cattle> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null, int? farmId = null, CattleStatus? status = null)
            => await _uow.Cattles.GetPagedAsync(page, pageSize, search, farmId, status);

        public async Task<IEnumerable<Cattle>> GetByFarmIdAsync(int farmId) => await _uow.Cattles.GetByFarmIdAsync(farmId);
        public async Task<IEnumerable<Cattle>> SearchAsync(string keyword) => await _uow.Cattles.SearchAsync(keyword);
        public async Task<IEnumerable<Cattle>> GetListedForSaleAsync() => await _uow.Cattles.GetListedForSaleAsync();
        public async Task<Cattle?> GetByIdAsync(int id) => await _uow.Cattles.GetByIdAsync(id);
        public async Task<Cattle?> GetWithDetailsAsync(int id) => await _uow.Cattles.GetWithDetailsAsync(id);

        public async Task<Cattle> CreateAsync(CattleViewModel vm)
        {
            var imagePath = await _img.SaveImageAsync(vm.ImageFile, "cattle");
            var cattle = new Cattle
            {
                TagId = vm.TagId, Name = vm.Name, Breed = vm.Breed, DateOfBirth = vm.DateOfBirth,
                Weight = vm.Weight, Gender = vm.Gender, HealthStatus = vm.HealthStatus,
                Status = vm.Status, FarmId = vm.FarmId, PurchasePrice = vm.PurchasePrice,
                SalePrice = vm.SalePrice, SaleDate = vm.SaleDate, PurchaseDate = vm.PurchaseDate,
                Description = vm.Description, IsListedForSale = vm.IsListedForSale,
                IsPremiumListing = vm.IsPremiumListing, ImagePath = imagePath
            };
            await _uow.Cattles.AddAsync(cattle);
            await _uow.SaveChangesAsync();
            return cattle;
        }

        public async Task<bool> UpdateAsync(int id, CattleViewModel vm)
        {
            var cattle = await _uow.Cattles.GetByIdAsync(id);
            if (cattle is null) return false;
            cattle.TagId = vm.TagId; cattle.Name = vm.Name; cattle.Breed = vm.Breed;
            cattle.DateOfBirth = vm.DateOfBirth; cattle.Weight = vm.Weight;
            cattle.Gender = vm.Gender; cattle.HealthStatus = vm.HealthStatus;
            cattle.Status = vm.Status; cattle.PurchasePrice = vm.PurchasePrice;
            cattle.SalePrice = vm.SalePrice; cattle.SaleDate = vm.SaleDate;
            cattle.PurchaseDate = vm.PurchaseDate; cattle.Description = vm.Description;
            cattle.IsListedForSale = vm.IsListedForSale; cattle.IsPremiumListing = vm.IsPremiumListing;
            cattle.UpdatedAt = DateTime.UtcNow;
            if (vm.ImageFile != null) { _img.DeleteImage(cattle.ImagePath); cattle.ImagePath = await _img.SaveImageAsync(vm.ImageFile, "cattle"); }
            _uow.Cattles.Update(cattle);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var cattle = await _uow.Cattles.GetByIdAsync(id);
            if (cattle is null) return false;
            cattle.IsDeleted = true; cattle.DeletedAt = DateTime.UtcNow;
            _uow.Cattles.Update(cattle);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(int id)
        {
            var cattle = await _uow.Cattles.GetByIdAsync(id);
            if (cattle is null) return false;
            cattle.IsDeleted = false; cattle.DeletedAt = null; cattle.UpdatedAt = DateTime.UtcNow;
            _uow.Cattles.Update(cattle);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task UpdateHealthStatusAsync(int cattleId)
        {
            var cattle = await _uow.Cattles.GetByIdAsync(cattleId);
            if (cattle is null) return;
            var latest = await _uow.HealthRecords.GetLatestByCattleIdAsync(cattleId);
            if (latest is not null) cattle.HealthStatus = latest.HealthStatus;
            _uow.Cattles.Update(cattle);
            await _uow.SaveChangesAsync();
        }
    }
}
