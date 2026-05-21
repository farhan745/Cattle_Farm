namespace CattleFarm.Models
{
    public class TransportRequest
    {
        public int    Id                 { get; set; }
        public TransportType RequestType { get; set; } = TransportType.General;
        public string PickupLocation     { get; set; } = string.Empty;
        public string Destination        { get; set; } = string.Empty;
        public DateTime ScheduledDate    { get; set; }
        public TimeSpan? ScheduledTime   { get; set; }
        public decimal? EstimatedDistanceKm { get; set; }
        public decimal? CargoWeight      { get; set; }           // tonnes
        public string? CargoDescription  { get; set; }
        public TripStatus Status         { get; set; } = TripStatus.Pending;
        public string? Notes             { get; set; }
        public int?   OrderId            { get; set; }           // linked order (optional)
        public int?   FarmId             { get; set; }
        public int    RequestedByUserId  { get; set; }
        public bool   IsDeleted          { get; set; } = false;
        public DateTime CreatedAt        { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt        { get; set; } = DateTime.UtcNow;

        // Navigation
        public Order?  Order             { get; set; }
        public Farm?   Farm              { get; set; }
        public User?   RequestedByUser   { get; set; }
        public Trip?   Trip              { get; set; }
    }
}
