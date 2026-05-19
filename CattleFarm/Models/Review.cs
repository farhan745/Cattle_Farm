using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Review
    {
        public int Id { get; set; }

        public ReviewTargetType TargetType { get; set; }

        public int TargetId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(2000)]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsDeleted  { get; set; } = false;
        public DateTime? DeletedAt  { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;

        // FK
        public int ReviewerId { get; set; }
        [ForeignKey(nameof(ReviewerId))]
        public virtual User? Reviewer { get; set; }
    }
}
