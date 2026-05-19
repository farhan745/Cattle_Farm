using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Application user — owns farms, places orders, holds subscriptions.</summary>
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        /// <summary>BCrypt hashed — never stored plain text.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50)]
        public string Role { get; set; } = "User";

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string? ProfileImagePath { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        // Account status
        public bool IsEmailVerified { get; set; } = false;
        public bool IsActive        { get; set; } = true;
        public int  FailedLoginCount { get; set; } = 0;
        public DateTime? LockedUntil { get; set; }

        // Security
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; }

        // Preferences
        public string PreferredLanguage { get; set; } = "en";   // "en" | "bn"

        // Subscription (denormalized for fast access)
        public string? SubscriptionType   { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }

        // Soft delete + audit
        public bool IsDeleted    { get; set; } = false;
        public DateTime? DeletedAt  { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt  { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        public virtual ICollection<Farm>         Farms         { get; set; } = new List<Farm>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<AuditLog>     AuditLogs     { get; set; } = new List<AuditLog>();
        public virtual ICollection<ActivityLog>  ActivityLogs  { get; set; } = new List<ActivityLog>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<Payment>      Payments      { get; set; } = new List<Payment>();
        public virtual ICollection<Order>        Orders        { get; set; } = new List<Order>();
        public virtual ICollection<Review>       Reviews       { get; set; } = new List<Review>();
    }
}
