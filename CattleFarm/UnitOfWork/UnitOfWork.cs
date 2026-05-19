using CattleFarm.Models;
using CattleFarm.Repositories.Implementations;
using CattleFarm.Repositories.Interfaces;

namespace CattleFarm.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CattleFarmDbContext _context;

        public UnitOfWork(CattleFarmDbContext context)
        {
            _context = context;
            Cattles         = new CattleRepository(_context);
            Users           = new UserRepository(_context);
            Farms           = new FarmRepository(_context);
            Workers         = new WorkerRepository(_context);
            Doctors         = new DoctorRepository(_context);
            HealthRecords   = new HealthRecordRepository(_context);
            Vaccinations    = new VaccinationRepository(_context);
            MilkProductions = new MilkProductionRepository(_context);
            Products        = new ProductRepository(_context);
            Orders          = new OrderRepository(_context);
            Expenses        = new ExpenseRepository(_context);
            Revenues        = new RevenueRepository(_context);
            Payments        = new PaymentRepository(_context);
            Subscriptions   = new SubscriptionRepository(_context);
            Notifications   = new NotificationRepository(_context);
            AuditLogs       = new AuditLogRepository(_context);
            ActivityLogs    = new ActivityLogRepository(_context);
            Appointments    = new AppointmentRepository(_context);
            Breedings       = new BreedingRepository(_context);
            FeedRecords     = new FeedRepository(_context);
        }

        public ICattleRepository          Cattles         { get; }
        public IUserRepository            Users           { get; }
        public IFarmRepository            Farms           { get; }
        public IWorkerRepository          Workers         { get; }
        public IDoctorRepository          Doctors         { get; }
        public IHealthRecordRepository    HealthRecords   { get; }
        public IVaccinationRepository     Vaccinations    { get; }
        public IMilkProductionRepository  MilkProductions { get; }
        public IProductRepository         Products        { get; }
        public IOrderRepository           Orders          { get; }
        public IExpenseRepository         Expenses        { get; }
        public IRevenueRepository         Revenues        { get; }
        public IPaymentRepository         Payments        { get; }
        public ISubscriptionRepository    Subscriptions   { get; }
        public INotificationRepository    Notifications   { get; }
        public IAuditLogRepository        AuditLogs       { get; }
        public IActivityLogRepository     ActivityLogs    { get; }
        public IAppointmentRepository     Appointments    { get; }
        public IBreedingRepository        Breedings       { get; }
        public IFeedRepository            FeedRecords     { get; }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
