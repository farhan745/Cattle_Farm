using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public ProductCategory Category { get; set; } = ProductCategory.Other;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, 1_000_000)]
        public double StockQuantity { get; set; }

        [StringLength(50)]
        public string Unit { get; set; } = "kg";   // e.g. "kg", "liter", "piece"

        public double MinStockLevel { get; set; } = 0;

        public string? ImagePath { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsDeleted   { get; set; } = false;
        public DateTime? DeletedAt  { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt  { get; set; }
        public bool IsFeatured { get; set; } = false;

        // FK
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        // Navigation
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Review>    Reviews    { get; set; } = new List<Review>();
    }
}
