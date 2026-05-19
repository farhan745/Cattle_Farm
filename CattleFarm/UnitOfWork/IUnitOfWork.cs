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
        IHealthRecordRepository     HealthRecords     { get; }
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

        /// <summary>Commits all pending changes atomically. Returns affected row count.</summary>
        Task<int> SaveChangesAsync();
    }
}
