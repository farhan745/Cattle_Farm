using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class MedicineRecord
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string MedicineName { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int? PrescribedByDoctorId { get; set; }
        [ForeignKey(nameof(PrescribedByDoctorId))]
        public virtual Doctor? PrescribedByDoctor { get; set; }
    }
}
