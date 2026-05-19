using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Vaccination
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string VaccineName { get; set; } = string.Empty;

        [Required]
        public DateTime VaccinationDate { get; set; }

        public DateTime? NextDueDate { get; set; }

        [StringLength(200)]
        public string? AdministeredBy { get; set; }

        public int DoseNumber { get; set; } = 1;

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? BatchNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int? DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public virtual Doctor? Doctor { get; set; }
    }
}
