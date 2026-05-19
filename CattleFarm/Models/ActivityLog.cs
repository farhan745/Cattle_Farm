using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string? EntityName { get; set; }

        public int? EntityId { get; set; }

        [StringLength(50)]
        public string? IPAddress { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // FK
        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
