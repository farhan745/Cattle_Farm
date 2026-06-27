using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Bull semen quality / performance evaluation record.</summary>
    public class BullPerformanceRecord
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Evaluation Date")]
        public DateTime EvaluationDate { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Motility %")]
        public decimal? MotilityPercent { get; set; }

        [Range(0, 100)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Morphology %")]
        public decimal? MorphologyPercent { get; set; }

        [Range(0, 5000)]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Concentration (Million/mL)")]
        public decimal? ConcentrationMillionPerMl { get; set; }

        [Range(0, 20)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Volume (mL)")]
        public decimal? VolumeML { get; set; }

        [Display(Name = "Semen Quality")]
        public SemenQuality QualityGrade { get; set; } = SemenQuality.Good;

        [StringLength(100)]
        [Display(Name = "Evaluated By (Lab/Vet)")]
        public string? EvaluatedBy { get; set; }

        [Range(0, 1000)]
        [Display(Name = "Doses Collected")]
        public int? DosesCollected { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Evaluation Cost")]
        public decimal? Cost { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK — must be a bull (Gender.Male, Category.Bull)
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }
    }
}
