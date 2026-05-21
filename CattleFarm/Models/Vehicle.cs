namespace CattleFarm.Models
{
    public class Vehicle
    {
        public int    Id                 { get; set; }
        public string Name               { get; set; } = string.Empty;
        public VehicleType   Type        { get; set; } = VehicleType.Truck;
        public string RegistrationNumber { get; set; } = string.Empty;
        public decimal Capacity          { get; set; }           // tonnes or units
        public string? CapacityUnit      { get; set; } = "tonnes";
        public FuelType FuelType         { get; set; } = FuelType.Diesel;
        public decimal FuelCostPerKm     { get; set; }           // local currency per km
        public VehicleStatus Status      { get; set; } = VehicleStatus.Available;
        public string? ImagePath         { get; set; }
        public string? Notes             { get; set; }
        public bool   IsDeleted          { get; set; } = false;
        public DateTime CreatedAt        { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt        { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Trip>  Trips    { get; set; } = new List<Trip>();
        public Driver?            Driver   { get; set; }
    }
}
