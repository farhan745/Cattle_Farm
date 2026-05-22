using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly CattleFarmDbContext _context;

        public AttendanceService(CattleFarmDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AttendanceViewModel>> GetAllAttendanceAsync(int? currentUserId = null, string? currentUserRole = null)
        {
            var query = _context.Attendances
                .Include(a => a.Worker)
                .AsQueryable();

            if (currentUserRole != AppRoles.Admin && currentUserId.HasValue)
            {
                query = query.Where(a => a.Worker != null && a.Worker.Farm != null && a.Worker.Farm.OwnerId == currentUserId.Value);
            }

            var records = await query.ToListAsync();

            return records.Select(a => new AttendanceViewModel
            {
                Id = a.Id,
                UserId = a.Worker?.UserId ?? 0,
                WorkerId = a.WorkerId,
                WorkerName = a.Worker?.FullName ?? string.Empty,
                Date = a.Date,
                Status = Enum.TryParse<AttendanceStatus>(a.Status, out var s1) ? s1 : AttendanceStatus.Present,
                Notes = a.Status
            }).ToList();
        }

        public async Task<IEnumerable<AttendanceViewModel>> GetAttendanceByUserIdAsync(int userId)
        {
            var records = await _context.Attendances
                .Include(a => a.Worker)
                .Where(a => a.Worker != null && a.Worker.UserId == userId)
                .ToListAsync();

            return records.Select(a => new AttendanceViewModel
            {
                Id = a.Id,
                UserId = userId,
                WorkerId = a.WorkerId,
                WorkerName = a.Worker?.FullName ?? string.Empty,
                Date = a.Date,
                Status = Enum.TryParse<AttendanceStatus>(a.Status, out var s2) ? s2 : AttendanceStatus.Present,
                Notes = a.Status
            }).ToList();
        }

        public async Task<AttendanceViewModel?> GetAttendanceByIdAsync(int id, int? currentUserId = null, string? currentUserRole = null)
        {
            var query = _context.Attendances
                .Include(a => a.Worker)
                    .ThenInclude(w => w.Farm)
                .AsQueryable();

            if (currentUserRole != AppRoles.Admin && currentUserId.HasValue)
            {
                query = query.Where(a => a.Worker != null && a.Worker.Farm != null && a.Worker.Farm.OwnerId == currentUserId.Value);
            }

            var a = await query.FirstOrDefaultAsync(a => a.Id == id);
            if (a == null) return null;

            return new AttendanceViewModel
            {
                Id = a.Id,
                UserId = a.Worker?.UserId ?? 0,
                WorkerId = a.WorkerId,
                WorkerName = a.Worker?.FullName ?? string.Empty,
                Date = a.Date,
                Status = Enum.TryParse<AttendanceStatus>(a.Status, out var s3) ? s3 : AttendanceStatus.Present,
                Notes = a.Status
            };
        }

        public async Task MarkAttendanceAsync(AttendanceMarkViewModel model)
        {
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.WorkerId == model.WorkerId && a.Date.Date == model.Date.Date);

            if (existing != null)
            {
                existing.Status = model.Status.ToString();
                existing.MarkedAt = DateTime.UtcNow;
                _context.Attendances.Update(existing);
            }
            else
            {
                var newRecord = new Attendance
                {
                    WorkerId = model.WorkerId,
                    Date = model.Date.Date,
                    Status = model.Status.ToString(),
                    MarkedByUserId = 1,
                    MarkedAt = DateTime.UtcNow
                };
                await _context.Attendances.AddAsync(newRecord);
            }
            await _context.SaveChangesAsync();
        }

        public async Task EditAttendanceAsync(AttendanceEditViewModel model, int? currentUserId = null, string? currentUserRole = null)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Worker)
                    .ThenInclude(w => w.Farm)
                .FirstOrDefaultAsync(a => a.Id == model.Id);

            if (attendance != null)
            {
                if (currentUserRole != AppRoles.Admin && currentUserId.HasValue)
                {
                    if (attendance.Worker?.Farm == null || attendance.Worker.Farm.OwnerId != currentUserId.Value)
                    {
                        throw new UnauthorizedAccessException("You do not have permission to edit this attendance record.");
                    }
                }

                attendance.Status = model.Status.ToString();
                attendance.Date = model.Date.Date;
                attendance.MarkedAt = DateTime.UtcNow;
                _context.Attendances.Update(attendance);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAttendanceAsync(int id, int? currentUserId = null, string? currentUserRole = null)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Worker)
                    .ThenInclude(w => w.Farm)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance != null)
            {
                if (currentUserRole != AppRoles.Admin && currentUserId.HasValue)
                {
                    if (attendance.Worker?.Farm == null || attendance.Worker.Farm.OwnerId != currentUserId.Value)
                    {
                        throw new UnauthorizedAccessException("You do not have permission to delete this attendance record.");
                    }
                }

                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();
            }
        }

        // ── Get Bulk Form with Existing Attendance Statuses populated ──
        public async Task<BulkAttendanceViewModel> GetBulkAttendanceFormAsync(DateTime date, int? currentUserId = null, string? currentUserRole = null)
        {
            var targetDate = date.Date;

            // Retrieve active workers
            var query = _context.Workers.Where(w => w.IsActive && !w.IsDeleted);

            if (currentUserRole != AppRoles.Admin && currentUserId.HasValue)
            {
                query = query.Where(w => w.Farm != null && w.Farm.OwnerId == currentUserId.Value);
            }

            var workers = await query.ToListAsync();

            // Retrieve existing attendance records for the target date
            var existingRecords = await _context.Attendances
                .Where(a => a.Date.Date == targetDate)
                .ToDictionaryAsync(a => a.WorkerId, a => a.Status);

            var formModel = new BulkAttendanceViewModel
            {
                Date = targetDate,
                Rows = workers.Select(w => new AttendanceRowViewModel
                {
                    WorkerId = w.Id,
                    WorkerName = w.FullName,
                    Role = w.Role,
                    Status = existingRecords.ContainsKey(w.Id) ? existingRecords[w.Id] : "Present"
                }).ToList()
            };

            return formModel;
        }

        // ── Save Bulk Attendance (UPSERT) ──
        public async Task MarkBulkAsync(BulkAttendanceViewModel model, int markedByUserId, string? currentUserRole = null)
        {
            var targetDate = model.Date.Date;

            List<int>? allowedWorkerIds = null;
            if (currentUserRole != AppRoles.Admin)
            {
                allowedWorkerIds = await _context.Workers
                    .Where(w => w.Farm != null && w.Farm.OwnerId == markedByUserId)
                    .Select(w => w.Id)
                    .ToListAsync();
            }

            foreach (var row in model.Rows)
            {
                if (allowedWorkerIds != null && !allowedWorkerIds.Contains(row.WorkerId))
                {
                    continue; // Skip workers outside the owner's farm
                }

                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.WorkerId == row.WorkerId && a.Date.Date == targetDate);

                if (existing != null)
                {
                    existing.Status = row.Status;
                    existing.MarkedByUserId = markedByUserId;
                    existing.MarkedAt = DateTime.UtcNow;
                    _context.Attendances.Update(existing);
                }
                else
                {
                    var newAttendance = new Attendance
                    {
                        WorkerId = row.WorkerId,
                        Date = targetDate,
                        Status = row.Status,
                        MarkedByUserId = markedByUserId,
                        MarkedAt = DateTime.UtcNow
                    };
                    await _context.Attendances.AddAsync(newAttendance);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
