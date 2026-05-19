namespace CattleFarm.Models
{
    public enum CattleStatus      { Active, Sold, Deceased }
    public enum HealthStatus      { Healthy, AtRisk, Sick, Critical }
    public enum RiskLevel         { Low, Medium, High }
    public enum Gender            { Male, Female }
    public enum FarmType          { Dairy, Beef, Mixed, Breeding }
    public enum OrderStatus       { Pending, Confirmed, Processing, Shipped, Delivered, Cancelled }
    public enum PaymentStatus     { Pending, Completed, Failed, Refunded }
    public enum PaymentMethod     { Cash, BankTransfer, Bkash, Nagad, Visa, MasterCard, IslamiBank, CityBank, BracBank }
    public enum SubscriptionPlan  { Free, Member, Owner, Enterprise }
    public enum NotificationType  { Vaccination, HealthAlert, OrderUpdate, Payment, Subscription, System, Appointment, WorkerAlert, MilkDrop, WeightLoss }
    public enum AppointmentStatus { Scheduled, Completed, Cancelled, NoShow }
    public enum ProductCategory   { Milk, Beef, Manure, BreedingService, Other }
    public enum ApprovalStatus    { Pending, Approved, Rejected }
    public enum AttendanceStatus  { Present, Absent, HalfDay, Leave }
    public enum ReviewTargetType  { Farm, Cattle, Worker, Doctor, Product }
    public enum PaymentPurpose    { Subscription, CattlePurchase, Order, FarmPurchase, Other }
    public enum ExpenseCategory   { Feed, Medicine, Labor, Equipment, Utilities, Maintenance, Veterinary, Other }
    public enum RevenueSource     { MilkSales, BeefSales, BreedingService, ManureSales, FarmSale, Other }
}
