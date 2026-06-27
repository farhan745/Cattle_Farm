using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetByFarmAsync(int farmId);
        Task<IEnumerable<Appointment>> GetUpcomingAsync(int farmId, int daysAhead = 7);
        Task<Appointment?> GetByIdAsync(int id);
        Task<Appointment> CreateAsync(AppointmentViewModel vm, int createdByUserId, string? userRole);
        Task<bool> ApproveAsync(int id, int doctorUserId);
        Task<bool> RejectAsync(int id, int doctorUserId, string? reason);
        Task<bool> CompleteAsync(CompleteAppointmentViewModel vm, int doctorUserId);
        Task<bool> CancelAsync(int id, int userId, string? userRole);
        Task<bool> DeleteAsync(int id);
        Task<(IEnumerable<Appointment> Items, int Total)> GetPagedAsync(
            int page, int pageSize,
            int? farmId,
            AppointmentStatus? status,
            int? doctorId,
            IReadOnlyCollection<int>? ownerFarmIds);
        Task<bool> CanViewAsync(int appointmentId, int userId, string? userRole);
    }
}
