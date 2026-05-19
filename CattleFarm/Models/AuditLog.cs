using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Action { get; set; } = string.Empty;    // e.g. "Created", "Updated", "Deleted"

        [Required, StringLength(200)]
        public string EntityName { get; set; } = string.Empty;

        public int? EntityId { get; set; }

        public string? OldValues { get; set; }   // JSON snapshot
        public string? NewValues { get; set; }   // JSON snapshot

        [StringLength(50)]
        public string? IPAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // FK
        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
