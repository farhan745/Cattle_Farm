using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceViewModel>> GetAllAttendanceAsync(int? currentUserId = null, string? currentUserRole = null);
        Task<IEnumerable<AttendanceViewModel>> GetAttendanceByUserIdAsync(int userId);
        Task<AttendanceViewModel?> GetAttendanceByIdAsync(int id, int? currentUserId = null, string? currentUserRole = null);
        Task MarkAttendanceAsync(AttendanceMarkViewModel model);
        Task EditAttendanceAsync(AttendanceEditViewModel model, int? currentUserId = null, string? currentUserRole = null);
        Task DeleteAttendanceAsync(int id, int? currentUserId = null, string? currentUserRole = null);

        // Bulk marking
        Task<BulkAttendanceViewModel> GetBulkAttendanceFormAsync(DateTime date, int? currentUserId = null, string? currentUserRole = null);
        Task MarkBulkAsync(BulkAttendanceViewModel model, int markedByUserId, string? currentUserRole = null);
    }
}
