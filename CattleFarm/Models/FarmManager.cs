using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Manager employed on a specific farm (not the farm owner).</summary>
    public class FarmManager
    {
        public int Id { get; set; }

        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        public int ManagerUserId { get; set; }
        [ForeignKey(nameof(ManagerUserId))]
        public virtual User? ManagerUser { get; set; }

        [StringLength(100)]
        public string Position { get; set; } = "Farm Manager";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        [StringLength(500)]
        public string? RemovalNote { get; set; }

        public bool RemovedByOwner { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }

    public static class JoinApplicantRole
    {
        public const string Worker  = "Worker";
        public const string Manager = "Manager";
    }
}
