namespace CattleFarm.Models
{
    public class Driver
    {
        public int    Id                { get; set; }
        public string FullName          { get; set; } = string.Empty;
        public string Phone             { get; set; } = string.Empty;
        public string LicenseNumber     { get; set; } = string.Empty;
        public string? LicenseType      { get; set; }       // e.g. "Commercial", "Light"
        public int    ExperienceYears   { get; set; }
        public string? Address          { get; set; }
        public decimal Rating           { get; set; } = 5m;  // 1–5 stars
        public DriverStatus Status      { get; set; } = DriverStatus.Available;
        public int?   AssignedVehicleId { get; set; }
        public string? Notes            { get; set; }
        public string? ImagePath        { get; set; }
        public bool   IsDeleted         { get; set; } = false;
        public DateTime CreatedAt       { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt       { get; set; } = DateTime.UtcNow;

        // Navigation
        public Vehicle?           AssignedVehicle  { get; set; }
        public ICollection<Trip>  Trips            { get; set; } = new List<Trip>();
    }
}
