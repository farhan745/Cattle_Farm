using BCrypt.Net;
using CattleFarm.Models;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(CattleFarmDbContext db)
        {
            await db.Database.EnsureCreatedAsync();

            // ── Users ─────────────────────────────────────────────────────────
            if (!await db.Users.AnyAsync())
            {
                var admin = new User
                {
                    Username = "admin", FullName = "System Administrator", Email = "admin@cattlefarm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), Role = AppRoles.Admin,
                    IsEmailVerified = true, IsActive = true, SubscriptionType = "Enterprise",
                    SubscriptionExpiry = DateTime.UtcNow.AddYears(10)
                };
                var owner = new User
                {
                    Username = "owner1", FullName = "Rahman Hossain", Email = "owner@cattlefarm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Owner@123"), Role = AppRoles.Owner,
                    IsEmailVerified = true, IsActive = true, PhoneNumber = "+8801711000001",
                    SubscriptionType = "Owner", SubscriptionExpiry = DateTime.UtcNow.AddYears(1)
                };
                var manager = new User
                {
                    Username = "manager1", FullName = "Karim Ahmed", Email = "manager@cattlefarm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"), Role = AppRoles.Manager,
                    IsEmailVerified = true, IsActive = true, PhoneNumber = "+8801722000002",
                    SubscriptionType = "Member", SubscriptionExpiry = DateTime.UtcNow.AddMonths(6)
                };
                var customer = new User
                {
                    Username = "customer1", FullName = "Rahim Uddin", Email = "customer@cattlefarm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"), Role = AppRoles.Customer,
                    IsEmailVerified = true, IsActive = true
                };
                await db.Users.AddRangeAsync(admin, owner, manager, customer);
                await db.SaveChangesAsync();

                // ── Subscriptions ─────────────────────────────────────────────
                await db.Subscriptions.AddRangeAsync(
                    new Subscription { UserId = owner.Id, Plan = SubscriptionPlan.Owner, PricePaid = 1500, StartDate = DateTime.UtcNow.AddMonths(-1), ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true, TransactionRef = "SEED-001" },
                    new Subscription { UserId = manager.Id, Plan = SubscriptionPlan.Member, PricePaid = 500, StartDate = DateTime.UtcNow.AddMonths(-2), ExpiryDate = DateTime.UtcNow.AddMonths(6), IsActive = true, TransactionRef = "SEED-002" }
                );

                // ── Farms ─────────────────────────────────────────────────────
                var farm1 = new Farm
                {
                    OwnerId = owner.Id, Name = "Green Pasture Farm", Location = "Dhaka, Bangladesh",
                    FarmType = FarmType.Dairy, SizeInAcres = 50, Capacity = 200,
                    Description = "A premier dairy farm with modern facilities and automated milk collection.",
                    Latitude = 23.8103, Longitude = 90.4125,
                    ApprovalStatus = ApprovalStatus.Approved, IsActive = true
                };
                var farm2 = new Farm
                {
                    OwnerId = owner.Id, Name = "Sunrise Beef Ranch", Location = "Gazipur, Bangladesh",
                    FarmType = FarmType.Beef, SizeInAcres = 120, Capacity = 500,
                    Description = "Large-scale beef production ranch with grass-fed premium cattle.",
                    Latitude = 23.9999, Longitude = 90.4203,
                    ApprovalStatus = ApprovalStatus.Approved, IsActive = true
                };
                await db.Farms.AddRangeAsync(farm1, farm2);
                await db.SaveChangesAsync();

                // ── Workers ───────────────────────────────────────────────────
                var worker1 = new Worker { FarmId = farm1.Id, FullName = "Salim Mia", Role = "Farm Hand", Phone = "+8801811000001", Skills = "Milking, Feeding, Cleaning", ExperienceYears = 5, Salary = 12000, IsAvailable = true, IsActive = true };
                var worker2 = new Worker { FarmId = farm1.Id, FullName = "Raju Bhai", Role = "Herd Manager", Phone = "+8801811000002", Skills = "Herd Management, Record Keeping", ExperienceYears = 8, Salary = 18000, IsAvailable = true, IsActive = true };
                var worker3 = new Worker { FarmId = farm2.Id, FullName = "Jamal Uddin", Role = "Cattle Handler", Phone = "+8801811000003", Skills = "Cattle Handling, Branding, Vaccination", ExperienceYears = 3, Salary = 10000, IsAvailable = true, IsActive = true };
                await db.Workers.AddRangeAsync(worker1, worker2, worker3);

                // ── Doctors ───────────────────────────────────────────────────
                var doc1 = new Doctor { FarmId = farm1.Id, FullName = "Dr. Nasreen Akhter", Specialization = "Bovine Medicine", Phone = "+8801911000001", Email = "dr.nasreen@vet.com", LicenseNumber = "VET-BD-0012", ConsultationFee = 1500, IsAvailable = true, IsActive = true };
                var doc2 = new Doctor { FullName = "Dr. Tariq Hasan", Specialization = "Veterinary Surgery", Phone = "+8801911000002", Email = "dr.tariq@vet.com", LicenseNumber = "VET-BD-0034", ConsultationFee = 2500, IsAvailable = true, IsActive = true };
                await db.Doctors.AddRangeAsync(doc1, doc2);
                await db.SaveChangesAsync();

                // ── Cattle ────────────────────────────────────────────────────
                var c1 = new Cattle { FarmId = farm1.Id, TagId = "GF-001", Name = "Lali", Breed = "Holstein Friesian", DateOfBirth = DateTime.UtcNow.AddYears(-3), Weight = 450, Gender = Gender.Female, HealthStatus = HealthStatus.Healthy, Status = CattleStatus.Active, PurchasePrice = 85000, PurchaseDate = DateTime.UtcNow.AddYears(-2), IsListedForSale = false, ApprovalStatus = ApprovalStatus.Approved };
                var c2 = new Cattle { FarmId = farm1.Id, TagId = "GF-002", Name = "Shyamla", Breed = "Jersey", DateOfBirth = DateTime.UtcNow.AddYears(-4), Weight = 380, Gender = Gender.Female, HealthStatus = HealthStatus.Healthy, Status = CattleStatus.Active, PurchasePrice = 70000, PurchaseDate = DateTime.UtcNow.AddYears(-3), IsListedForSale = false, ApprovalStatus = ApprovalStatus.Approved };
                var c3 = new Cattle { FarmId = farm1.Id, TagId = "GF-003", Name = "Kalo Mota", Breed = "Sahiwal", DateOfBirth = DateTime.UtcNow.AddYears(-2), Weight = 520, Gender = Gender.Male, HealthStatus = HealthStatus.Healthy, Status = CattleStatus.Active, PurchasePrice = 120000, PurchaseDate = DateTime.UtcNow.AddMonths(-10), IsListedForSale = true, SalePrice = 150000, IsPremiumListing = true, ApprovalStatus = ApprovalStatus.Approved };
                var c4 = new Cattle { FarmId = farm2.Id, TagId = "SR-001", Name = "Raja", Breed = "Brahman", DateOfBirth = DateTime.UtcNow.AddYears(-5), Weight = 680, Gender = Gender.Male, HealthStatus = HealthStatus.Healthy, Status = CattleStatus.Active, PurchasePrice = 200000, PurchaseDate = DateTime.UtcNow.AddYears(-4), IsListedForSale = true, SalePrice = 280000, IsPremiumListing = true, ApprovalStatus = ApprovalStatus.Approved };
                var c5 = new Cattle { FarmId = farm2.Id, TagId = "SR-002", Name = "Goru", Breed = "Red Sindhi", DateOfBirth = DateTime.UtcNow.AddYears(-3), Weight = 420, Gender = Gender.Female, HealthStatus = HealthStatus.AtRisk, Status = CattleStatus.Active, PurchasePrice = 60000, PurchaseDate = DateTime.UtcNow.AddYears(-2), IsListedForSale = false, ApprovalStatus = ApprovalStatus.Approved };
                await db.Cattles.AddRangeAsync(c1, c2, c3, c4, c5);
                await db.SaveChangesAsync();

                // ── Health Records ────────────────────────────────────────────
                await db.HealthRecords.AddRangeAsync(
                    new HealthRecord { CattleId = c1.Id, DoctorId = doc1.Id, RecordDate = DateTime.UtcNow.AddDays(-10), Temperature = 38.5, Weight = 450, HealthStatus = HealthStatus.Healthy, RiskLevel = RiskLevel.Low, Notes = "Routine check — all normal." },
                    new HealthRecord { CattleId = c5.Id, DoctorId = doc1.Id, RecordDate = DateTime.UtcNow.AddDays(-3), Temperature = 39.8, Weight = 415, HealthStatus = HealthStatus.AtRisk, RiskLevel = RiskLevel.High, Symptoms = "Reduced appetite, slight fever", VetRecommendation = "Administer antibiotics, recheck in 5 days." }
                );

                // ── Vaccinations ──────────────────────────────────────────────
                await db.Vaccinations.AddRangeAsync(
                    new Vaccination { CattleId = c1.Id, DoctorId = doc1.Id, VaccineName = "FMD Vaccine", VaccinationDate = DateTime.UtcNow.AddMonths(-6), NextDueDate = DateTime.UtcNow.AddMonths(6), AdministeredBy = "Dr. Nasreen", DoseNumber = 1 },
                    new Vaccination { CattleId = c2.Id, DoctorId = doc1.Id, VaccineName = "Brucellosis Vaccine", VaccinationDate = DateTime.UtcNow.AddMonths(-3), NextDueDate = DateTime.UtcNow.AddDays(7), AdministeredBy = "Dr. Nasreen", DoseNumber = 1 },
                    new Vaccination { CattleId = c5.Id, VaccineName = "Black Quarter Vaccine", VaccinationDate = DateTime.UtcNow.AddMonths(-8), NextDueDate = DateTime.UtcNow.AddDays(-5), AdministeredBy = "Dr. Tariq", DoseNumber = 1 }
                );

                // ── Milk Production (last 7 days) ─────────────────────────────
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.Date.AddDays(-i);
                    await db.MilkProductions.AddRangeAsync(
                        new MilkProduction { CattleId = c1.Id, FarmId = farm1.Id, Date = date, MorningYieldLiters = 8.5, EveningYieldLiters = 7.2 },
                        new MilkProduction { CattleId = c2.Id, FarmId = farm1.Id, Date = date, MorningYieldLiters = 6.0, EveningYieldLiters = 5.5 }
                    );
                }

                // ── Products ──────────────────────────────────────────────────
                await db.Products.AddRangeAsync(
                    new Product { FarmId = farm1.Id, Name = "Fresh Whole Milk", Category = ProductCategory.Milk, Description = "Farm-fresh whole milk, collected daily.", Price = 65, StockQuantity = 500, Unit = "Liter", MinStockLevel = 50, IsAvailable = true, IsFeatured = true },
                    new Product { FarmId = farm1.Id, Name = "Organic Butter", Category = ProductCategory.Other, Description = "Premium organic butter made from farm-fresh cream.", Price = 350, StockQuantity = 80, Unit = "250g Pack", IsAvailable = true, IsFeatured = true },
                    new Product { FarmId = farm2.Id, Name = "Premium Beef (Dressed)", Category = ProductCategory.Beef, Description = "USDA-grade premium dressed beef from grass-fed cattle.", Price = 700, StockQuantity = 200, Unit = "kg", MinStockLevel = 20, IsAvailable = true, IsFeatured = true },
                    new Product { FarmId = farm1.Id, Name = "Organic Manure", Category = ProductCategory.Manure, Description = "Natural organic manure, ideal for agriculture.", Price = 20, StockQuantity = 2000, Unit = "kg", IsAvailable = true },
                    new Product { FarmId = farm2.Id, Name = "Breeding Service (Bull)", Category = ProductCategory.BreedingService, Description = "Premium Brahman bull breeding service.", Price = 5000, StockQuantity = 10, Unit = "Service", IsAvailable = true }
                );

                // ── Expenses ──────────────────────────────────────────────────
                await db.Expenses.AddRangeAsync(
                    new Expense { FarmId = farm1.Id, Category = ExpenseCategory.Feed, Amount = 45000, Date = DateTime.UtcNow.AddMonths(-1), Description = "Monthly feed and fodder", CreatedByUserId = owner.Id },
                    new Expense { FarmId = farm1.Id, Category = ExpenseCategory.Veterinary, Amount = 8500, Date = DateTime.UtcNow.AddMonths(-1), Description = "Vet consultations and medicines", CreatedByUserId = owner.Id },
                    new Expense { FarmId = farm1.Id, Category = ExpenseCategory.Labor, Amount = 30000, Date = DateTime.UtcNow.AddMonths(-1), Description = "Worker salaries", CreatedByUserId = owner.Id },
                    new Expense { FarmId = farm2.Id, Category = ExpenseCategory.Feed, Amount = 120000, Date = DateTime.UtcNow.AddMonths(-1), Description = "Monthly feed for beef cattle", CreatedByUserId = owner.Id }
                );

                // ── Revenue ───────────────────────────────────────────────────
                await db.Revenues.AddRangeAsync(
                    new Revenue { FarmId = farm1.Id, Source = RevenueSource.MilkSales, Amount = 95000, Date = DateTime.UtcNow.AddMonths(-1), Description = "Monthly milk sales", CreatedByUserId = owner.Id },
                    new Revenue { FarmId = farm1.Id, Source = RevenueSource.Other, Amount = 28000, Date = DateTime.UtcNow.AddMonths(-1), Description = "Butter and by-products sales", CreatedByUserId = owner.Id },
                    new Revenue { FarmId = farm2.Id, Source = RevenueSource.BeefSales, Amount = 350000, Date = DateTime.UtcNow.AddMonths(-1), Description = "Beef sales this month", CreatedByUserId = owner.Id }
                );

                // ── Appointments ──────────────────────────────────────────────
                await db.Appointments.AddRangeAsync(
                    new Appointment { CattleId = c5.Id, DoctorId = doc1.Id, FarmId = farm2.Id, ScheduledAt = DateTime.UtcNow.AddDays(2), Reason = "Follow-up for fever and appetite loss", Status = AppointmentStatus.Scheduled, CreatedByUserId = owner.Id },
                    new Appointment { CattleId = c1.Id, DoctorId = doc1.Id, FarmId = farm1.Id, ScheduledAt = DateTime.UtcNow.AddDays(7), Reason = "Annual check-up", Status = AppointmentStatus.Scheduled, CreatedByUserId = owner.Id }
                );

                // ── Notifications ─────────────────────────────────────────────
                await db.Notifications.AddRangeAsync(
                    new Notification { UserId = owner.Id, Title = "Welcome to Smart Cattle Farm!", Message = "Your farm management dashboard is ready. Add your first cattle to get started.", Type = NotificationType.System },
                    new Notification { UserId = owner.Id, Title = "Vaccination Due Soon", Message = "Shyamla (GF-002) Brucellosis vaccine is due in 7 days.", Type = NotificationType.Vaccination, RelatedEntityType = "Cattle", RelatedEntityId = c2.Id },
                    new Notification { UserId = owner.Id, Title = "High Risk Alert!", Message = "Goru (SR-002) has been flagged as HIGH RISK. Please review immediately.", Type = NotificationType.HealthAlert, RelatedEntityType = "Cattle", RelatedEntityId = c5.Id },
                    new Notification { UserId = admin.Id, Title = "New Farm Registration", Message = "A new farm registration request is pending approval.", Type = NotificationType.System }
                );

                // ── Audit Logs ────────────────────────────────────────────────
                await db.AuditLogs.AddAsync(new AuditLog { UserId = admin.Id, Action = "CREATE", EntityName = "User", EntityId = admin.Id, NewValues = "{\"Username\":\"admin\"}", Timestamp = DateTime.UtcNow });
                await db.ActivityLogs.AddAsync(new ActivityLog { UserId = admin.Id, Description = "System initialized with seed data", Timestamp = DateTime.UtcNow });

                await db.SaveChangesAsync();
            }
        }
    }
}
