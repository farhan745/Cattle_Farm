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
        public CattleCategory Category   { get; set; } = CattleCategory.DairyCow;
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
        public CattleStatus Status       { get; set; } = CattleStatus.Active;

        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        public decimal? SalePrice   { get; set; }
        public DateTime? SaleDate   { get; set; }
        public DateTime? PurchaseDate { get; set; }

        [StringLength(2000)]
        public string? Description  { get; set; }

        [StringLength(200)]
        public string? Origin { get; set; }

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

        [StringLength(1000)]
        public string? Notes { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string?   ExistingImagePath { get; set; }
    }

    public class DoctorSelfRegisterVM
    {
        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Profile photo is required")]
        [DataType(DataType.Upload)]
        public IFormFile ProfilePhoto { get; set; } = null!;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Specialization { get; set; } = string.Empty;

        [Required, Range(0.01, 100000)]
        public decimal ConsultationFee { get; set; }

        [Required, StringLength(500)]
        public string AvailableTimeSlot { get; set; } = string.Empty;

        [Required, Range(0, 60)]
        public int YearsOfExperience { get; set; }

        [StringLength(100)]
        public string? LicenseNumber { get; set; }
    }

    public class CattleMedicalRecordViewModel
    {
        public int Id { get; set; }

        [Required]
        public int CattleId { get; set; }

        [Required]
        public DateTime ExaminationDate { get; set; } = DateTime.Today;

        [Required, StringLength(2000)]
        public string ChiefComplaint { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Diagnosis { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Prescription { get; set; }

        [StringLength(200)]
        public string? MedicineName { get; set; }

        [StringLength(100)]
        public string? MedicineDose { get; set; }

        [StringLength(100)]
        public string? DoseFrequency { get; set; }

        [Range(0, 365)]
        public int DoseDurationDays { get; set; }

        public DateTime? NextVisitDate { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        public string? CattleName { get; set; }
        public string? CattleTagId { get; set; }
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

        [Range(0, 10)]
        public decimal? FatPercentage { get; set; }

        [Range(0, 10)]
        public decimal? ProteinLevel { get; set; }

        [Range(0, 20)]
        public decimal? SolidNotFat { get; set; }

        [StringLength(50)]
        public string? MilkQualityGrade { get; set; }
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

    public class CompleteAppointmentViewModel
    {
        public int Id { get; set; }
        [StringLength(2000)] public string? CompletionNotes { get; set; }
        [Required] public IFormFile? EvidenceFile { get; set; }
        [Required] public IFormFile? PrescriptionFile { get; set; }
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
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit      { get; set; }
        public int ActiveCount   { get; set; }
        public int SickCount     { get; set; }
        public int SoldCount     { get; set; }
        public int DeceasedCount { get; set; }
        public List<DailyMilkTrend>   MilkWeeklyTrend { get; set; } = new();
        public List<ActivityLog>  RecentActivity  { get; set; } = new();
        public List<AuditLog>     RecentAuditLogs { get; set; } = new();
        public List<MonthlyTrendItem> MonthlyTrend { get; set; } = new();
        public List<Farm> Farms { get; set; } = new();
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
        public int?          MyFarmId          { get; set; }
        public string?       MyFarmName        { get; set; }
        public int           TotalAttendanceDays { get; set; }
        public int           PresentThisMonth  { get; set; }
        public List<MilkProduction> RecentMilkLogs { get; set; } = new();
        public List<TaskViewModel>  MyTasks         { get; set; } = new();
        public int           PendingTasks      { get; set; }
        public int           InProgressTasks   { get; set; }
        public int           CompletedTasks    { get; set; }
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

    // ── Cattle Sell ViewModels ───────────────────────────────────────────────────
    public class CattleExpenseViewModel
    {
        public int Id { get; set; }

        [Required]
        public int CattleId { get; set; }

        [Required]
        public CattleExpenseCategory Category { get; set; } = CattleExpenseCategory.Other;

        [Required, Range(1, 10_000_000, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Description { get; set; }
    }

    public class CattleSellViewModel
    {
        // Cattle info (display only)
        public int     CattleId       { get; set; }
        public string  CattleName     { get; set; } = string.Empty;
        public string  TagId          { get; set; } = string.Empty;
        public string  Breed          { get; set; } = string.Empty;
        public string? ImagePath      { get; set; }
        public string? FarmName       { get; set; }
        public bool    IsListedForSale  { get; set; }
        public bool    IsPremiumListing { get; set; }

        // Financial breakdown
        public decimal  PurchasePrice    { get; set; }
        public decimal? SalePrice        { get; set; }   // current marketplace listing price
        public decimal  TotalCostAmount  { get; set; }   // sum of all CattleExpenses
        public decimal  DesiredProfit    { get; set; }
        public decimal  CalculatedSellPrice => PurchasePrice + TotalCostAmount + DesiredProfit;

        // Existing expenses for this cattle
        public List<CattleExpense> CattleExpenses { get; set; } = new();

        // Form for adding a new expense inline
        public CattleExpenseViewModel NewExpense { get; set; } = new();

        // Sale details
        [Required]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        [StringLength(200)]
        public string? BuyerName  { get; set; }

        [StringLength(20)]
        public string? BuyerPhone { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}

// ────────────────────────────────────────────────────────────────────────────
// Transport Module ViewModels
// ────────────────────────────────────────────────────────────────────────────
namespace CattleFarm.ViewModels
{
    using System.ComponentModel.DataAnnotations;
    using CattleFarm.Models;

    // ── Shared transport DTO ──────────────────────────────────────────────────
    public class MonthlyTransportStat
    {
        public string  Month     { get; set; } = string.Empty;
        public int     TripCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalKm   { get; set; }
    }

    // ── Vehicle ───────────────────────────────────────────────────────────────
    public class VehicleViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        public VehicleType   Type               { get; set; } = VehicleType.Truck;
        public FuelType      FuelType            { get; set; } = FuelType.Diesel;
        public VehicleStatus Status              { get; set; } = VehicleStatus.Available;

        [Required, StringLength(50)]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Capacity       { get; set; }
        public string? CapacityUnit   { get; set; } = "tonnes";

        [Range(0, 100000)]
        public decimal FuelCostPerKm  { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // ── Driver ────────────────────────────────────────────────────────────────
    public class DriverViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? LicenseType { get; set; }

        [Range(0, 60)]
        public int ExperienceYears { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public DriverStatus Status { get; set; } = DriverStatus.Available;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int? AssignedVehicleId { get; set; }
    }

    // ── Transport Request ─────────────────────────────────────────────────────
    public class TransportRequestViewModel
    {
        public int Id { get; set; }

        public TransportType RequestType { get; set; } = TransportType.General;

        [Required, StringLength(500)]
        public string PickupLocation { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(1);

        [Range(0.1, 100000)]
        public decimal? EstimatedDistanceKm { get; set; }

        [Range(0, 100000)]
        public decimal? CargoWeight { get; set; }

        [StringLength(500)]
        public string? CargoDescription { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public int?  FarmId  { get; set; }
        public int?  OrderId { get; set; }

        // Smart suggestion return values
        public int?  SuggestedVehicleId { get; set; }
        public int?  SuggestedDriverId  { get; set; }
    }

    // ── Trip Assign ───────────────────────────────────────────────────────────
    public class TripAssignViewModel
    {
        [Required]
        public int TransportRequestId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int DriverId { get; set; }

        [Required, Range(0.1, 100000)]
        public decimal DistanceKm { get; set; }

        [StringLength(500)]
        public string? RouteNotes { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    // ── Trip Complete ─────────────────────────────────────────────────────────
    public class TripCompleteViewModel
    {
        public int     TripId          { get; set; }
        public decimal? ActualDistance  { get; set; }
        public decimal? AdditionalCost  { get; set; }
        public string?  AdditionalNote  { get; set; }
    }

    // ── Transport Dashboard ───────────────────────────────────────────────────
    public class TransportDashboardViewModel
    {
        public int     TotalVehicles        { get; set; }
        public int     AvailableVehicles    { get; set; }
        public int     OnTripVehicles       { get; set; }
        public int     MaintenanceVehicles  { get; set; }
        public int     TotalDrivers         { get; set; }
        public int     AvailableDrivers     { get; set; }
        public int     OnTripDrivers        { get; set; }
        public int     PendingRequests      { get; set; }
        public int     OngoingTrips         { get; set; }
        public int     CompletedTripsTotal  { get; set; }
        public decimal TotalCostThisMonth   { get; set; }
        public decimal TotalCostAllTime     { get; set; }

        public List<Trip>   RecentTrips    { get; set; } = new();
        public List<Trip>   OngoingTripList{ get; set; } = new();
        public List<MonthlyTransportStat> MonthlyStats { get; set; } = new();
        public Dictionary<string, int>   VehicleStatusBreakdown { get; set; } = new();
    }

    // ── Transport Report ──────────────────────────────────────────────────────
    public class TransportReportViewModel
    {
        public DateTime From              { get; set; }
        public DateTime To                { get; set; }
        public int      TotalTrips        { get; set; }
        public int      CompletedTrips    { get; set; }
        public int      CancelledTrips    { get; set; }
        public decimal  TotalDistanceKm   { get; set; }
        public decimal  TotalCost         { get; set; }
        public decimal  AverageTripCost   { get; set; }
        public double   AverageTripDistKm { get; set; }
        public List<VehicleUsageStat>     VehicleUsage      { get; set; } = new();
        public List<DriverPerformanceStat> DriverPerformance { get; set; } = new();
        public List<MonthlyTransportStat>  MonthlyStats      { get; set; } = new();
    }

    public class VehicleUsageStat
    {
        public string  VehicleName  { get; set; } = string.Empty;
        public string  Registration { get; set; } = string.Empty;
        public int     TripCount    { get; set; }
        public decimal TotalKm      { get; set; }
        public decimal TotalCost    { get; set; }
    }

    public class DriverPerformanceStat
    {
        public string  DriverName { get; set; } = string.Empty;
        public int     TripCount  { get; set; }
        public decimal TotalKm    { get; set; }
        public decimal Rating     { get; set; }
    }

    public class CattleTransferViewModel
    {
        [Required]
        public int CattleId { get; set; }

        public string? CattleName { get; set; }
        public string? TagId { get; set; }

        [Required(ErrorMessage = "Transfer location/destination is required")]
        [StringLength(200)]
        [Display(Name = "Transferred To")]
        public string TransferredTo { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Transfer Date")]
        public DateTime TransferDate { get; set; } = DateTime.Today;
    }

    public class TopProducerItemViewModel
    {
        public int CattleId { get; set; }
        public string TagId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public HealthStatus HealthStatus { get; set; }
        public double TotalLiters { get; set; }
        public double AverageLiters { get; set; }
        public int RecordCount { get; set; }
    }

    public class MilkDropAlertViewModel
    {
        public int CattleId { get; set; }
        public string TagId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public decimal BaselineAverage { get; set; }
        public decimal RecentAverage { get; set; }
        public decimal DropPercentage { get; set; }
        public DateTime LastRecordedDate { get; set; }
    }

    public class CostPerLiterViewModel
    {
        public DateTime Date { get; set; }
        public decimal TotalFeedCost { get; set; }
        public double TotalMilkLiters { get; set; }
        public decimal CostPerLiter => TotalMilkLiters > 0 ? TotalFeedCost / (decimal)TotalMilkLiters : 0;
    }

    public class MonthlyProfitLossViewModel
    {
        public string MonthName { get; set; } = string.Empty;
        public int MonthNumber { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expense { get; set; }
        public decimal NetProfit => Revenue - Expense;
    }

    public class RevenueBreakdownViewModel
    {
        public string Source { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class WorkerPerformanceViewModel
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public double TaskCompletionRate { get; set; }
        public int TotalAttendances { get; set; }
        public int PresentCount { get; set; }
        public double AttendanceRate { get; set; }
        public double OverallScore { get; set; }
    }
}
