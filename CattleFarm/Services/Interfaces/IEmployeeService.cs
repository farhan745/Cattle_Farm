using System.Collections.Generic;
using System.Threading.Tasks;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeViewModel>> GetAllEmployeesAsync();
        Task<EmployeeViewModel?> GetEmployeeByIdAsync(int id);
        Task CreateEmployeeAsync(EmployeeCreateViewModel model);
        Task UpdateEmployeeAsync(EmployeeEditViewModel model);
        Task DeleteEmployeeAsync(int id);
    }
}
