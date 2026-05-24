using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class WorkerPerformanceHistory
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        [ForeignKey(nameof(WorkerId))]
        public virtual Worker? Worker { get; set; }
        public int FarmId { get; set; }
        public int CompletedTasks { get; set; }
        public int RejectedTasks { get; set; }
        public decimal BonusEarned { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class TaskHistory
    {
        public int Id { get; set; }
        public int TaskAssignmentId { get; set; }
        public int FarmId { get; set; }
        public int? WorkerId { get; set; }
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public int? ChangedByUserId { get; set; }
    }

    public class AttendanceHistory
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public int FarmId { get; set; }
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CattleHealthHistory
    {
        public int Id { get; set; }
        public int CattleId { get; set; }
        public int FarmId { get; set; }
        [StringLength(100)]
        public string HealthStatus { get; set; } = string.Empty;
        [StringLength(1000)]
        public string? Notes { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
