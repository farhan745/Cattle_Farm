using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int WorkerUserId { get; set; }
        [ForeignKey(nameof(WorkerUserId))]
        public virtual User? WorkerUser { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = LeaveRequestStatus.Pending;

        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(500)]
        public string? OwnerNote { get; set; }

        public int? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public static class LeaveRequestStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
    }
}
