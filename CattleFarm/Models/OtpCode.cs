using System.ComponentModel.DataAnnotations;

namespace CattleFarm.Models
{
    /// <summary>OTP code for email verification and password reset.</summary>
    public class OtpCode
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(6)]
        public string Code { get; set; } = string.Empty;

        /// <summary>Purpose: "EmailVerify" or "PasswordReset"</summary>
        [StringLength(50)]
        public string Purpose { get; set; } = "PasswordReset";

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
