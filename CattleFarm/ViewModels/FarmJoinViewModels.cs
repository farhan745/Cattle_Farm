using System.ComponentModel.DataAnnotations;

namespace CattleFarm.ViewModels
{
    // ── Owner creates login for a manually-added worker ───────────────────────

    public class CreateWorkerLoginViewModel
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string? WorkerEmail { get; set; }

        [Required(ErrorMessage = "Username দিতে হবে")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email দিতে হবে")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password দিতে হবে")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password কমপক্ষে 6 অক্ষর হতে হবে")]
        public string Password { get; set; } = string.Empty;

        [Compare(nameof(Password), ErrorMessage = "Password মিলছে না")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // ── FarmJoin ViewModels ───────────────────────────────────────────────────

    public class FarmJoinBrowseViewModel
    {
        public IEnumerable<FarmBrowseItem> Farms { get; set; } = Enumerable.Empty<FarmBrowseItem>();
        public int? MyActiveFarmId { get; set; }
        public string? MyActiveFarmName { get; set; }
    }

    public class FarmBrowseItem
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = string.Empty;
        public string Location    { get; set; } = string.Empty;
        public string? ImagePath  { get; set; }
        public int    WorkerCount { get; set; }
        public string ApplicationStatus { get; set; } = "None"; // None | Pending | Accepted | Rejected | Cooldown
        public DateTime? CooldownEnds { get; set; }
        public bool AlreadyJoined { get; set; }
    }

    public class FarmJoinApplyViewModel
    {
        [Required]
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Message { get; set; }
    }

    public class IncomingRequestViewModel
    {
        public int    Id          { get; set; }
        public int    FarmId      { get; set; }
        public string FarmName    { get; set; } = string.Empty;
        public int    WorkerUserId { get; set; }
        public string WorkerName  { get; set; } = string.Empty;
        public string WorkerEmail { get; set; } = string.Empty;
        public string? Message    { get; set; }
        public string Status      { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
    }

    public class MyJoinRequestViewModel
    {
        public int    Id       { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string Status   { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? CooldownEnds { get; set; }
        public string? ReviewNote { get; set; }
    }
}
