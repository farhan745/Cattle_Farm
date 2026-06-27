using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Heat detection / estrus cycle monitoring record for a cow.</summary>
    public class HeatRecord
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Observation Date")]
        public DateTime ObservationDate { get; set; }

        [Required]
        [Display(Name = "Heat Status")]
        public HeatStatus HeatStatus { get; set; } = HeatStatus.InHeat;

        [Range(0, 48)]
        [Display(Name = "Heat Duration (hrs)")]
        public double? HeatDurationHours { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Next Expected Heat")]
        public DateTime? NextExpectedHeatDate { get; set; }

        [StringLength(200)]
        [Display(Name = "Observed By")]
        public string? ObservedBy { get; set; }

        [StringLength(20)]
        [Display(Name = "Detection Method")]
        public string? DetectionMethod { get; set; }  // Visual, Pedometer, Tail Paint, etc.

        public bool ReadyForBreeding { get; set; } = false;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }
    }
}
