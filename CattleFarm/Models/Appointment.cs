using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [Required, StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Notes { get; set; }

        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt   { get; set; }

        // FK
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public virtual Doctor? Doctor { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedByUser { get; set; }
    }
}
