namespace CattleFarm.Models
{
    public class Trip
    {
        public int    Id                   { get; set; }
        public int    TransportRequestId   { get; set; }
        public int    VehicleId            { get; set; }
        public int    DriverId             { get; set; }
        public TripStatus Status           { get; set; } = TripStatus.Assigned;
        public DateTime? StartTime         { get; set; }
        public DateTime? EndTime           { get; set; }
        public decimal DistanceKm          { get; set; }
        public decimal FuelCostPerKm       { get; set; }
        public decimal BaseCost            { get; set; }
        public decimal FuelCost            { get; set; }
        public decimal AdditionalCost      { get; set; }
        public decimal TotalCost           { get; set; }
        public string? AdditionalCostNote  { get; set; }
        public string? RouteNotes          { get; set; }
        public string? Notes               { get; set; }
        public bool   IsDeleted            { get; set; } = false;
        public DateTime CreatedAt          { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt          { get; set; } = DateTime.UtcNow;

        // Navigation
        public TransportRequest TransportRequest { get; set; } = null!;
        public Vehicle          Vehicle          { get; set; } = null!;
        public Driver           Driver           { get; set; } = null!;
    }
}
