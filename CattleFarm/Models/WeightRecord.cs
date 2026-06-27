using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Per-cattle body weight entry for growth chart tracking.</summary>
    public class WeightRecord
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Measurement Date")]
        public DateTime MeasuredAt { get; set; }

        [Required]
        [Range(1, 5000)]
        [Column(TypeName = "decimal(8,2)")]
        [Display(Name = "Weight (kg)")]
        public decimal WeightKg { get; set; }

        [StringLength(200)]
        [Display(Name = "Body Condition Score (1-9)")]
        public string? BodyConditionScore { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? RecordedByUserId { get; set; }
        [ForeignKey(nameof(RecordedByUserId))]
        public virtual User? RecordedByUser { get; set; }
    }
}
