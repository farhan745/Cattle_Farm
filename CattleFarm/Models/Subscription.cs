using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;

        [Required]
        public DateTime StartDate  { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsActive   { get; set; } = true;
        public bool AutoRenew  { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePaid { get; set; }

        [StringLength(200)]
        public string? TransactionRef { get; set; }

        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // FK
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
