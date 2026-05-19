using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>Full cattle record — the core entity of the platform.</summary>
    public class Cattle
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Tag ID")]
        public string TagId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [Display(Name = "Cattle Name")]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Breed { get; set; } = string.Empty;

        [Required, DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Range(0.1, 5000)]
        [Display(Name = "Weight (kg)")]
        public double Weight { get; set; }

        public Gender       Gender       { get; set; } = Gender.Female;
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
        public CattleStatus Status       { get; set; } = CattleStatus.Active;

        public string? ImagePath { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Purchase Price")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Sale Price")]
        public decimal? SalePrice { get; set; }

        public DateTime? SaleDate    { get; set; }
        public DateTime? PurchaseDate { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        // Marketplace
        public bool IsListedForSale   { get; set; } = false;
        public bool IsPremiumListing  { get; set; } = false;

        // Approval
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;

        // Soft delete + audit
        public bool IsDeleted    { get; set; } = false;
        public DateTime? DeletedAt  { get; set; }
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt  { get; set; }

        // FK
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        // Navigation
        public virtual ICollection<HealthRecord>   HealthRecords   { get; set; } = new List<HealthRecord>();
        public virtual ICollection<Vaccination>    Vaccinations    { get; set; } = new List<Vaccination>();
        public virtual ICollection<MedicineRecord> MedicineRecords { get; set; } = new List<MedicineRecord>();
        public virtual ICollection<MilkProduction> MilkProductions { get; set; } = new List<MilkProduction>();
        public virtual ICollection<Appointment>    Appointments    { get; set; } = new List<Appointment>();
        public virtual ICollection<Review>         Reviews         { get; set; } = new List<Review>();
    }
}