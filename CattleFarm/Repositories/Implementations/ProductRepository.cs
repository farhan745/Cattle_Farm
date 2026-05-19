using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Product>> GetByFarmIdAsync(int farmId)
            => await _dbSet.Where(p => p.FarmId == farmId).Include(p => p.Farm).ToListAsync();

        public async Task<IEnumerable<Product>> GetByCategoryAsync(ProductCategory category)
            => await _dbSet.Where(p => p.Category == category).Include(p => p.Farm).ToListAsync();

        public async Task<IEnumerable<Product>> GetLowStockAsync(int farmId)
            => await _dbSet.Where(p => p.FarmId == farmId && p.StockQuantity <= p.MinStockLevel).ToListAsync();

        public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, string? search = null, ProductCategory? category = null)
        {
            var q = _dbSet.Include(p => p.Farm).AsQueryable();
            if (farmId.HasValue)  q = q.Where(p => p.FarmId == farmId.Value);
            if (!string.IsNullOrWhiteSpace(search)) q = q.Where(p => p.Name.Contains(search));
            if (category.HasValue) q = q.Where(p => p.Category == category.Value);
            int total = await q.CountAsync();
            var items = await q.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }

        public async Task<int> CountByFarmAsync(int farmId)
            => await _dbSet.CountAsync(p => p.FarmId == farmId);
    }
}
