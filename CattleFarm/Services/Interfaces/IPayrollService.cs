using System.Collections.Generic;
using System.Threading.Tasks;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IPayrollService
    {
        Task<IEnumerable<PayrollViewModel>> GetAllPayrollsAsync();
        Task<PayrollViewModel?> GetPayrollByIdAsync(int id);
        Task<IEnumerable<PayrollViewModel>> GetPayrollsByUserIdAsync(int userId);
        Task<IEnumerable<PayrollViewModel>> GetPayrollsByFarmIdsAsync(IEnumerable<int> farmIds);
        Task GenerateMonthlyPayrollAsync(int year, int month);
        Task UpdatePayrollAsync(PayrollEditViewModel model);
        Task DeletePayrollAsync(int id);
    }
}
