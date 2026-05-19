using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Repositories.Implementations
{
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(CattleFarmDbContext context) : base(context) { }

        public async Task<IEnumerable<Expense>> GetByFarmIdAsync(int farmId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(e => e.FarmId == farmId).AsQueryable();
            if (from.HasValue) q = q.Where(e => e.Date >= from.Value);
            if (to.HasValue)   q = q.Where(e => e.Date <= to.Value);
            return await q.OrderByDescending(e => e.Date).ToListAsync();
        }

        public async Task<decimal> GetTotalByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null)
        {
            var q = _dbSet.Where(e => e.FarmId == farmId).AsQueryable();
            if (from.HasValue) q = q.Where(e => e.Date >= from.Value);
            if (to.HasValue)   q = q.Where(e => e.Date <= to.Value);
            return await q.SumAsync(e => e.Amount);
        }

        public async Task<(IEnumerable<Expense> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, ExpenseCategory? category = null)
        {
            var q = _dbSet.AsQueryable();
            if (farmId.HasValue)   q = q.Where(e => e.FarmId == farmId.Value);
            if (category.HasValue) q = q.Where(e => e.Category == category.Value);
            int total = await q.CountAsync();
            var items = await q.OrderByDescending(e => e.Date).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, total);
        }
    }
}
