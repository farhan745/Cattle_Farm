using CattleFarm.Models;

namespace CattleFarm.Repositories.Interfaces
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetByFarmIdAsync(int farmId);
        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<Appointment>> GetByCattleIdAsync(int cattleId);
        Task<IEnumerable<Appointment>> GetUpcomingAsync(int farmId, int daysAhead = 7);
        Task<(IEnumerable<Appointment> Items, int Total)> GetPagedAsync(int page, int pageSize, int? farmId = null, AppointmentStatus? status = null);
    }
}
