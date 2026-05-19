using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public enum FeedType { Hay, Silage, Concentrate, Grain, Supplement, Mineral, Pasture, Other }

    public class FeedRecord
    {
        public int Id { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int? RecordedByWorkerId { get; set; }
        [ForeignKey(nameof(RecordedByWorkerId))]
        public virtual Worker? RecordedByWorker { get; set; }

        [Required]
        public FeedType FeedType { get; set; } = FeedType.Hay;

        [Required, StringLength(200)]
        public string FeedName { get; set; } = string.Empty;

        [Required, Range(0.01, 100000)]
        public double QuantityKg { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Range(0, double.MaxValue)]
        public decimal CostPerKg { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost => (decimal)QuantityKg * CostPerKg;

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Supplier { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
