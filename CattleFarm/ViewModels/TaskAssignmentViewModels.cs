using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CattleFarm.ViewModels
{
    // ── Create / Edit task ───────────────────────────────────────────────────

    public class TaskAssignViewModel
    {
        public int Id { get; set; }
        public int FarmId { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        // Owner does not choose a worker. First available worker to accept wins.
        public int AssignedWorkerId { get; set; }

        [Required]
        public string Priority { get; set; } = Models.TaskPriority.Medium;

        // Always Open for the smart task workflow.
        [Required]
        public string TaskType { get; set; } = Models.TaskTypes.Open;

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(3);

        // Optional auto-expire (only relevant for Open tasks)
        public DateTime? ExpiresAt { get; set; }

        [Range(0, 9999999)]
        public decimal BonusAmount { get; set; } = 0;

        // Used only on edit
        public string? Status { get; set; }
    }

    // ── Read / display task ──────────────────────────────────────────────────

    public class TaskViewModel
    {
        public int    Id               { get; set; }
        public int    FarmId           { get; set; }
        public string Title            { get; set; } = string.Empty;
        public string? Description     { get; set; }
        public int    AssignedWorkerId { get; set; }
        public int    AssignedUserId   { get; set; }
        public string WorkerName       { get; set; } = string.Empty;
        public string Priority         { get; set; } = Models.TaskPriority.Medium;
        public string TaskType         { get; set; } = Models.TaskTypes.Direct;
        public string Status           { get; set; } = Models.TaskStatus.Pending;
        public DateTime DueDate        { get; set; }
        public DateTime? ExpiresAt     { get; set; }
        public DateTime AssignedAt     { get; set; }
        public DateTime? AcceptedAt    { get; set; }
        public DateTime? CompletedAt   { get; set; }
        public DateTime? ProofSubmittedAt { get; set; }
        public decimal BonusAmount     { get; set; }
        public bool   BonusApproved    { get; set; }
        public bool   BonusPaid        { get; set; }
        public string? BonusNote       { get; set; }
        public string? ProofImagePath  { get; set; }
        public string? ProofNote       { get; set; }
        public string? RejectionReason { get; set; }
        public int    CreatedBy        { get; set; }
        public DateTime CreatedAt      { get; set; }

        // Computed helpers
        public bool IsOverdue    => DueDate.Date < DateTime.Today && Status != Models.TaskStatus.Completed && Status != Models.TaskStatus.Expired;
        public bool HasBonus     => BonusAmount > 0;
        public bool IsOpen       => TaskType == Models.TaskTypes.Open &&
                                    (Status == Models.TaskStatus.Pending || Status == Models.TaskStatus.Open);
    }

    // ── Submit proof ─────────────────────────────────────────────────────────

    public class SubmitProofViewModel
    {
        [Required]
        public int TaskId { get; set; }

        [StringLength(500)]
        public string? ProofNote { get; set; }

        // Image file upload
        public IFormFile? ProofImage { get; set; }
    }

    // ── Owner approve / reject bonus ─────────────────────────────────────────

    public class ReviewProofViewModel
    {
        [Required]
        public int  TaskId      { get; set; }
        public bool Approve     { get; set; }   // true = approve, false = reject

        [StringLength(500)]
        public string? Note     { get; set; }   // rejection reason or approval note
    }

    // ── Open task board (worker browse) ──────────────────────────────────────

    public class OpenTaskBoardViewModel
    {
        public IEnumerable<TaskViewModel> OpenTasks       { get; set; } = Enumerable.Empty<TaskViewModel>();
        public IEnumerable<TaskViewModel> MyActiveTasks   { get; set; } = Enumerable.Empty<TaskViewModel>();
        public IEnumerable<TaskViewModel> MyCompletedTasks { get; set; } = Enumerable.Empty<TaskViewModel>();
    }
}
