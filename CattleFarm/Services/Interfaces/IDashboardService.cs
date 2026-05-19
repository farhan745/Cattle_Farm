using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel>   GetAdminDashboardAsync();
        Task<OwnerDashboardViewModel>   GetOwnerDashboardAsync(int ownerId, int? farmId = null);
        Task<WorkerDashboardViewModel>  GetWorkerDashboardAsync(int workerId);
        Task<DoctorDashboardViewModel>  GetDoctorDashboardAsync(int doctorId);
        Task<CustomerDashboardViewModel> GetCustomerDashboardAsync(int customerId);
    }
}
