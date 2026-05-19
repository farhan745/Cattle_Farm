using System.ComponentModel.DataAnnotations;
using CattleFarm.Models;

namespace CattleFarm.ViewModels
{
    public class FarmViewModel
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

        public double? Latitude  { get; set; }
        public double? Longitude { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string?   ExistingImagePath { get; set; }
    }

    public class CattleViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string TagId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Breed { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Range(0.1, 5000)]
        public double Weight { get; set; }

        public Gender       Gender       { get; set; } = Gender.Female;
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
        public CattleStatus Status       { get; set; } = CattleStatus.Active;

        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        public decimal? SalePrice   { get; set; }
        public DateTime? SaleDate   { get; set; }
        public DateTime? PurchaseDate { get; set; }

        [StringLength(2000)]
        public string? Description  { get; set; }

        public bool IsListedForSale  { get; set; } = false;
        public bool IsPremiumListing { get; set; } = false;

        public int FarmId { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string?   ExistingImagePath { get; set; }
    }

    public class WorkerViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(200)]
        public string? Email { get; set; }

        [StringLength(1000)]
        public string? Skills { get; set; }

        public int     ExperienceYears { get; set; }
        public decimal Salary          { get; set; }
        public bool    IsAvailable     { get; set; } = true;
        public int     FarmId          { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string?   ExistingImagePath { get; set; }
    }

    public class DoctorViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(200)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? LicenseNumber { get; set; }

        public decimal ConsultationFee { get; set; }
        public bool    IsAvailable     { get; set; } = true;
        public int?    FarmId          { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string?   ExistingImagePath { get; set; }
    }

    public class HealthRecordViewModel
    {
        public int Id { get; set; }
        [Required] public int CattleId { get; set; }
        public int?    DoctorId    { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.Now;
        [Range(30, 45)] public double? Temperature { get; set; }
        [Range(0.1, 5000)] public double? Weight   { get; set; }
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
        public RiskLevel    RiskLevel    { get; set; } = RiskLevel.Low;

        [StringLength(1000)] public string? Symptoms          { get; set; }
        [StringLength(2000)] public string? Notes             { get; set; }
        [StringLength(2000)] public string? VetRecommendation { get; set; }
    }

    public class VaccinationViewModel
    {
        public int Id { get; set; }
        [Required] public int CattleId { get; set; }
        public int?    DoctorId { get; set; }
        [Required, StringLength(200)] public string VaccineName { get; set; } = string.Empty;
        [Required] public DateTime VaccinationDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        [StringLength(200)] public string? AdministeredBy { get; set; }
        public int DoseNumber { get; set; } = 1;
        [StringLength(500)] public string? Notes       { get; set; }
        [StringLength(100)] public string? BatchNumber { get; set; }
    }

    public class MilkProductionViewModel
    {
        public int Id { get; set; }
        [Required] public int      CattleId             { get; set; }
        [Required] public int      FarmId               { get; set; }
        public int?      RecordedByWorkerId              { get; set; }
        [Required] public DateTime Date                 { get; set; } = DateTime.Today;
        [Range(0,200)] public double MorningYieldLiters { get; set; }
        [Range(0,200)] public double EveningYieldLiters { get; set; }
        [StringLength(500)] public string? Notes        { get; set; }
    }

    public class ProductViewModel
    {
        public int Id { get; set; }
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        public ProductCategory Category { get; set; } = ProductCategory.Other;
        [StringLength(1000)] public string? Description { get; set; }
        [Required] public decimal Price         { get; set; }
        [Range(0, 1_000_000)] public double StockQuantity { get; set; }
        [StringLength(50)] public string Unit  { get; set; } = "kg";
        public double MinStockLevel             { get; set; } = 0;
        public bool IsAvailable                 { get; set; } = true;
        public bool IsFeatured                  { get; set; } = false;
        public int  FarmId                      { get; set; }
        public IFormFile? ImageFile             { get; set; }
        public string?   ExistingImagePath      { get; set; }
    }

    public class OrderViewModel
    {
        public int Id { get; set; }
        [Required] public int FarmId { get; set; }
        [StringLength(500)] public string? DeliveryAddress { get; set; }
        [StringLength(1000)] public string? Notes          { get; set; }
        public List<OrderItemViewModel> Items              { get; set; } = new();

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }

    public class OrderItemViewModel
    {
        public int    ProductId { get; set; }
        public double Quantity  { get; set; }
    }

    public class ExpenseViewModel
    {
        public int Id { get; set; }
        public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;
        [Required] public decimal  Amount { get; set; }
        [Required] public DateTime Date   { get; set; } = DateTime.Today;
        [StringLength(1000)] public string? Description { get; set; }
        public int FarmId { get; set; }
        public IFormFile? ReceiptFile { get; set; }
    }

    public class RevenueViewModel
    {
        public int Id { get; set; }
        public RevenueSource Source { get; set; } = RevenueSource.Other;
        [Required] public decimal  Amount { get; set; }
        [Required] public DateTime Date   { get; set; } = DateTime.Today;
        [StringLength(1000)] public string? Description { get; set; }
        public int  FarmId  { get; set; }
        public int? OrderId { get; set; }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        [Required] public int      CattleId    { get; set; }
        [Required] public int      DoctorId    { get; set; }
        [Required] public int      FarmId      { get; set; }
        [Required] public DateTime ScheduledAt { get; set; }
        [Required, StringLength(500)] public string Reason { get; set; } = string.Empty;
        [StringLength(2000)] public string? Notes { get; set; }
    }

    public class BreedingViewModel
    {
        public int Id { get; set; }
        [Required] public int CattleId { get; set; }
        public int  SireId  { get; set; }
        [Required] public int FarmId  { get; set; }
        [Required] public DateTime BreedingDate { get; set; } = DateTime.Today;
        public DateTime? ExpectedCalvingDate { get; set; }
        public DateTime? ActualCalvingDate   { get; set; }
        public BreedingMethod  Method  { get; set; } = BreedingMethod.Natural;
        public BreedingOutcome Outcome { get; set; } = BreedingOutcome.Pending;
        public int?     CalvesCount   { get; set; }
        [StringLength(200)] public string? SireBreed { get; set; }
        [StringLength(200)] public string? InseminationTechnician { get; set; }
        [Range(0, double.MaxValue)] public decimal? Cost { get; set; }
        [StringLength(1000)] public string? Notes { get; set; }
    }

    public class FeedViewModel
    {
        public int Id { get; set; }
        [Required] public int FarmId  { get; set; }
        public int  CattleId { get; set; }
        [Required] public FeedType FeedType { get; set; } = FeedType.Hay;
        [Required, StringLength(200)] public string FeedName { get; set; } = string.Empty;
        [Required, Range(0.01, 100000)] public double QuantityKg { get; set; }
        [Range(0, double.MaxValue)] public decimal CostPerKg { get; set; }
        [Required] public DateTime Date { get; set; } = DateTime.Today;
        [StringLength(200)] public string? Supplier { get; set; }
        [StringLength(1000)] public string? Notes   { get; set; }
    }

    // ── Shared DTOs ──────────────────────────────────────────────────────────────
    public class MonthlyTrendItem
    {
        public string  Month   { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Expense { get; set; }
    }

    public class DailyMilkTrend
    {
        public string Day     { get; set; } = string.Empty;
        public double Liters  { get; set; }
    }

    public class TopProducerItem
    {
        public string      TagId        { get; set; } = string.Empty;
        public string      Name         { get; set; } = string.Empty;
        public string      Breed        { get; set; } = string.Empty;
        public double      DailyAvgLiters { get; set; }
        public HealthStatus HealthStatus { get; set; }
    }

    // ── Dashboard ViewModels ────────────────────────────────────────────────────
    public class AdminDashboardViewModel
    {
        public int TotalUsers     { get; set; }
        public int TotalFarms     { get; set; }
        public int TotalCattle    { get; set; }
        public int TotalWorkers   { get; set; }
        public int TotalDoctors   { get; set; }
        public int PendingFarms   { get; set; }
        public int ActiveOrders   { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ActivityLog>  RecentActivity  { get; set; } = new();
        public List<AuditLog>     RecentAuditLogs { get; set; } = new();
        public List<MonthlyTrendItem> MonthlyTrend { get; set; } = new();
    }

    public class OwnerDashboardViewModel
    {
        // Core metrics
        public int    TotalFarms      { get; set; }
        public int    TotalCattle     { get; set; }
        public int    ActiveCattle    { get; set; }
        public int    SickCattle      { get; set; }
        public int    TotalWorkers    { get; set; }
        public double MilkTodayLiters { get; set; }
        public decimal NetProfit      { get; set; }
        public decimal TotalRevenue   { get; set; }
        public decimal TotalExpenses  { get; set; }
        public int    PendingOrders   { get; set; }
        public int    UpcomingAppointments { get; set; }

        // Alert lists
        public List<Cattle>      HighRiskCattle       { get; set; } = new();
        public List<Vaccination> UpcomingVaccinations { get; set; } = new();
        public int               VaccinationDueCount  { get; set; }
        public int               HighRiskCount        { get; set; }

        // Cattle status breakdown (donut chart)
        public int ActiveCount   { get; set; }
        public int SickCount     { get; set; }
        public int SoldCount     { get; set; }
        public int DeceasedCount { get; set; }

        // Analytics
        public List<DailyMilkTrend>   MilkWeeklyTrend { get; set; } = new();
        public List<MonthlyTrendItem> MonthlyTrend    { get; set; } = new();
        public List<TopProducerItem>  TopProducers    { get; set; } = new();

        // Profitability
        public decimal CostPerLiter { get; set; }
        public decimal Roi          { get; set; }

        // Navigation
        public List<Farm> Farms       { get; set; } = new();
        public Farm?      SelectedFarm { get; set; }
    }

    public class WorkerDashboardViewModel
    {
        public Worker?       WorkerProfile     { get; set; }
        public int           TotalAttendanceDays { get; set; }
        public int           PresentThisMonth  { get; set; }
        public List<MilkProduction> RecentMilkLogs { get; set; } = new();
    }

    public class DoctorDashboardViewModel
    {
        public Doctor?              DoctorProfile       { get; set; }
        public int                  TotalAppointments   { get; set; }
        public int                  TodayAppointments   { get; set; }
        public List<Appointment>    UpcomingAppointments { get; set; } = new();
        public List<HealthRecord>   RecentHealthRecords  { get; set; } = new();
    }

    public class CustomerDashboardViewModel
    {
        public List<Order>   RecentOrders   { get; set; } = new();
        public int           TotalOrders    { get; set; }
        public decimal       TotalSpent     { get; set; }
        public List<Product> FeaturedProducts { get; set; } = new();
    }
}
