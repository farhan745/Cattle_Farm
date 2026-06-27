namespace CattleFarm.Models
{
    // ── Existing Farm Enums ───────────────────────────────────────────────────
    public enum CattleStatus      { Active, Sold, Deceased, Transferred }
    public enum CattleCategory    { DairyCow, BeefCattle, Calf, Bull, Heifer, Other }
    public enum HeatStatus        { NotInHeat, InHeat, Confirmed, Uncertain }
    public enum SemenQuality      { Excellent, Good, Fair, Poor, Rejected }
    public enum HealthStatus      { Healthy, AtRisk, Sick, Critical }
    public enum RiskLevel         { Low, Medium, High }
    public enum Gender            { Male, Female }
    public enum FarmType          { Dairy, Beef, Mixed, Breeding }
    public enum OrderStatus       { Pending, Confirmed, Processing, Shipped, Delivered, Cancelled }
    public enum PaymentStatus     { Pending, Completed, Failed, Refunded }
    public enum PaymentMethod     { Cash, BankTransfer, Bkash, Nagad, Visa, MasterCard, IslamiBank, CityBank, BracBank }
    public enum SubscriptionPlan  { Free, Member, Owner, Enterprise }
    public enum NotificationType
    {
        Vaccination,
        HealthAlert,
        OrderUpdate,
        Payment,
        Subscription,
        System,
        Appointment,
        WorkerAlert,
        MilkDrop,
        WeightLoss,
        TripStarted,
        TripCompleted,
        TransportBooked,
        FarmJoinRequest,
        JoinAccepted,
        JoinRejected,
        NewTask,
        TaskAccepted,
        TaskCompleted,
        BonusAdded,
        SalaryUpdate,
        LeaveRequest,
        Warning,
        EmergencyTask,
        // ── Doctor Invitation Module ──────────────────────────────────────
        DoctorInvitationSent,
        DoctorRegistered,
        DoctorPendingApproval,
        DoctorApproved,
        AppointmentRequested,
        AppointmentAccepted,
        AppointmentRejected,
        AppointmentCompleted,
        // ── Feed Inventory Module ─────────────────────────────────────────────
        LowFeedStock
    }
    /// <summary>Owner books → Pending; assigned vet approves → Accepted; vet completes with evidence → Completed.</summary>
    public enum AppointmentStatus { Pending, Accepted, Completed, Cancelled, Rejected, NoShow }
    public enum ProductCategory   { Milk, Beef, Manure, BreedingService, Other }
    public enum ApprovalStatus    { Pending, Approved, Rejected }
    public enum AttendanceStatus  { Present, Absent, HalfDay, Leave }
    public enum ReviewTargetType  { Farm, Cattle, Worker, Doctor, Product }
    public enum PaymentPurpose    { Subscription, CattlePurchase, Order, FarmPurchase, Transport, Other }
    public enum ExpenseCategory   { Feed, Medicine, Labor, Equipment, Utilities, Maintenance, Veterinary, Transport, Other }
    public enum RevenueSource     { MilkSales, BeefSales, BreedingService, ManureSales, FarmSale, Other }

    // ── Cattle Expense Category (per-cattle sell cost tracking) ──────────────
    public enum CattleExpenseCategory { Feed, Medicine, Veterinary, Transport, Labor, Other }

    // ── Transport Module Enums ────────────────────────────────────────────────
    public enum VehicleStatus    { Available, Assigned, OnTrip, Maintenance, Retired }
    public enum VehicleType      { Truck, Pickup, Van, Motorcycle, Trailer, Tractor, MiniBus, Other }
    public enum FuelType         { Diesel, Petrol, CNG, Electric, Hybrid }
    public enum DriverStatus     { Available, OnTrip, OffDuty, Suspended }
    public enum TransportType    { CattleTransport, FeedDelivery, ProductDelivery, EquipmentDelivery, General }
    public enum TripStatus       { Pending, Approved, Assigned, Ongoing, Completed, Cancelled }
    public enum TransportCostBasis { PerKm, PerTrip, Hourly }

    // ── Doctor Invitation Module Enums ────────────────────────────────────────
    /// <summary>Lifecycle status of a doctor invitation token.</summary>
    public enum InvitationStatus  { Pending, Accepted, Expired, Revoked }

    /// <summary>Doctor's self-reported gender (separate from cattle Gender enum).</summary>
    public enum DoctorGender      { Male, Female, Other, PreferNotToSay }
}
