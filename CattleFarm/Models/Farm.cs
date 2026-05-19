using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Farm entity — one user (Owner) can have many farms.</summary>
    public class Farm
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Location { get; set; } = string.Empty;

        public FarmType FarmType { get; set; } = FarmType.Mixed;

        [Range(0.1, 1_000_000)]
        public double SizeInAcres { get; set; }

        [Range(1, 100_000)]
        public int Capacity { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? ImagePath { get; set; }

        // GPS
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public bool IsActive   { get; set; } = true;
        public bool IsDeleted  { get; set; } = false;
        public DateTime? DeletedAt  { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt  { get; set; }

        // FK → Owner
        public int OwnerId { get; set; }
        [ForeignKey(nameof(OwnerId))]
        public virtual User? Owner { get; set; }

        // Navigation
        public virtual ICollection<Cattle>          Cattles          { get; set; } = new List<Cattle>();
        public virtual ICollection<Worker>          Workers          { get; set; } = new List<Worker>();
        public virtual ICollection<Doctor>          Doctors          { get; set; } = new List<Doctor>();
        public virtual ICollection<Product>         Products         { get; set; } = new List<Product>();
        public virtual ICollection<Expense>         Expenses         { get; set; } = new List<Expense>();
        public virtual ICollection<Revenue>         Revenues         { get; set; } = new List<Revenue>();
        public virtual ICollection<Order>           Orders           { get; set; } = new List<Order>();
        public virtual ICollection<MilkProduction>  MilkProductions  { get; set; } = new List<MilkProduction>();
        public virtual ICollection<Appointment>     Appointments     { get; set; } = new List<Appointment>();
        public virtual ICollection<Review>          Reviews          { get; set; } = new List<Review>();
    }
}
