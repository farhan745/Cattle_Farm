using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _img;
        public ProductService(IUnitOfWork uow, IImageService img) { _uow = uow; _img = img; }

        public async Task<IEnumerable<Product>> GetByFarmAsync(int farmId) => await _uow.Products.GetByFarmIdAsync(farmId);
        public async Task<Product?> GetByIdAsync(int id) => await _uow.Products.GetByIdAsync(id);

        public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null, ProductCategory? category = null)
            => await _uow.Products.GetPagedAsync(page, pageSize, farmId, search, category);

        public async Task<Product> CreateAsync(ProductViewModel vm)
        {
            var imagePath = await _img.SaveImageAsync(vm.ImageFile, "products");
            var p = new Product { Name = vm.Name, Category = vm.Category, Description = vm.Description, Price = vm.Price, StockQuantity = vm.StockQuantity, Unit = vm.Unit, MinStockLevel = vm.MinStockLevel, IsAvailable = vm.IsAvailable, IsFeatured = vm.IsFeatured, FarmId = vm.FarmId, ImagePath = imagePath };
            await _uow.Products.AddAsync(p);
            await _uow.SaveChangesAsync();
            return p;
        }

        public async Task<bool> UpdateAsync(int id, ProductViewModel vm)
        {
            var p = await _uow.Products.GetByIdAsync(id);
            if (p is null) return false;
            p.Name = vm.Name; p.Category = vm.Category; p.Description = vm.Description; p.Price = vm.Price; p.StockQuantity = vm.StockQuantity; p.Unit = vm.Unit; p.MinStockLevel = vm.MinStockLevel; p.IsAvailable = vm.IsAvailable; p.IsFeatured = vm.IsFeatured; p.UpdatedAt = DateTime.UtcNow;
            if (vm.ImageFile != null) { _img.DeleteImage(p.ImagePath); p.ImagePath = await _img.SaveImageAsync(vm.ImageFile, "products"); }
            _uow.Products.Update(p);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var p = await _uow.Products.GetByIdAsync(id);
            if (p is null) return false;
            p.IsDeleted = true; p.DeletedAt = DateTime.UtcNow;
            _uow.Products.Update(p);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdjustStockAsync(int productId, double quantity, bool isAddition)
        {
            var p = await _uow.Products.GetByIdAsync(productId);
            if (p is null) return false;
            p.StockQuantity = isAddition ? p.StockQuantity + quantity : p.StockQuantity - quantity;
            if (p.StockQuantity < 0) p.StockQuantity = 0;
            p.UpdatedAt = DateTime.UtcNow;
            _uow.Products.Update(p);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
