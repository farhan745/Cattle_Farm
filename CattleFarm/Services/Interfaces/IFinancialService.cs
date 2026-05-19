using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IFinancialService
    {
        // Expenses
        Task<IEnumerable<Expense>> GetExpensesAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<Expense>  CreateExpenseAsync(ExpenseViewModel vm, int userId);
        Task<bool>     UpdateExpenseAsync(int id, ExpenseViewModel vm);
        Task<bool>     DeleteExpenseAsync(int id);

        // Revenue
        Task<IEnumerable<Revenue>> GetRevenuesAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<Revenue>  CreateRevenueAsync(RevenueViewModel vm, int userId);
        Task<bool>     DeleteRevenueAsync(int id);

        // Summary
        Task<decimal>  GetTotalExpensesAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<decimal>  GetTotalRevenueAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<decimal>  GetNetProfitAsync(int farmId, DateTime? from = null, DateTime? to = null);
        Task<List<CattleFarm.ViewModels.MonthlyTrendItem>> GetMonthlyTrendAsync(int farmId, int months = 12);
    }
}
