using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class CattleMedicalRecord
    {
        public int Id { get; set; }

        [Required]
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        /// <summary>User.Id of the veterinarian (Role = Doctor).</summary>
        [Required]
        public int DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public virtual User? Doctor { get; set; }

        [Required]
        public DateTime ExaminationDate { get; set; } = DateTime.UtcNow;

        [Required, StringLength(2000)]
        public string ChiefComplaint { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Prescription { get; set; }

        [StringLength(200)]
        public string? MedicineName { get; set; }

        [StringLength(100)]
        public string? MedicineDose { get; set; }

        [StringLength(100)]
        public string? DoseFrequency { get; set; }

        [Range(0, 365)]
        public int DoseDurationDays { get; set; }

        public DateTime? NextVisitDate { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
