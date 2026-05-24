using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using CattleFarm.Hubs;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly CattleFarmDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationService _notifications;
        private readonly IHubContext<FarmDashboardHub> _hub;

        public TaskAssignmentService(
            CattleFarmDbContext context,
            IWebHostEnvironment env,
            INotificationService notifications,
            IHubContext<FarmDashboardHub> hub)
        {
            _context = context;
            _env     = env;
            _notifications = notifications;
            _hub = hub;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<TaskViewModel>> GetAllTasksAsync()
        {
            return await _context.TaskAssignments
                .Where(t => !t.IsDeleted)
                .Include(t => t.Worker)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => MapToViewModel(t))
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskViewModel>> GetTasksByUserIdAsync(int userId)
        {
            return await _context.TaskAssignments
                .Where(t => !t.IsDeleted && t.AssignedUserId == userId)
                .Include(t => t.Worker)
                .OrderBy(t => t.DueDate)
                .Select(t => MapToViewModel(t))
                .ToListAsync();
        }

        public async Task<TaskViewModel?> GetTaskByIdAsync(int id)
        {
            var t = await _context.TaskAssignments
                .Include(t => t.Worker)
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            return t == null ? null : MapToViewModel(t);
        }

        public async Task<IEnumerable<TaskViewModel>> GetOpenTasksAsync(int farmId)
        {
            // Open tasks belong to a farm — filter by the farm's owner who created the task
            var farm = await _context.Farms.FindAsync(farmId);
            if (farm == null) return Enumerable.Empty<TaskViewModel>();

            return await _context.TaskAssignments
                .Where(t => !t.IsDeleted
                         && t.TaskType == TaskTypes.Open
                         && t.Status   == Models.TaskStatus.Pending
                         && t.FarmId == farmId
                         && (t.ExpiresAt == null || t.ExpiresAt > DateTime.UtcNow))
                .OrderBy(t => t.Priority == TaskPriority.Emergency ? 0
                            : t.Priority == TaskPriority.High      ? 1
                            : t.Priority == TaskPriority.Medium    ? 2 : 3)
                .ThenBy(t => t.DueDate)
                .Select(t => MapToViewModel(t))
                .ToListAsync();
        }

        // ── Write ─────────────────────────────────────────────────────────────

        public async Task<int> CreateTaskAsync(TaskAssignViewModel model, int createdByUserId)
        {
            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.Id == model.FarmId && !f.IsDeleted && f.IsActive);

            var creator = await _context.Users.FindAsync(createdByUserId);
            var canManageFarm = creator?.Role == AppRoles.Admin || farm?.OwnerId == createdByUserId;

            if (farm == null || !canManageFarm)
                throw new UnauthorizedAccessException("You can only create tasks for farms you own.");

            var task = new TaskAssignment
            {
                Title            = model.Title,
                Description      = model.Description,
                Priority         = model.Priority,
                TaskType         = TaskTypes.Open,
                FarmId           = model.FarmId,
                AssignedWorkerId = null,
                AssignedUserId   = 0,
                DueDate          = model.DueDate,
                ExpiresAt        = model.ExpiresAt,
                BonusAmount      = model.BonusAmount,
                Status           = Models.TaskStatus.Pending,
                AssignedAt       = DateTime.UtcNow,
                CreatedAt        = DateTime.UtcNow,
                CreatedBy        = createdByUserId
            };

            await _context.TaskAssignments.AddAsync(task);
            await _context.SaveChangesAsync();

            var availableWorkers = await _context.FarmWorkers
                .Where(fw => fw.FarmId == model.FarmId &&
                             fw.IsActive &&
                             fw.WorkerStatus == WorkerStatusType.Available)
                .Select(fw => fw.WorkerUserId)
                .Distinct()
                .ToListAsync();

            var notificationType = task.Priority == TaskPriority.Emergency
                ? NotificationType.EmergencyTask
                : NotificationType.NewTask;

            foreach (var workerUserId in availableWorkers)
            {
                await _notifications.SendAsync(
                    workerUserId,
                    task.Priority == TaskPriority.Emergency ? "Emergency task available" : "New task available",
                    $"{task.Title} is open for acceptance.",
                    notificationType,
                    nameof(TaskAssignment),
                    task.Id);
            }

            await _hub.Clients.Group(FarmDashboardHub.FarmGroup(model.FarmId))
                .SendAsync("TaskCreated", new { task.Id, task.Title, task.Priority, task.Status });

            return task.Id;
        }

        public async Task UpdateTaskAsync(TaskAssignViewModel model)
        {
            var existing = await _context.TaskAssignments
                .FirstOrDefaultAsync(t => t.Id == model.Id && !t.IsDeleted);

            if (existing == null) return;

            var farm = await _context.Farms
                .FirstOrDefaultAsync(f => f.Id == model.FarmId && !f.IsDeleted && f.IsActive);

            var editor = await _context.Users.FindAsync(existing.CreatedBy);
            var canEditFarm = editor?.Role == AppRoles.Admin || farm?.OwnerId == existing.CreatedBy;

            if (farm == null || !canEditFarm)
                throw new UnauthorizedAccessException("Invalid farm for this task.");

            existing.Title            = model.Title;
            existing.Description      = model.Description;
            existing.Priority         = model.Priority;
            existing.TaskType         = TaskTypes.Open;
            existing.FarmId           = model.FarmId;
            existing.DueDate          = model.DueDate;
            existing.ExpiresAt        = model.ExpiresAt;
            existing.BonusAmount      = model.BonusAmount;
            existing.Status           = model.Status ?? existing.Status;
            existing.UpdatedAt        = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _context.TaskAssignments
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

            if (task == null) return;

            task.IsDeleted = true;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ── Worker actions ────────────────────────────────────────────────────

        public async Task<bool> AcceptTaskAsync(int taskId, int workerUserId)
        {
            // Use a transaction to prevent race conditions (first-come-first-served)
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var task = await _context.TaskAssignments
                    .FirstOrDefaultAsync(t => t.Id == taskId
                                           && !t.IsDeleted
                                           && t.Status == Models.TaskStatus.Pending
                                           && (t.ExpiresAt == null || t.ExpiresAt > DateTime.UtcNow));

                if (task == null) return false; // already taken or expired

                var worker = await _context.Workers
                    .FirstOrDefaultAsync(w => w.UserId == workerUserId &&
                                              w.FarmId == task.FarmId &&
                                              w.IsActive &&
                                              !w.IsDeleted);

                var farmWorker = await _context.FarmWorkers
                    .Include(fw => fw.WorkerUser)
                    .FirstOrDefaultAsync(fw => fw.FarmId == task.FarmId &&
                                               fw.WorkerUserId == workerUserId &&
                                               fw.IsActive &&
                                               fw.WorkerStatus == WorkerStatusType.Available);

                if (farmWorker == null) return false;

                if (worker == null)
                {
                    worker = new Worker
                    {
                        FullName = farmWorker.WorkerUser?.FullName ?? "Worker",
                        Role = farmWorker.Position,
                        Email = farmWorker.WorkerUser?.Email,
                        Phone = farmWorker.WorkerUser?.PhoneNumber,
                        Salary = farmWorker.Salary,
                        FarmId = task.FarmId,
                        UserId = workerUserId,
                        IsActive = true,
                        HiredAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Workers.AddAsync(worker);
                    await _context.SaveChangesAsync();
                }

                var hasActiveTask = await _context.TaskAssignments.AnyAsync(t =>
                    !t.IsDeleted &&
                    t.AssignedUserId == workerUserId &&
                    t.Id != task.Id &&
                    (t.Status == Models.TaskStatus.Accepted ||
                     t.Status == Models.TaskStatus.InProgress ||
                     t.Status == Models.TaskStatus.ProofSubmitted));

                if (hasActiveTask) return false;

                task.AssignedWorkerId = worker.Id;
                task.AssignedUserId   = workerUserId;
                task.Status           = Models.TaskStatus.Accepted;
                task.AcceptedAt       = DateTime.UtcNow;
                task.UpdatedAt        = DateTime.UtcNow;
                farmWorker.WorkerStatus = WorkerStatusType.Busy;
                farmWorker.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _notifications.SendAsync(
                    task.CreatedBy,
                    "Task accepted",
                    $"{worker.FullName} accepted {task.Title}.",
                    NotificationType.TaskAccepted,
                    nameof(TaskAssignment),
                    task.Id);

                await _hub.Clients.Group(FarmDashboardHub.FarmGroup(task.FarmId))
                    .SendAsync("TaskAccepted", new { task.Id, task.AssignedUserId, WorkerName = worker.FullName });

                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, int workerUserId, string status)
        {
            var task = await _context.TaskAssignments
                .FirstOrDefaultAsync(t => t.Id == taskId
                                       && t.AssignedUserId == workerUserId
                                       && !t.IsDeleted);

            if (task == null) return false;

            // Only allow moving to InProgress (not directly to Completed — proof required)
            if (status == Models.TaskStatus.Completed) return false;

            task.Status    = status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubmitProofAsync(SubmitProofViewModel model, int workerUserId)
        {
            var task = await _context.TaskAssignments
                .FirstOrDefaultAsync(t => t.Id == model.TaskId
                                       && t.AssignedUserId == workerUserId
                                       && !t.IsDeleted
                                       && t.Status != Models.TaskStatus.Completed
                                       && t.Status != Models.TaskStatus.Expired);

            if (task == null) return false;

            // Save proof image if provided
            if (model.ProofImage != null && model.ProofImage.Length > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "task-proofs");
                Directory.CreateDirectory(uploadDir);

                var fileName  = $"proof_{task.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(model.ProofImage.FileName)}";
                var filePath  = Path.Combine(uploadDir, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await model.ProofImage.CopyToAsync(stream);

                task.ProofImagePath = $"/uploads/task-proofs/{fileName}";
            }

            task.ProofNote          = model.ProofNote;
            task.Status             = Models.TaskStatus.ProofSubmitted;
            task.ProofSubmittedAt   = DateTime.UtcNow;
            task.UpdatedAt          = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _notifications.SendAsync(
                task.CreatedBy,
                "Task proof submitted",
                $"Proof was submitted for {task.Title}.",
                NotificationType.TaskCompleted,
                nameof(TaskAssignment),
                task.Id);

            return true;
        }

        public async Task<bool> ReviewProofAsync(ReviewProofViewModel model, int ownerUserId)
        {
            var task = await _context.TaskAssignments
                .Include(t => t.Farm)
                .Include(t => t.Worker)
                .FirstOrDefaultAsync(t => t.Id == model.TaskId
                                       && !t.IsDeleted
                                       && t.Status == Models.TaskStatus.ProofSubmitted);

            if (task == null) return false;

            // Ownership check — only the owner of the farm can approve
            if (task.Farm?.OwnerId != ownerUserId) return false;

            if (model.Approve)
            {
                task.Status        = Models.TaskStatus.Completed;
                task.CompletedAt   = DateTime.UtcNow;
                task.BonusApproved = task.BonusAmount > 0;
                task.BonusNote     = model.Note;

                if (task.BonusAmount > 0 && !task.BonusPaid)
                {
                    if (!task.AssignedWorkerId.HasValue) return false;

                    var now = DateTime.UtcNow;
                    var farmWorker = await _context.FarmWorkers
                        .FirstOrDefaultAsync(fw => fw.FarmId == task.FarmId &&
                                                   fw.WorkerUserId == task.AssignedUserId &&
                                                   fw.IsActive);

                    var salary = await _context.SalaryHistories
                        .FirstOrDefaultAsync(s => s.WorkerId == task.AssignedWorkerId.Value &&
                                                  s.Year == now.Year &&
                                                  s.Month == now.Month);

                    if (salary == null)
                    {
                        salary = new SalaryHistory
                        {
                            FarmId = task.FarmId,
                            WorkerId = task.AssignedWorkerId.Value,
                            WorkerUserId = task.AssignedUserId,
                            Year = now.Year,
                            Month = now.Month,
                            BaseSalary = farmWorker?.Salary ?? 0,
                            Bonus = 0,
                            TotalSalary = farmWorker?.Salary ?? 0,
                            UpdatedByUserId = ownerUserId
                        };
                        await _context.SalaryHistories.AddAsync(salary);
                    }

                    salary.Bonus += task.BonusAmount;
                    salary.TotalSalary = salary.BaseSalary + salary.Bonus;
                    salary.TaskAssignmentId = task.Id;
                    salary.UpdatedByUserId = ownerUserId;

                    var payroll = await _context.Payrolls
                        .FirstOrDefaultAsync(p => p.WorkerId == task.AssignedWorkerId.Value &&
                                                  p.Year == now.Year &&
                                                  p.Month == now.Month &&
                                                  !p.IsDeleted);
                    if (payroll == null)
                    {
                        payroll = new Payroll
                        {
                            WorkerId = task.AssignedWorkerId.Value,
                            UserId = task.AssignedUserId,
                            FarmId = task.FarmId,
                            Year = now.Year,
                            Month = now.Month,
                            BaseSalary = salary.BaseSalary,
                            Bonus = 0,
                            OvertimePay = 0,
                            OvertimeHours = 0,
                            Deductions = 0,
                            GeneratedAt = DateTime.UtcNow
                        };
                        await _context.Payrolls.AddAsync(payroll);
                    }

                    payroll.Bonus += task.BonusAmount;
                    payroll.NetSalary = payroll.BaseSalary + payroll.OvertimePay + payroll.Bonus - payroll.Deductions;
                    payroll.UpdatedAt = DateTime.UtcNow;

                    task.BonusPaid = true;
                    task.Status = Models.TaskStatus.BonusAdded;
                }
            }
            else
            {
                task.Status           = Models.TaskStatus.Rejected;
                task.RejectionReason  = model.Note;
                task.BonusApproved    = false;
            }

            task.UpdatedAt = DateTime.UtcNow;

            var workerMembership = await _context.FarmWorkers
                .FirstOrDefaultAsync(fw => fw.FarmId == task.FarmId &&
                                           fw.WorkerUserId == task.AssignedUserId &&
                                           fw.IsActive);
            if (workerMembership != null && workerMembership.WorkerStatus == WorkerStatusType.Busy)
            {
                workerMembership.WorkerStatus = WorkerStatusType.Available;
                workerMembership.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            await _notifications.SendAsync(
                task.AssignedUserId,
                model.Approve ? "Task approved" : "Task proof rejected",
                model.Approve
                    ? $"{task.Title} was approved and salary was updated."
                    : $"{task.Title} needs more work.",
                model.Approve && task.BonusAmount > 0 ? NotificationType.BonusAdded : NotificationType.Warning,
                nameof(TaskAssignment),
                task.Id);

            await _hub.Clients.Group(FarmDashboardHub.FarmGroup(task.FarmId))
                .SendAsync("TaskReviewed", new { task.Id, task.Status, task.AssignedUserId });

            return true;
        }

        // ── Mapper ────────────────────────────────────────────────────────────

        private static TaskViewModel MapToViewModel(TaskAssignment t) => new TaskViewModel
        {
            Id                = t.Id,
            FarmId            = t.FarmId,
            Title             = t.Title,
            Description       = t.Description,
            AssignedWorkerId  = t.AssignedWorkerId ?? 0,
            AssignedUserId    = t.AssignedUserId,
            WorkerName        = t.Worker?.FullName ?? string.Empty,
            Priority          = t.Priority,
            TaskType          = t.TaskType,
            Status            = t.Status,
            DueDate           = t.DueDate,
            ExpiresAt         = t.ExpiresAt,
            AssignedAt        = t.AssignedAt,
            AcceptedAt        = t.AcceptedAt,
            CompletedAt       = t.CompletedAt,
            ProofSubmittedAt  = t.ProofSubmittedAt,
            BonusAmount       = t.BonusAmount,
            BonusApproved     = t.BonusApproved,
            BonusPaid         = t.BonusPaid,
            BonusNote         = t.BonusNote,
            ProofImagePath    = t.ProofImagePath,
            ProofNote         = t.ProofNote,
            RejectionReason   = t.RejectionReason,
            CreatedBy         = t.CreatedBy,
            CreatedAt         = t.CreatedAt
        };
    }
}
