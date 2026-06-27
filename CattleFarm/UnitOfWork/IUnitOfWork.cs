using CattleFarm.Models;
using CattleFarm.Repositories.Interfaces;

namespace CattleFarm.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        // ── Repositories ───────────────────────────────────────────────────────
        ICattleRepository           Cattles           { get; }
        IUserRepository             Users             { get; }
        IFarmRepository             Farms             { get; }
        IWorkerRepository           Workers           { get; }
        IDoctorRepository           Doctors           { get; }
        IDoctorInvitationRepository DoctorInvitations { get; }
        IHealthRecordRepository         HealthRecords         { get; }
        ICattleMedicalRecordRepository  CattleMedicalRecords  { get; }
        IVaccinationRepository      Vaccinations      { get; }
        IMilkProductionRepository   MilkProductions   { get; }
        IProductRepository          Products          { get; }
        IOrderRepository            Orders            { get; }
        IExpenseRepository          Expenses          { get; }
        IRevenueRepository          Revenues          { get; }
        IPaymentRepository          Payments          { get; }
        ISubscriptionRepository     Subscriptions     { get; }
        INotificationRepository     Notifications     { get; }
        IAuditLogRepository         AuditLogs         { get; }
        IActivityLogRepository      ActivityLogs      { get; }
        IAppointmentRepository      Appointments      { get; }
        IBreedingRepository         Breedings         { get; }
        IFeedRepository             FeedRecords       { get; }

        // ── Transport Module ────────────────────────────────────────────
        IVehicleRepository          Vehicles          { get; }
        IDriverRepository           Drivers           { get; }
        ITransportRequestRepository TransportRequests { get; }
        ITripRepository             Trips             { get; }

        // ── Gap-Fill Modules ───────────────────────────────────────────
        IRepository<WeightRecord>           WeightRecords          { get; }
        IRepository<HeatRecord>             HeatRecords            { get; }
        IRepository<BullPerformanceRecord>  BullPerformanceRecords { get; }

        /// <summary>Commits all pending changes atomically. Returns affected row count.</summary>
        Task<int> SaveChangesAsync();

        /// <summary>Begins a new database transaction.</summary>
        Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync();
    }
}
