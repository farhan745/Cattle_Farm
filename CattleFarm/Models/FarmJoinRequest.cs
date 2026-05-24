using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>
    /// A worker's request to join a specific farm.
    /// Statuses: Applied → Pending → Accepted | Rejected
    /// Cooldown: Worker cannot re-apply for 7 days after rejection.
    /// </summary>
    public class FarmJoinRequest
    {
        public int Id { get; set; }

        // Who is applying
        public int WorkerUserId { get; set; }
        [ForeignKey(nameof(WorkerUserId))]
        public virtual User? WorkerUser { get; set; }

        // Which farm
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        // Status: Applied | Pending | Accepted | Rejected
        [Required, StringLength(20)]
        public string Status { get; set; } = JoinRequestStatus.Applied;

        [StringLength(500)]
        public string? Message { get; set; }  // optional message from worker

        [StringLength(500)]
        public string? OwnerNote { get; set; } // owner's accept/reject note

        // Timestamps
        public DateTime AppliedAt   { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        // Cooldown: after rejection, worker can re-apply after this date
        public DateTime? CanReApplyAt { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>Join request status constants — no magic strings.</summary>
    public static class JoinRequestStatus
    {
        public const string Applied  = "Applied";
        public const string Pending  = "Pending";
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
    }
}
