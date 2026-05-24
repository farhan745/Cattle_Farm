using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>
    /// Junction table: a worker officially belongs to a farm.
    /// Created when owner accepts a FarmJoinRequest.
    /// Supports soft-remove (owner removes worker, or worker leaves voluntarily).
    /// </summary>
    public class FarmWorker
    {
        public int Id { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int WorkerUserId { get; set; }
        [ForeignKey(nameof(WorkerUserId))]
        public virtual User? WorkerUser { get; set; }

        // Worker position on this farm
        [StringLength(100)]
        public string Position { get; set; } = WorkerPosition.Feeder;

        // Worker status on this farm
        [StringLength(50)]
        public string WorkerStatus { get; set; } = WorkerStatusType.Available;

        // Contract details
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public DateTime JoinedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt   { get; set; }

        // Reason when removed/left
        [StringLength(500)]
        public string? RemovalNote { get; set; }

        // Who removed (owner or self)
        public bool RemovedByOwner { get; set; } = false;

        public bool IsActive   { get; set; } = true;
        public bool IsDeleted  { get; set; } = false;
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>Worker position constants.</summary>
    public static class WorkerPosition
    {
        public const string Feeder             = "Feeder";
        public const string Cleaner            = "Cleaner";
        public const string Milker             = "Milker";
        public const string VeterinaryAssistant = "VeterinaryAssistant";
    }

    /// <summary>Worker status constants.</summary>
    public static class WorkerStatusType
    {
        public const string Available = "Available";
        public const string Busy      = "Busy";
        public const string Offline   = "Offline";
        public const string OnLeave   = "OnLeave";
    }
}
