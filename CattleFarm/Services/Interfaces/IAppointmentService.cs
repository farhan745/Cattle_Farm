using CattleFarm.Models;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<Appointment>> GetByFarmAsync(int farmId);
        Task<IEnumerable<Appointment>> GetUpcomingAsync(int farmId, int daysAhead = 7);
        Task<Appointment?> GetByIdAsync(int id);
        Task<Appointment>  CreateAsync(AppointmentViewModel vm, int createdByUserId);
        Task<bool>         UpdateStatusAsync(int id, AppointmentStatus status, string? notes);
        Task<bool>         DeleteAsync(int id);
        Task<(IEnumerable<Appointment> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, AppointmentStatus? status = null);
    }
}
