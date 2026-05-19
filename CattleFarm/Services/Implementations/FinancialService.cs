using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class FinancialService : IFinancialService
    {
        private readonly IUnitOfWork _uow;
        public FinancialService(IUnitOfWork uow) { _uow = uow; }

        public async Task<IEnumerable<Expense>> GetExpensesAsync(int farmId, DateTime? from = null, DateTime? to = null)
            => await _uow.Expenses.GetByFarmIdAsync(farmId, from, to);

        public async Task<Expense> CreateExpenseAsync(ExpenseViewModel vm, int userId)
        {
            var expense = new Expense { FarmId = vm.FarmId, Category = vm.Category, Amount = vm.Amount, Date = vm.Date, Description = vm.Description, CreatedByUserId = userId };
            await _uow.Expenses.AddAsync(expense);
            await _uow.SaveChangesAsync();
            return expense;
        }

        public async Task<bool> UpdateExpenseAsync(int id, ExpenseViewModel vm)
        {
            var e = await _uow.Expenses.GetByIdAsync(id);
            if (e is null) return false;
            e.Category = vm.Category; e.Amount = vm.Amount; e.Date = vm.Date; e.Description = vm.Description;
            _uow.Expenses.Update(e);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteExpenseAsync(int id)
        {
            var e = await _uow.Expenses.GetByIdAsync(id);
            if (e is null) return false;
            e.IsDeleted = true; e.DeletedAt = DateTime.UtcNow;
            _uow.Expenses.Update(e);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Revenue>> GetRevenuesAsync(int farmId, DateTime? from = null, DateTime? to = null)
            => await _uow.Revenues.GetByFarmIdAsync(farmId, from, to);

        public async Task<Revenue> CreateRevenueAsync(RevenueViewModel vm, int userId)
        {
            var rev = new Revenue { FarmId = vm.FarmId, Source = vm.Source, Amount = vm.Amount, Date = vm.Date, Description = vm.Description, OrderId = vm.OrderId, CreatedByUserId = userId };
            await _uow.Revenues.AddAsync(rev);
            await _uow.SaveChangesAsync();
            return rev;
        }

        public async Task<bool> DeleteRevenueAsync(int id)
        {
            var r = await _uow.Revenues.GetByIdAsync(id);
            if (r is null) return false;
            r.IsDeleted = true; r.DeletedAt = DateTime.UtcNow;
            _uow.Revenues.Update(r);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalExpensesAsync(int farmId, DateTime? from = null, DateTime? to = null)
            => await _uow.Expenses.GetTotalByFarmAsync(farmId, from, to);

        public async Task<decimal> GetTotalRevenueAsync(int farmId, DateTime? from = null, DateTime? to = null)
            => await _uow.Revenues.GetTotalByFarmAsync(farmId, from, to);

        public async Task<decimal> GetNetProfitAsync(int farmId, DateTime? from = null, DateTime? to = null)
            => await GetTotalRevenueAsync(farmId, from, to) - await GetTotalExpensesAsync(farmId, from, to);

        public async Task<List<ViewModels.MonthlyTrendItem>> GetMonthlyTrendAsync(int farmId, int months = 12)
        {
            var result = new List<ViewModels.MonthlyTrendItem>();
            for (int i = months - 1; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var from = new DateTime(date.Year, date.Month, 1);
                var to   = from.AddMonths(1).AddDays(-1);
                var rev  = await GetTotalRevenueAsync(farmId, from, to);
                var exp  = await GetTotalExpensesAsync(farmId, from, to);
                result.Add(new ViewModels.MonthlyTrendItem { Month = from.ToString("MMM yyyy"), Revenue = rev, Expense = exp });
            }
            return result;
        }
    }
}
