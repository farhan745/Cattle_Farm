using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required, StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type  { get; set; } = NotificationType.System;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt    { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

        // Generic pointer to the triggering entity
        [StringLength(100)]
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId      { get; set; }

        // FK
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
