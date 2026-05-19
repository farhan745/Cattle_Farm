using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class HealthRecord
    {
        public int Id { get; set; }

        [Required]
        public DateTime RecordDate { get; set; } = DateTime.UtcNow;

        [Range(30.0, 45.0)]
        public double? Temperature { get; set; }   // °C

        [Range(0.1, 5000)]
        public double? Weight { get; set; }        // kg

        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
        public RiskLevel    RiskLevel    { get; set; } = RiskLevel.Low;

        [StringLength(1000)]
        public string? Symptoms { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        [StringLength(2000)]
        public string? VetRecommendation { get; set; }

        public bool IsDeleted  { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

        // FK
        public int CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        public int? DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public virtual Doctor? Doctor { get; set; }
    }
}
