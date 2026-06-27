using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class FeedInventory
    {
        public int Id { get; set; }

        [Required]
        public int FarmId { get; set; }

        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        [Required]
        public FeedType FeedType { get; set; } = FeedType.Hay;

        [Required]
        [Range(0.0, 1000000.0)]
        public double StockQuantityKg { get; set; }

        [Required]
        [Range(0.0, 100000.0)]
        public double MinStockThresholdKg { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
