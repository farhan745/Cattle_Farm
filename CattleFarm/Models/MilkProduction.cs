using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class MilkProduction
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, 200)]
        public double MorningYieldLiters { get; set; }

        [Range(0, 200)]
        public double EveningYieldLiters { get; set; }

        public double TotalYieldLiters => MorningYieldLiters + EveningYieldLiters;

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

        public int? RecordedByWorkerId { get; set; }
        [ForeignKey(nameof(RecordedByWorkerId))]
        public virtual Worker? RecordedByWorker { get; set; }
    }
}
