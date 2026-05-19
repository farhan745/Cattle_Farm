using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetByFarmIdAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<decimal>  GetTotalByFarmAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<(IEnumerable<Expense> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, ExpenseCategory? category = null);
    }
}
