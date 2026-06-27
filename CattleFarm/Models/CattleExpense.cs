using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>
    /// Tracks individual costs tied to a specific cattle (feed, medicine, vet, transport, etc.)
    /// Used by the owner to calculate the final sell price: BuyPrice + TotalCosts + DesiredProfit.
    /// </summary>
    public class CattleExpense
    {
        public int Id { get; set; }

        [Required]
        public CattleExpenseCategory Category { get; set; } = CattleExpenseCategory.Other;

        [Required, Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK → Cattle
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        // FK → User (who added the expense)
        public int? CreatedByUserId { get; set; }
        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedByUser { get; set; }
    }
}
