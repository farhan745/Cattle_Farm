using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>
    /// Phase 2 — Smart Task System
    /// Adds: Priority, Bonus, Deadline/Expiry, Acceptance workflow,
    ///       Proof upload, Owner approval, Auto-expire support
    /// </summary>
    public class TaskAssignment
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        // ── Assignment target ─────────────────────────────────────────────────
        // For "Open" tasks: AssignedWorkerId = 0, AssignedUserId = 0 until accepted
        public int? AssignedWorkerId { get; set; }
        public int AssignedUserId  { get; set; }
        public int FarmId { get; set; }

        // ── Priority ─────────────────────────────────────────────────────────
        // Low | Medium | High | Emergency
        [Required, StringLength(20)]
        public string Priority { get; set; } = TaskPriority.Medium;

        // ── Task type ─────────────────────────────────────────────────────────
        // Direct   — owner assigns to a specific worker
        // Open     — any farm worker can accept (first-come-first-served)
        [Required, StringLength(20)]
        public string TaskType { get; set; } = TaskTypes.Direct;

        // ── Status ────────────────────────────────────────────────────────────
        // Pending | Accepted | InProgress | ProofSubmitted | Approved | BonusAdded | Rejected | Expired
        [Required, StringLength(30)]
        public string Status { get; set; } = TaskStatus.Pending;

        // ── Dates ─────────────────────────────────────────────────────────────
        public DateTime DueDate    { get; set; }
        public DateTime? ExpiresAt { get; set; }   // auto-expire deadline (null = no auto-expire)
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt   { get; set; }
        public DateTime? CompletedAt  { get; set; }
        public DateTime? ProofSubmittedAt { get; set; }

        // ── Bonus ─────────────────────────────────────────────────────────────
        [Column(TypeName = "decimal(18,2)")]
        public decimal BonusAmount { get; set; } = 0;

        public bool   BonusApproved  { get; set; } = false;
        public bool   BonusPaid      { get; set; } = false;
        public string? BonusNote     { get; set; }   // owner's note when approving/rejecting

        // ── Proof upload ──────────────────────────────────────────────────────
        public string? ProofImagePath  { get; set; }
        public string? ProofNote       { get; set; }   // worker's note when submitting proof

        // ── Rejection / review ────────────────────────────────────────────────
        public string? RejectionReason { get; set; }  // owner fills this when rejecting proof
        public DateTime? DeletedAt { get; set; }

        // ── Audit / soft delete ───────────────────────────────────────────────
        public bool      IsDeleted  { get; set; } = false;
        public int       CreatedBy  { get; set; }   // UserId of owner/manager who created
        public DateTime  CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt  { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────
        [ForeignKey(nameof(AssignedWorkerId))]
        public virtual Worker? Worker { get; set; }

        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }
    }

    // ── Constant bags (use instead of magic strings) ──────────────────────────

    public static class TaskPriority
    {
        public const string Low       = "Low";
        public const string Medium    = "Medium";
        public const string High      = "High";
        public const string Emergency = "Emergency";
    }

    public static class TaskTypes
    {
        public const string Direct = "Direct";  // assigned to specific worker
        public const string Open   = "Open";    // any worker can claim
    }

    public static class TaskStatus
    {
        public const string Pending         = "Pending";        // awaiting acceptance
        public const string Open            = "Open";           // legacy awaiting acceptance
        public const string Accepted        = "Accepted";       // worker accepted
        public const string InProgress      = "InProgress";     // worker started
        public const string ProofSubmitted  = "ProofSubmitted"; // worker uploaded proof
        public const string Completed       = "Completed";      // owner approved
        public const string Approved        = "Approved";       // owner approved
        public const string BonusAdded      = "BonusAdded";     // payroll updated
        public const string Rejected        = "Rejected";       // owner rejected proof
        public const string Expired         = "Expired";        // auto-expired
    }
}
