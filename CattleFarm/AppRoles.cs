/// <summary>All role constants in the system. Use these everywhere — no magic strings.</summary>
public static class AppRoles
{
    public const string Admin    = "Admin";
    public const string Manager  = "Manager";
    public const string Owner    = "Owner";
    public const string Doctor   = "Doctor";
    public const string Worker   = "Worker";
    public const string Member   = "Member";
    public const string Customer = "Customer";
    public const string User_Role = "User";      // default for new registrations

    // Composite strings for [Authorize(Roles = ...)] attributes (must be compile-time constants)
    public const string AdminOrManager        = "Admin,Manager";
    public const string AdminOrOwner          = "Admin,Owner";
    public const string AdminManagerOrOwner   = "Admin,Manager,Owner";
    public const string AdminManagerOwnerDoctor = "Admin,Manager,Owner,Doctor";

    /// <summary>All roles in display order (highest privilege first).</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin, Manager, Owner, Doctor, Worker, Member, Customer, User_Role
    };

    /// <summary>Roles that can register and manage farms.</summary>
    public static readonly IReadOnlyList<string> FarmRoles = new[] { Admin, Manager, Owner };
}
