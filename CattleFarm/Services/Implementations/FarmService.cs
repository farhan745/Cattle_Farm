using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class FarmService : IFarmService
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _imageService;
        private readonly IFarmAccessService _farmAccess;

        public FarmService(IUnitOfWork uow, IImageService imageService, IFarmAccessService farmAccess)
        {
            _uow = uow;
            _imageService = imageService;
            _farmAccess = farmAccess;
        }

        public async Task<IEnumerable<Farm>> GetAllAsync() => await _uow.Farms.GetAllAsync();
        public async Task<IEnumerable<Farm>> GetByOwnerAsync(int ownerId) => await _uow.Farms.GetByOwnerIdAsync(ownerId);
        public async Task<Farm?> GetByIdAsync(int id) => await _uow.Farms.GetByIdAsync(id);
        public async Task<Farm?> GetWithDetailsAsync(int id) => await _uow.Farms.GetWithDetailsAsync(id);

        public async Task<(IEnumerable<Farm> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search = null)
        {
            var items = await _uow.Farms.GetPagedAsync(page, pageSize, search);
            int total = await _uow.Farms.CountAsync();
            return (items, total);
        }

        public async Task<(IEnumerable<Farm> Items, int Total)> GetPagedForUserAsync(int page, int pageSize, int userId, string? role, string? search = null)
        {
            var accessible = (await _farmAccess.GetAccessibleFarmsAsync(userId, role)).ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                accessible = accessible.Where(f =>
                    f.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    f.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var total = accessible.Count;
            var items = accessible
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            return (items, total);
        }

        public async Task<Farm> CreateAsync(FarmViewModel vm, int ownerId)
        {
            var imagePath = await _imageService.SaveImageAsync(vm.ImageFile, "farms");
            var farm = new Farm
            {
                Name = vm.Name, Location = vm.Location, FarmType = vm.FarmType,
                SizeInAcres = vm.SizeInAcres, Capacity = vm.Capacity, Description = vm.Description,
                Latitude = vm.Latitude, Longitude = vm.Longitude, OwnerId = ownerId,
                ImagePath = imagePath, ApprovalStatus = ApprovalStatus.Pending
            };
            await _uow.Farms.AddAsync(farm);
            await _uow.SaveChangesAsync();
            return farm;
        }

        public async Task<bool> UpdateAsync(int id, FarmViewModel vm)
        {
            var farm = await _uow.Farms.GetByIdAsync(id);
            if (farm is null) return false;
            farm.Name = vm.Name; farm.Location = vm.Location; farm.FarmType = vm.FarmType;
            farm.SizeInAcres = vm.SizeInAcres; farm.Capacity = vm.Capacity; farm.Description = vm.Description;
            farm.Latitude = vm.Latitude; farm.Longitude = vm.Longitude; farm.UpdatedAt = DateTime.UtcNow;
            if (vm.ImageFile != null)
            {
                _imageService.DeleteImage(farm.ImagePath);
                farm.ImagePath = await _imageService.SaveImageAsync(vm.ImageFile, "farms");
            }
            _uow.Farms.Update(farm);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var farm = await _uow.Farms.GetByIdAsync(id);
            if (farm is null) return false;
            farm.IsDeleted = true; farm.DeletedAt = DateTime.UtcNow;
            _uow.Farms.Update(farm);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var farm = await _uow.Farms.GetByIdAsync(id);
            if (farm is null) return false;
            farm.ApprovalStatus = ApprovalStatus.Approved; farm.UpdatedAt = DateTime.UtcNow;
            _uow.Farms.Update(farm);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectAsync(int id)
        {
            var farm = await _uow.Farms.GetByIdAsync(id);
            if (farm is null) return false;
            farm.ApprovalStatus = ApprovalStatus.Rejected; farm.UpdatedAt = DateTime.UtcNow;
            _uow.Farms.Update(farm);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
