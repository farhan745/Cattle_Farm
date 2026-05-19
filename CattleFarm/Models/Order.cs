using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Order
    {
        public int Id { get; set; }

        public OrderStatus   OrderStatus   { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt   { get; set; }

        // FK
        public int CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public virtual User? Customer { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        // Navigation
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Payment>   Payments   { get; set; } = new List<Payment>();
    }
}
