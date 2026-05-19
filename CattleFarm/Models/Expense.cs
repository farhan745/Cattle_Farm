using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? ReceiptImagePath { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsDeleted  { get; set; } = false;
        public DateTime? DeletedAt  { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // FK
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedByUser { get; set; }
    }
}
