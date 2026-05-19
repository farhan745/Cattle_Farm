using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class WorkerAttendance
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public DateTime? CheckIn  { get; set; }
        public DateTime? CheckOut { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public double? HoursWorked { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int WorkerId { get; set; }
        [ForeignKey(nameof(WorkerId))]
        public virtual Worker? Worker { get; set; }
    }
}
