using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Worker
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Role { get; set; } = string.Empty;   // e.g. "Milker", "Feeder", "Guard"

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(200)]
        public string? Email { get; set; }

        [StringLength(1000)]
        public string? Skills { get; set; }

        public int ExperienceYears { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public string? ImagePath { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsActive    { get; set; } = true;
        public bool IsDeleted   { get; set; } = false;
        public DateTime? DeletedAt  { get; set; }
        public DateTime HiredAt     { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt  { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // FK
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        // Navigation
        public virtual ICollection<WorkerAttendance> Attendances      { get; set; } = new List<WorkerAttendance>();
        public virtual ICollection<MilkProduction>   MilkProductions  { get; set; } = new List<MilkProduction>();
    }
}
