using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class MilkProduction
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 200)]
        public double MorningYieldLiters { get; set; }

        [Range(0, 200)]
        public double EveningYieldLiters { get; set; }

        public double TotalYieldLiters => MorningYieldLiters + EveningYieldLiters;

        [StringLength(500)]
        public string? Notes { get; set; }

        // ── Milk Quality Records ──────────────────────────────────────────────
        [Range(0, 10)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Fat %")]
        public decimal? FatPercentage { get; set; }

        [Range(0, 10)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Protein %")]
        public decimal? ProteinLevel { get; set; }

        [Range(0, 20)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Solid-Not-Fat %")]
        public decimal? SolidNotFat { get; set; }

        [StringLength(50)]
        [Display(Name = "Quality Grade")]
        public string? MilkQualityGrade { get; set; } // e.g. "A", "B", "C"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? RecordedByWorkerId { get; set; }
        [ForeignKey(nameof(RecordedByWorkerId))]
        public virtual Worker? RecordedByWorker { get; set; }
    }
}
