using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public enum SensorReadingType { BarnTemperature, BarnHumidity, Ammonia, WaterLevel, FeedBinLevel, MilkTankTemperature }
    public enum FeedingCommandStatus { Pending, Sent, Acknowledged, Failed, Cancelled }
    public enum OfflineSyncStatus { Pending, Processing, Synced, Failed }

    public class SensorReading
    {
        public int Id { get; set; }

        [Required]
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        [Required, StringLength(80)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public SensorReadingType ReadingType { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Value { get; set; }

        [Required, StringLength(20)]
        public string Unit { get; set; } = string.Empty;

        [Required]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [StringLength(120)]
        public string? BarnZone { get; set; }
    }

    public class GpsTrackerSnapshot
    {
        public int Id { get; set; }

        [Required]
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        [Required, StringLength(80)]
        public string TrackerId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpeedKph { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }

    public class AutomatedFeedingCommand
    {
        public int Id { get; set; }

        [Required]
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        [Required, StringLength(80)]
        public string ControllerId { get; set; } = string.Empty;

        [Required, StringLength(120)]
        public string FeedName { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public double QuantityKg { get; set; }

        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
        public FeedingCommandStatus Status { get; set; } = FeedingCommandStatus.Pending;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    public class MilkMachineImport
    {
        public int Id { get; set; }

        [Required]
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int? CattleId { get; set; }
        [ForeignKey(nameof(CattleId))]
        public virtual Cattle? Cattle { get; set; }

        [Required, StringLength(80)]
        public string MachineId { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal YieldLiters { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? FatPercentage { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? ProteinPercentage { get; set; }

        public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
        public bool ConvertedToMilkRecord { get; set; }
    }

    public class OfflineSyncItem
    {
        public int Id { get; set; }

        [Required]
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        [Required, StringLength(80)]
        public string ClientId { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public string PayloadJson { get; set; } = string.Empty;

        public OfflineSyncStatus Status { get; set; } = OfflineSyncStatus.Pending;
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string? ErrorMessage { get; set; }
    }
}
