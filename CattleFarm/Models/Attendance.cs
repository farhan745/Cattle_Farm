using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Represents worker daily attendance marked by Admin/Owner</summary>
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WorkerId { get; set; }

        [ForeignKey(nameof(WorkerId))]
        public virtual Worker? Worker { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = "Present"; // Present, Absent, Late

        [Required]
        public int MarkedByUserId { get; set; }

        [ForeignKey(nameof(MarkedByUserId))]
        public virtual User? MarkedByUser { get; set; }

        [Required]
        public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
    }
}
