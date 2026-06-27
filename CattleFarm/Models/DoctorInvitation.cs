using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>
    /// Represents a one-time secure invitation sent to a prospective veterinarian.
    /// Admin/Owner creates the invitation → token is emailed → doctor completes profile.
    /// </summary>
    public class DoctorInvitation
    {
        public int Id { get; set; }

        /// <summary>Unique GUID-based token embedded in the invitation URL. Never reusable.</summary>
        [Required, StringLength(64)]
        public string Token { get; set; } = string.Empty;

        // ── Invitee Details ───────────────────────────────────────────────────
        [Required, StringLength(200)]
        public string DoctorName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        // ── Optional Fields ───────────────────────────────────────────────────
        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime? ExpectedJoiningDate { get; set; }

        // ── Lifecycle ─────────────────────────────────────────────────────────
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt  { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool     IsUsed     { get; set; } = false;
        public DateTime? UsedAt    { get; set; }

        public InvitationStatus InvitationStatus { get; set; } = InvitationStatus.Pending;

        // ── Revocation ────────────────────────────────────────────────────────
        public DateTime? RevokedAt       { get; set; }
        public int?      RevokedByUserId { get; set; }

        [ForeignKey(nameof(RevokedByUserId))]
        public virtual User? RevokedByUser { get; set; }

        // ── Relationships ─────────────────────────────────────────────────────
        /// <summary>Optional farm to pre-assign the doctor upon acceptance.</summary>
        public int? FarmId { get; set; }

        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        /// <summary>Admin/Owner who sent this invitation.</summary>
        [Required]
        public int CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User CreatedByUser { get; set; } = null!;

        // ── Navigation ────────────────────────────────────────────────────────
        /// <summary>The Doctor record created upon acceptance (null until then).</summary>
        public virtual Doctor? Doctor { get; set; }
    }
}
