using System.ComponentModel.DataAnnotations;

namespace CattleFarm.ViewModels
{
    /// <summary>
    /// View model used by the role-change form in the User Management panel.
    /// </summary>
    public class ChangeRoleViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string NewRole { get; set; } = string.Empty;
    }
}
