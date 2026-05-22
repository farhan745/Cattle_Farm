using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Models
{
    public class CattleFarmDbContext : DbContext
    {
        public CattleFarmDbContext(DbContextOptions<CattleFarmDbContext> options) : base(options) { }

        // ── DbSets ────────────────────────────────────────────────────────────
        public DbSet<User>             Users             { get; set; }
        public DbSet<Farm>             Farms             { get; set; }
        public DbSet<Cattle>           Cattles           { get; set; }
        public DbSet<Worker>           Workers           { get; set; }
        public DbSet<Doctor>           Doctors           { get; set; }
        public DbSet<HealthRecord>     HealthRecords     { get; set; }
        public DbSet<Vaccination>      Vaccinations      { get; set; }
        public DbSet<MedicineRecord>   MedicineRecords   { get; set; }
        public DbSet<MilkProduction>   MilkProductions   { get; set; }
        public DbSet<Product>          Products          { get; set; }
        public DbSet<Order>            Orders            { get; set; }
        public DbSet<OrderItem>        OrderItems        { get; set; }
        public DbSet<Expense>          Expenses          { get; set; }
        public DbSet<Revenue>          Revenues          { get; set; }
        public DbSet<Payment>          Payments          { get; set; }
        public DbSet<Subscription>     Subscriptions     { get; set; }
        public DbSet<Notification>     Notifications     { get; set; }
        public DbSet<AuditLog>         AuditLogs         { get; set; }
        public DbSet<ActivityLog>      ActivityLogs      { get; set; }
        public DbSet<Review>           Reviews           { get; set; }
        public DbSet<WorkerAttendance> WorkerAttendances { get; set; }
        public DbSet<Appointment>      Appointments      { get; set; }
        public DbSet<Breeding>         Breedings         { get; set; }
        public DbSet<FeedRecord>       FeedRecords       { get; set; }
        public DbSet<Attendance>       Attendances       { get; set; }

        // ── Transport Module ──────────────────────────────────────────────────
        public DbSet<Vehicle>           Vehicles          { get; set; }
        public DbSet<Driver>            Drivers           { get; set; }
        public DbSet<TransportRequest>  TransportRequests { get; set; }
        public DbSet<Trip>              Trips             { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Unique indexes ────────────────────────────────────────────────
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.Username).IsUnique();
                e.Property(u => u.Role).HasDefaultValue("User");
            });

            modelBuilder.Entity<Cattle>(e =>
            {
                e.HasIndex(c => new { c.TagId, c.FarmId }).IsUnique();
                e.Property(c => c.Name).HasMaxLength(100).IsRequired();
                e.Property(c => c.Breed).HasMaxLength(100);
            });

            modelBuilder.Entity<Attendance>(e =>
            {
                e.HasIndex(a => new { a.WorkerId, a.Date }).IsUnique();

                e.HasOne(a => a.Worker)
                 .WithMany(w => w.DailyAttendances)
                 .HasForeignKey(a => a.WorkerId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(a => a.MarkedByUser)
                 .WithMany()
                 .HasForeignKey(a => a.MarkedByUserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Global soft-delete query filters ──────────────────────────────
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Farm>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Cattle>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Worker>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Doctor>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Revenue>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<HealthRecord>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Vehicle>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Driver>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<TransportRequest>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Trip>().HasQueryFilter(e => !e.IsDeleted);

            // ── Relationships — restrict cascades to avoid multiple cascade paths ──
            // Farm → Workers/Doctors/Products/etc. (restrict so parent delete requires manual cleanup)
            modelBuilder.Entity<Farm>()
                .HasMany(f => f.Workers)
                .WithOne(w => w.Farm)
                .HasForeignKey(w => w.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Farm>()
                .HasMany(f => f.Doctors)
                .WithOne(d => d.Farm)
                .HasForeignKey(d => d.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Farm>()
                .HasMany(f => f.Products)
                .WithOne(p => p.Farm)
                .HasForeignKey(p => p.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Farm>()
                .HasMany(f => f.Expenses)
                .WithOne(e => e.Farm)
                .HasForeignKey(e => e.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Farm>()
                .HasMany(f => f.Revenues)
                .WithOne(r => r.Farm)
                .HasForeignKey(r => r.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Farm>()
                .HasMany(f => f.Orders)
                .WithOne(o => o.Farm)
                .HasForeignKey(o => o.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Farm>()
                .HasMany(f => f.MilkProductions)
                .WithOne(m => m.Farm)
                .HasForeignKey(m => m.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Farm>()
                .HasMany(f => f.Appointments)
                .WithOne(a => a.Farm)
                .HasForeignKey(a => a.FarmId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cattle relations (restrict to prevent multi-cascade)
            modelBuilder.Entity<Cattle>()
                .HasMany(c => c.HealthRecords)
                .WithOne(h => h.Cattle)
                .HasForeignKey(h => h.CattleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cattle>()
                .HasMany(c => c.Vaccinations)
                .WithOne(v => v.Cattle)
                .HasForeignKey(v => v.CattleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cattle>()
                .HasMany(c => c.MedicineRecords)
                .WithOne(m => m.Cattle)
                .HasForeignKey(m => m.CattleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cattle>()
                .HasMany(c => c.MilkProductions)
                .WithOne(m => m.Cattle)
                .HasForeignKey(m => m.CattleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cattle>()
                .HasMany(c => c.Appointments)
                .WithOne(a => a.Cattle)
                .HasForeignKey(a => a.CattleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor relations
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Appointments)
                .WithOne(a => a.Doctor)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.HealthRecords)
                .WithOne(h => h.Doctor)
                .HasForeignKey(h => h.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.MedicineRecords)
                .WithOne(m => m.PrescribedByDoctor)
                .HasForeignKey(m => m.PrescribedByDoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Worker → MilkProduction (restrict)
            modelBuilder.Entity<Worker>()
                .HasMany(w => w.MilkProductions)
                .WithOne(m => m.RecordedByWorker)
                .HasForeignKey(m => m.RecordedByWorkerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Order → Payment
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // User-owned User relationships — restrict
            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Payments)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.Reviewer)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense/Revenue CreatedByUser — restrict
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Revenue>()
                .HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Revenue → Order (optional)
            modelBuilder.Entity<Revenue>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Appointment → CreatedByUser
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.CreatedByUser)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Worker optional User link
            modelBuilder.Entity<Worker>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Doctor optional User link
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Breeding relationships
            modelBuilder.Entity<Breeding>(e =>
            {
                e.HasOne(b => b.Farm)
                 .WithMany()
                 .HasForeignKey(b => b.FarmId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(b => b.Cattle)
                 .WithMany()
                 .HasForeignKey(b => b.CattleId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(b => b.Sire)
                 .WithMany()
                 .HasForeignKey(b => b.SireId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // FeedRecord relationships
            modelBuilder.Entity<FeedRecord>(e =>
            {
                e.HasOne(f => f.Farm)
                 .WithMany()
                 .HasForeignKey(f => f.FarmId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(f => f.Cattle)
                 .WithMany()
                 .HasForeignKey(f => f.CattleId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(f => f.RecordedByWorker)
                 .WithMany()
                 .HasForeignKey(f => f.RecordedByWorkerId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Vehicle relationships ─────────────────────────────────────────
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Driver)
                .WithOne(d => d.AssignedVehicle)
                .HasForeignKey<Driver>(d => d.AssignedVehicleId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Trip relationships ────────────────────────────────────────────
            modelBuilder.Entity<Trip>(e =>
            {
                e.HasOne(t => t.TransportRequest)
                 .WithOne(r => r.Trip)
                 .HasForeignKey<Trip>(t => t.TransportRequestId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.Vehicle)
                 .WithMany(v => v.Trips)
                 .HasForeignKey(t => t.VehicleId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.Driver)
                 .WithMany(d => d.Trips)
                 .HasForeignKey(t => t.DriverId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── TransportRequest relationships ────────────────────────────────
            modelBuilder.Entity<TransportRequest>(e =>
            {
                e.HasOne(r => r.Order)
                 .WithMany()
                 .HasForeignKey(r => r.OrderId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(r => r.Farm)
                 .WithMany()
                 .HasForeignKey(r => r.FarmId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(r => r.RequestedByUser)
                 .WithMany()
                 .HasForeignKey(r => r.RequestedByUserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}