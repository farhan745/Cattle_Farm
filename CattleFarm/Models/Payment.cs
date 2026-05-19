using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentMethod Method  { get; set; } = PaymentMethod.Cash;
        public PaymentStatus Status  { get; set; } = PaymentStatus.Pending;
        public PaymentPurpose Purpose { get; set; } = PaymentPurpose.Other;

        [StringLength(200)]
        public string? TransactionId { get; set; }

        // Generic reference to whatever entity this payment covers
        public int? ReferenceId { get; set; }

        [StringLength(100)]
        public string? ReferenceType { get; set; }   // "Order", "Subscription", etc.

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // FK
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        public int? OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Order? Order { get; set; }
    }
}
