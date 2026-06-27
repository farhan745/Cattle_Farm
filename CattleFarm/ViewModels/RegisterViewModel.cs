using System.ComponentModel.DataAnnotations;

namespace CattleFarm.ViewModels
{
    /// <summary>
    /// View model for the Register form.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Full name too long.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3–50 characters.")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Account Type")]
        public string Role { get; set; } = "User";

        [Display(Name = "Profile Photo")]
        public IFormFile? ProfileImage { get; set; }

        // Veterinarian-only fields (when Role = Doctor)
        [StringLength(200)]
        public string? Specialization { get; set; }

        [Range(0.01, 100000)]
        public decimal ConsultationFee { get; set; } = 500;

        [StringLength(500)]
        public string? AvailableTimeSlot { get; set; }

        [Range(0, 60)]
        public int YearsOfExperience { get; set; }

        [StringLength(100)]
        public string? LicenseNumber { get; set; }
    }
}
