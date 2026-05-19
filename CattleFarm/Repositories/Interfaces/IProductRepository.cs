using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Product>> GetByCategoryAsync(ProductCategory category);
        Task<IEnumerable<Product>> GetLowStockAsync(int farmId);
        Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null, ProductCategory? category = null);
        Task<int> CountByFarmAsync(int farmId);
    }
}
