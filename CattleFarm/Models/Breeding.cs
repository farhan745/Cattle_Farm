using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public enum BreedingMethod { Natural, ArtificialInsemination }
    public enum BreedingOutcome { Pending, Successful, Unsuccessful, Miscarried }

    public class Breeding
    {
        public int Id { get; set; }

        [Required]
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int? SireId { get; set; }
        [ForeignKey(nameof(SireId))]
        public virtual Cattle? Sire { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        [Required]
        public DateTime BreedingDate { get; set; }

        public DateTime? ExpectedCalvingDate { get; set; }
        public DateTime? ActualCalvingDate   { get; set; }

        public BreedingMethod  Method  { get; set; } = BreedingMethod.Natural;
        public BreedingOutcome Outcome { get; set; } = BreedingOutcome.Pending;

        public int? CalvesCount { get; set; }

        [StringLength(200)]
        public string? SireBreed { get; set; }

        [StringLength(200)]
        public string? InseminationTechnician { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cost { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
