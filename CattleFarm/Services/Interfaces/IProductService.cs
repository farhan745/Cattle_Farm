using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetByFarmAsync(int farmId);
        Task<Product?> GetByIdAsync(int id);
        Task<Product>  CreateAsync(ProductViewModel vm);
        Task<bool>     UpdateAsync(int id, ProductViewModel vm);
        Task<bool>     DeleteAsync(int id);
        Task<bool>     AdjustStockAsync(int productId, double quantity, bool isAddition);
        Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null, ProductCategory? category = null);
    }
}
