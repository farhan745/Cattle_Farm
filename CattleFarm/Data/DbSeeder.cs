using BCrypt.Net;
using CattleFarm.Models;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(CattleFarmDbContext db, bool isDevelopment = true)
        {
            // ── Fast Migration Check ─────────────────────────────────────────
            // Only run MigrateAsync if there are actually pending migrations.
            // This avoids executing database schema inspection and history table checks on every startup.
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await db.Database.MigrateAsync();
            }

            // ── Fast Seed Check ──────────────────────────────────────────────
            // If the database has already been seeded (admin user exists), skip all other checks.
            // This reduces startup database queries from 8+ sequential queries to just 1.
            var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == "admin@cattlefarm.com" || u.Username == "admin");
            if (admin != null)
            {
                if (isDevelopment)
                {
                    await EnsureRichDemoDataAsync(db, admin);
                }

                return;
            }

            if (admin == null)
            {
                admin = new User
                {
                    Username = "admin", FullName = "System Administrator", Email = "admin@cattlefarm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), Role = AppRoles.Admin,
                    IsEmailVerified = true, IsActive = true, SubscriptionType = "Enterprise",
                    SubscriptionExpiry = DateTime.UtcNow.AddYears(10)
                };
                await db.Users.AddAsync(admin);
                await db.SaveChangesAsync();
            }

            if (!isDevelopment)
            {
                return;
            }

            // ── Development/Demo Users ───────────────────────────────────────
            var owner = await db.Users.FirstOrDefaultAsync(u => u.Email == "owner@farm.com" || u.Username == "owner");
            if (owner == null)
            {
                owner = new User
                {
                    Username = "owner", FullName = "Rahman Hossain", Email = "owner@farm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Owner@123"), Role = AppRoles.Owner,
                    IsEmailVerified = true, IsActive = true, PhoneNumber = "+8801711000001",
                    SubscriptionType = "Owner", SubscriptionExpiry = DateTime.UtcNow.AddYears(1)
                };
                await db.Users.AddAsync(owner);
            }

            var manager = await db.Users.FirstOrDefaultAsync(u => u.Email == "manager@cattlefarm.com" || u.Username == "manager1");
            if (manager == null)
            {
                manager = new User
                {
                    Username = "manager1", FullName = "Karim Ahmed", Email = "manager@cattlefarm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"), Role = AppRoles.Manager,
                    IsEmailVerified = true, IsActive = true, PhoneNumber = "+8801722000002",
                    SubscriptionType = "Member", SubscriptionExpiry = DateTime.UtcNow.AddMonths(6)
                };
                await db.Users.AddAsync(manager);
            }

            var customer = await db.Users.FirstOrDefaultAsync(u => u.Email == "customer@farm.com" || u.Username == "customer");
            if (customer == null)
            {
                customer = new User
                {
                    Username = "customer", FullName = "Rahim Uddin", Email = "customer@farm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"), Role = AppRoles.Customer,
                    IsEmailVerified = true, IsActive = true
                };
                await db.Users.AddAsync(customer);
            }

            var doctorUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "doctor@farm.com" || u.Username == "doctor");
            if (doctorUser == null)
            {
                doctorUser = new User
                {
                    Username = "doctor", FullName = "Dr. Nasreen Akhter", Email = "doctor@farm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123"), Role = AppRoles.Doctor,
                    IsEmailVerified = true, IsActive = true, PhoneNumber = "+8801911000001"
                };
                await db.Users.AddAsync(doctorUser);
            }

            var workerUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "worker@farm.com" || u.Username == "worker");
            if (workerUser == null)
            {
                workerUser = new User
                {
                    Username = "worker", FullName = "Salim Mia", Email = "worker@farm.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Worker@123"), Role = AppRoles.Worker,
                    IsEmailVerified = true, IsActive = true, PhoneNumber = "+8801811000001"
                };
                await db.Users.AddAsync(workerUser);
            }

            await db.SaveChangesAsync();

            // ── Subscriptions ─────────────────────────────────────────────
            if (!await db.Subscriptions.AnyAsync())
            {
                await db.Subscriptions.AddRangeAsync(
                    new Subscription { UserId = owner.Id, Plan = SubscriptionPlan.Owner, PricePaid = 1500, StartDate = DateTime.UtcNow.AddMonths(-1), ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true, TransactionRef = "SEED-001" },
                    new Subscription { UserId = manager.Id, Plan = SubscriptionPlan.Member, PricePaid = 500, StartDate = DateTime.UtcNow.AddMonths(-2), ExpiryDate = DateTime.UtcNow.AddMonths(6), IsActive = true, TransactionRef = "SEED-002" }
                );
                await db.SaveChangesAsync();
            }

            // ── Farms ─────────────────────────────────────────────────────
            if (!await db.Farms.AnyAsync())
            {
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
                var worker1 = new Worker { FarmId = farm1.Id, FullName = "Salim Mia", Role = "Farm Hand", Phone = "+8801811000001", Skills = "Milking, Feeding, Cleaning", ExperienceYears = 5, Salary = 12000, IsAvailable = true, IsActive = true, UserId = workerUser.Id };
                var worker2 = new Worker { FarmId = farm1.Id, FullName = "Raju Bhai", Role = "Herd Manager", Phone = "+8801811000002", Skills = "Herd Management, Record Keeping", ExperienceYears = 8, Salary = 18000, IsAvailable = true, IsActive = true };
                var worker3 = new Worker { FarmId = farm2.Id, FullName = "Jamal Uddin", Role = "Cattle Handler", Phone = "+8801811000003", Skills = "Cattle Handling, Branding, Vaccination", ExperienceYears = 3, Salary = 10000, IsAvailable = true, IsActive = true };
                await db.Workers.AddRangeAsync(worker1, worker2, worker3);

                // ── Doctors ───────────────────────────────────────────────────
                var doc1 = new Doctor { FullName = "Dr. Nasreen Akhter", Specialization = "Bovine Medicine", Phone = "+8801911000001", Email = "dr.nasreen@vet.com", LicenseNumber = "VET-BD-0012", ConsultationFee = 1500, AvailableTimeSlot = "Mon-Fri 9am-5pm", IsAvailable = true, IsActive = true, UserId = doctorUser.Id };
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
                    new Appointment { CattleId = c5.Id, DoctorId = doc1.Id, FarmId = farm2.Id, ScheduledAt = DateTime.UtcNow.AddDays(2), Reason = "Follow-up for fever and appetite loss", Status = AppointmentStatus.Accepted, AcceptedAt = DateTime.UtcNow, CreatedByUserId = owner.Id },
                    new Appointment { CattleId = c1.Id, DoctorId = doc1.Id, FarmId = farm1.Id, ScheduledAt = DateTime.UtcNow.AddDays(7), Reason = "Annual check-up", Status = AppointmentStatus.Pending, CreatedByUserId = owner.Id }
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

            await EnsureRichDemoDataAsync(db, admin);
        }

        private static async Task EnsureRichDemoDataAsync(CattleFarmDbContext db, User admin)
        {
            const int target = 12;
            var now = DateTime.UtcNow;
            var password = BCrypt.Net.BCrypt.HashPassword("Demo@123");
            var roles = new[] { AppRoles.Owner, AppRoles.Manager, AppRoles.Worker, AppRoles.Doctor, AppRoles.Customer, AppRoles.Member };
            var names = new[]
            {
                "Aminul Islam", "Farida Begum", "Mizan Rahman", "Nusrat Jahan",
                "Hasan Mahmud", "Sharmin Akter", "Kamal Uddin", "Rasheda Khatun",
                "Biplob Das", "Samira Chowdhury", "Tariq Hasan", "Lamia Sultana"
            };

            for (var i = 1; i <= target; i++)
            {
                var email = $"demo{i:00}@cattlefarm.com";
                if (!await db.Users.AnyAsync(u => u.Email == email || u.Username == $"demo{i:00}"))
                {
                    await db.Users.AddAsync(new User
                    {
                        Username = $"demo{i:00}",
                        FullName = names[i - 1],
                        Email = email,
                        PasswordHash = password,
                        Role = roles[(i - 1) % roles.Length],
                        PhoneNumber = $"+88017{11000000 + i:00000000}",
                        Address = $"House {i}, Farm Road, Dhaka",
                        IsEmailVerified = true,
                        IsActive = true,
                        SubscriptionType = i % 3 == 0 ? "Owner" : "Member",
                        SubscriptionExpiry = now.AddMonths(6 + i),
                        CreatedAt = now.AddDays(-i)
                    });
                }
            }
            await db.SaveChangesAsync();

            var users = await db.Users.OrderBy(u => u.Id).Take(50).ToListAsync();
            var owners = users.Where(u => u.Role == AppRoles.Owner || u.Role == AppRoles.Admin).DefaultIfEmpty(admin).ToList();
            var workerUsers = users.Where(u => u.Role == AppRoles.Worker).DefaultIfEmpty(users.First()).ToList();
            var managerUsers = users.Where(u => u.Role == AppRoles.Manager).DefaultIfEmpty(admin).ToList();
            var doctorUsers = users.Where(u => u.Role == AppRoles.Doctor).DefaultIfEmpty(users.First()).ToList();
            var customerUsers = users.Where(u => u.Role == AppRoles.Customer || u.Role == AppRoles.Member).DefaultIfEmpty(users.First()).ToList();

            var farmNames = new[]
            {
                "Green Valley Dairy", "Padma Agro Farm", "Meghna Cattle Estate", "Jamuna Beef Ranch",
                "Rupsha Organic Farm", "Surma Milk House", "Karnafuli Livestock", "Teesta Breeding Center",
                "Brahmaputra Dairy", "Halda Natural Farm", "Madhumati Ranch", "Buriganga Fresh Farm"
            };

            for (var i = 1; i <= target; i++)
            {
                if (!await db.Farms.AnyAsync(f => f.Name == farmNames[i - 1]))
                {
                    await db.Farms.AddAsync(new Farm
                    {
                        OwnerId = owners[(i - 1) % owners.Count].Id,
                        Name = farmNames[i - 1],
                        Location = $"{new[] { "Dhaka", "Gazipur", "Narayanganj", "Savar", "Manikganj", "Tangail" }[(i - 1) % 6]}, Bangladesh",
                        FarmType = (FarmType)((i - 1) % Enum.GetValues<FarmType>().Length),
                        SizeInAcres = 20 + i * 5,
                        Capacity = 80 + i * 20,
                        MaximumWorkers = 30 + i,
                        MaximumCattle = 150 + i * 20,
                        Description = $"Demo farm {i} with healthy cattle, feed planning, and regular veterinary checks.",
                        Latitude = 23.70 + i * 0.01,
                        Longitude = 90.30 + i * 0.01,
                        ApprovalStatus = ApprovalStatus.Approved,
                        IsActive = true,
                        CreatedAt = now.AddDays(-40 - i)
                    });
                }
            }
            await db.SaveChangesAsync();

            var farms = await db.Farms.OrderBy(f => f.Id).Take(50).ToListAsync();
            if (farms.Count == 0)
            {
                return;
            }

            var workerRoles = new[] { "Milker", "Feeder", "Cleaner", "Herd Assistant", "Veterinary Assistant", "Night Guard" };
            for (var i = 1; i <= target; i++)
            {
                var phone = $"+88018{22000000 + i:00000000}";
                if (!await db.Workers.AnyAsync(w => w.Phone == phone))
                {
                    var workerUser = workerUsers[(i - 1) % workerUsers.Count];
                    await db.Workers.AddAsync(new Worker
                    {
                        FarmId = farms[(i - 1) % farms.Count].Id,
                        UserId = workerUser.Id,
                        FullName = $"Demo Worker {i:00}",
                        Role = workerRoles[(i - 1) % workerRoles.Length],
                        Phone = phone,
                        Email = $"worker{i:00}@farm.local",
                        Skills = "Milking, feeding, cleaning, animal handling",
                        ExperienceYears = 1 + (i % 10),
                        Salary = 12000 + i * 900,
                        IsAvailable = i % 4 != 0,
                        IsActive = true,
                        HiredAt = now.AddMonths(-i),
                        CreatedAt = now.AddMonths(-i)
                    });
                }
            }

            for (var i = 1; i <= target; i++)
            {
                var license = $"DEMO-VET-{i:000}";
                if (!await db.Doctors.AnyAsync(d => d.LicenseNumber == license))
                {
                    await db.Doctors.AddAsync(new Doctor
                    {
                        UserId = doctorUsers[(i - 1) % doctorUsers.Count].Id,
                        FullName = $"Dr. Demo Vet {i:00}",
                        Specialization = new[] { "Bovine Medicine", "Veterinary Surgery", "Dairy Health", "Reproduction" }[(i - 1) % 4],
                        Phone = $"+88019{33000000 + i:00000000}",
                        Email = $"vet{i:00}@cattlefarm.com",
                        LicenseNumber = license,
                        ConsultationFee = 1000 + i * 150,
                        IsAvailable = true,
                        IsActive = true,
                        IsVerified = true,
                        ApprovalStatus = ApprovalStatus.Approved,
                        YearsOfExperience = 2 + i,
                        AvailableDays = "Saturday,Sunday,Monday,Tuesday,Wednesday",
                        AvailableTimeFrom = "09:00",
                        AvailableTimeTo = "17:00",
                        AvailableTimeSlot = "Sat-Wed 9am-5pm",
                        CreatedAt = now.AddDays(-30 - i)
                    });
                }
            }
            await db.SaveChangesAsync();

            var workers = await db.Workers.OrderBy(w => w.Id).Take(50).ToListAsync();
            var doctors = await db.Doctors.OrderBy(d => d.Id).Take(50).ToListAsync();
            var hasWeightRecords = await TableExistsAsync(db, "WeightRecords");
            var hasHeatRecords = await TableExistsAsync(db, "HeatRecords");
            var hasBullPerformanceRecords = await TableExistsAsync(db, "BullPerformanceRecords");
            var hasFeedInventories = await TableExistsAsync(db, "FeedInventories");
            var hasSensorReadings = await TableExistsAsync(db, "SensorReadings");
            var hasGpsTrackerSnapshots = await TableExistsAsync(db, "GpsTrackerSnapshots");
            var hasAutomatedFeedingCommands = await TableExistsAsync(db, "AutomatedFeedingCommands");
            var hasMilkMachineImports = await TableExistsAsync(db, "MilkMachineImports");
            var hasOfflineSyncItems = await TableExistsAsync(db, "OfflineSyncItems");

            var breeds = new[] { "Holstein Friesian", "Jersey", "Sahiwal", "Brahman", "Red Sindhi", "Local Cross" };
            for (var i = 1; i <= target; i++)
            {
                var farm = farms[(i - 1) % farms.Count];
                var tag = $"DEMO-{i:000}";
                if (!await db.Cattles.AnyAsync(c => c.FarmId == farm.Id && c.TagId == tag))
                {
                    var isMale = i % 4 == 0;
                    await db.Cattles.AddAsync(new Cattle
                    {
                        FarmId = farm.Id,
                        TagId = tag,
                        Name = $"Demo Cattle {i:00}",
                        Breed = breeds[(i - 1) % breeds.Length],
                        DateOfBirth = now.Date.AddYears(-(2 + i % 5)).AddDays(i * 11),
                        Weight = 260 + i * 28,
                        Gender = isMale ? Gender.Male : Gender.Female,
                        Category = isMale ? CattleCategory.Bull : CattleCategory.DairyCow,
                        HealthStatus = i % 6 == 0 ? HealthStatus.AtRisk : HealthStatus.Healthy,
                        Status = CattleStatus.Active,
                        Origin = "Demo seed stock",
                        PurchasePrice = 65000 + i * 8500,
                        PurchaseDate = now.AddMonths(-18 - i),
                        IsListedForSale = i % 5 == 0,
                        SalePrice = i % 5 == 0 ? 120000 + i * 10000 : null,
                        IsPremiumListing = i % 5 == 0,
                        ApprovalStatus = ApprovalStatus.Approved,
                        Description = $"Healthy demo cattle record {i} for testing farm workflows.",
                        CreatedAt = now.AddDays(-25 - i)
                    });
                }
            }
            await db.SaveChangesAsync();

            var cattle = await db.Cattles.OrderBy(c => c.Id).Take(100).ToListAsync();
            if (cattle.Count == 0 || workers.Count == 0 || doctors.Count == 0)
            {
                return;
            }

            var products = new[]
            {
                ("Fresh Whole Milk", ProductCategory.Milk, "Liter", 75m),
                ("Premium Beef", ProductCategory.Beef, "kg", 780m),
                ("Organic Manure", ProductCategory.Manure, "kg", 25m),
                ("Breeding Service", ProductCategory.BreedingService, "Service", 4500m),
                ("Ghee", ProductCategory.Other, "Jar", 650m),
                ("Paneer", ProductCategory.Other, "kg", 420m)
            };
            for (var i = 1; i <= target; i++)
            {
                var spec = products[(i - 1) % products.Length];
                var name = $"{spec.Item1} Demo {i:00}";
                if (!await db.Products.AnyAsync(p => p.Name == name))
                {
                    await db.Products.AddAsync(new Product
                    {
                        FarmId = farms[(i - 1) % farms.Count].Id,
                        Name = name,
                        Category = spec.Item2,
                        Description = $"Quality {spec.Item1.ToLower()} from verified demo farm stock.",
                        Price = spec.Item4 + i * 5,
                        StockQuantity = 100 + i * 25,
                        Unit = spec.Item3,
                        MinStockLevel = 10 + i,
                        IsAvailable = true,
                        IsFeatured = i % 3 == 0,
                        CreatedAt = now.AddDays(-i)
                    });
                }
            }
            await db.SaveChangesAsync();

            var productRows = await db.Products.OrderBy(p => p.Id).Take(100).ToListAsync();

            for (var i = 1; i <= target; i++)
            {
                var cow = cattle[(i - 1) % cattle.Count];
                var farm = farms.FirstOrDefault(f => f.Id == cow.FarmId) ?? farms[0];
                var doctor = doctors[(i - 1) % doctors.Count];
                var worker = workers[(i - 1) % workers.Count];
                var user = users[(i - 1) % users.Count];
                var product = productRows[(i - 1) % productRows.Count];

                if (await db.HealthRecords.CountAsync() < target)
                {
                    await db.HealthRecords.AddAsync(new HealthRecord { CattleId = cow.Id, DoctorId = doctor.Id, RecordDate = now.AddDays(-i), Temperature = 38.1 + (i % 4) * 0.2, Weight = cow.Weight, HealthStatus = cow.HealthStatus, RiskLevel = i % 6 == 0 ? RiskLevel.Medium : RiskLevel.Low, Notes = $"Routine demo health check {i}." });
                }
                if (await db.Vaccinations.CountAsync() < target)
                {
                    await db.Vaccinations.AddAsync(new Vaccination { CattleId = cow.Id, DoctorId = doctor.Id, VaccineName = new[] { "FMD Vaccine", "Anthrax Vaccine", "Black Quarter Vaccine", "HS Vaccine" }[(i - 1) % 4], VaccinationDate = now.AddMonths(-i), NextDueDate = now.AddMonths(6 - i % 3), AdministeredBy = doctor.FullName, DoseNumber = 1 + i % 2, BatchNumber = $"VAC-{i:000}" });
                }
                if (await db.MedicineRecords.CountAsync() < target)
                {
                    await db.MedicineRecords.AddAsync(new MedicineRecord { CattleId = cow.Id, PrescribedByDoctorId = doctor.Id, MedicineName = new[] { "Vitamin AD3E", "Calcium Plus", "Dewormer", "Electrolyte" }[(i - 1) % 4], Dosage = "10 ml daily", StartDate = now.AddDays(-i), EndDate = now.AddDays(3 - i), IsCompleted = i % 2 == 0, Notes = $"Demo medicine course {i}." });
                }
                if (await db.MilkProductions.CountAsync() < target)
                {
                    await db.MilkProductions.AddAsync(new MilkProduction { CattleId = cow.Id, FarmId = farm.Id, RecordedByWorkerId = worker.Id, Date = now.Date.AddDays(-i), MorningYieldLiters = 4 + i % 6, EveningYieldLiters = 3 + i % 5, FatPercentage = 3.5m + (i % 3) * 0.2m, ProteinLevel = 3.1m + (i % 2) * 0.1m, SolidNotFat = 8.2m + (i % 3) * 0.1m, MilkQualityGrade = i % 5 == 0 ? "B" : "A", Notes = $"Demo milk record {i}." });
                }
                if (await db.CattleMedicalRecords.CountAsync() < target)
                {
                    await db.CattleMedicalRecords.AddAsync(new CattleMedicalRecord { CattleId = cow.Id, DoctorId = doctorUsers[(i - 1) % doctorUsers.Count].Id, ExaminationDate = now.AddDays(-i), ChiefComplaint = "Routine examination", Diagnosis = "Normal condition", Prescription = "Balanced feed and clean water", MedicineName = "Vitamin Mix", MedicineDose = "10 ml", DoseFrequency = "Daily", DoseDurationDays = 5, NextVisitDate = now.AddDays(30 + i), Notes = $"Demo medical record {i}." });
                }
                if (hasWeightRecords && await db.WeightRecords.CountAsync() < target)
                {
                    await db.WeightRecords.AddAsync(new WeightRecord { CattleId = cow.Id, FarmId = farm.Id, RecordedByUserId = admin.Id, MeasuredAt = now.Date.AddDays(-i), WeightKg = (decimal)cow.Weight, BodyConditionScore = $"{4 + i % 3}/9", Notes = $"Demo weight entry {i}." });
                }
                if (hasHeatRecords && cow.Gender == Gender.Female && await db.HeatRecords.CountAsync() < target)
                {
                    await db.HeatRecords.AddAsync(new HeatRecord { CattleId = cow.Id, FarmId = farm.Id, ObservationDate = now.Date.AddDays(-i), HeatStatus = i % 2 == 0 ? HeatStatus.InHeat : HeatStatus.NotInHeat, HeatDurationHours = 8 + i, NextExpectedHeatDate = now.Date.AddDays(21 + i), ObservedBy = worker.FullName, DetectionMethod = "Visual", ReadyForBreeding = i % 2 == 0, Notes = $"Demo heat observation {i}." });
                }
                if (hasBullPerformanceRecords && cow.Gender == Gender.Male && await db.BullPerformanceRecords.CountAsync() < target)
                {
                    await db.BullPerformanceRecords.AddAsync(new BullPerformanceRecord { CattleId = cow.Id, FarmId = farm.Id, EvaluationDate = now.Date.AddDays(-i), MotilityPercent = 70 + i, MorphologyPercent = 75 + i, ConcentrationMillionPerMl = 900 + i * 20, VolumeML = 4 + i % 4, QualityGrade = SemenQuality.Good, EvaluatedBy = doctor.FullName, DosesCollected = 20 + i, Cost = 1200 + i * 100, Notes = $"Demo bull performance {i}." });
                }
                if (await db.CattleExpenses.CountAsync() < target)
                {
                    await db.CattleExpenses.AddAsync(new CattleExpense { CattleId = cow.Id, CreatedByUserId = admin.Id, Category = (CattleExpenseCategory)((i - 1) % Enum.GetValues<CattleExpenseCategory>().Length), Amount = 500 + i * 120, Date = now.Date.AddDays(-i), Description = $"Demo cattle cost {i}." });
                }
                if (await db.FeedRecords.CountAsync() < target)
                {
                    await db.FeedRecords.AddAsync(new FeedRecord { FarmId = farm.Id, CattleId = cow.Id, RecordedByWorkerId = worker.Id, FeedType = (FeedType)((i - 1) % Enum.GetValues<FeedType>().Length), FeedName = new[] { "Napier Grass", "Silage Mix", "Concentrate", "Mineral Mix" }[(i - 1) % 4], QuantityKg = 15 + i, CostPerKg = 18 + i, Date = now.Date.AddDays(-i), Supplier = "Demo Supplier", Notes = $"Demo feed record {i}." });
                }
                if (await db.Expenses.CountAsync() < target)
                {
                    await db.Expenses.AddAsync(new Expense { FarmId = farm.Id, CreatedByUserId = admin.Id, Category = (ExpenseCategory)((i - 1) % Enum.GetValues<ExpenseCategory>().Length), Amount = 3000 + i * 750, Date = now.Date.AddDays(-i * 2), Description = $"Demo farm expense {i}.", IsApproved = true });
                }
                if (await db.Revenues.CountAsync() < target)
                {
                    await db.Revenues.AddAsync(new Revenue { FarmId = farm.Id, CreatedByUserId = admin.Id, Source = (RevenueSource)((i - 1) % Enum.GetValues<RevenueSource>().Length), Amount = 6000 + i * 1200, Date = now.Date.AddDays(-i * 2), Description = $"Demo farm revenue {i}." });
                }
                if (await db.Reviews.CountAsync() < target)
                {
                    await db.Reviews.AddAsync(new Review { ReviewerId = customerUsers[(i - 1) % customerUsers.Count].Id, TargetType = ReviewTargetType.Product, TargetId = product.Id, Rating = 3 + i % 3, Comment = $"Good demo product quality #{i}.", IsApproved = true, CreatedAt = now.AddDays(-i) });
                }
                if (await db.Appointments.CountAsync() < target)
                {
                    await db.Appointments.AddAsync(new Appointment { CattleId = cow.Id, DoctorId = doctor.Id, FarmId = farm.Id, ScheduledAt = now.AddDays(i), Reason = $"Demo appointment {i}", Status = i % 3 == 0 ? AppointmentStatus.Accepted : AppointmentStatus.Pending, CreatedByUserId = admin.Id });
                }
                if (await db.TaskAssignments.CountAsync() < target)
                {
                    await db.TaskAssignments.AddAsync(new TaskAssignment { FarmId = farm.Id, AssignedWorkerId = worker.Id, AssignedUserId = worker.UserId ?? workerUsers[(i - 1) % workerUsers.Count].Id, CreatedBy = admin.Id, Title = $"Demo task {i:00}", Description = "Daily farm operation demo task.", Priority = new[] { TaskPriority.Low, TaskPriority.Medium, TaskPriority.High, TaskPriority.Emergency }[(i - 1) % 4], TaskType = TaskTypes.Direct, Status = i % 3 == 0 ? CattleFarm.Models.TaskStatus.Completed : CattleFarm.Models.TaskStatus.Pending, DueDate = now.AddDays(i), ExpiresAt = now.AddDays(i + 2), BonusAmount = i % 3 == 0 ? 250 : 0 });
                }
                if (await db.Notifications.CountAsync() < target)
                {
                    await db.Notifications.AddAsync(new Notification { UserId = user.Id, Title = $"Demo notice {i:00}", Message = $"Demo notification message {i}.", Type = (NotificationType)((i - 1) % Enum.GetValues<NotificationType>().Length), RelatedEntityType = "Cattle", RelatedEntityId = cow.Id, CreatedAt = now.AddDays(-i) });
                }
                if (await db.AuditLogs.CountAsync() < target)
                {
                    await db.AuditLogs.AddAsync(new AuditLog { UserId = admin.Id, Action = "SEED", EntityName = "DemoData", EntityId = i, NewValues = $"{{\"Seed\":{i}}}", IPAddress = "127.0.0.1", Timestamp = now.AddDays(-i) });
                }
                if (await db.ActivityLogs.CountAsync() < target)
                {
                    await db.ActivityLogs.AddAsync(new ActivityLog { UserId = admin.Id, Description = $"Demo activity log {i}.", EntityName = "DemoData", EntityId = i, IPAddress = "127.0.0.1", Timestamp = now.AddDays(-i) });
                }
                if (await db.WorkerAttendances.CountAsync() < target)
                {
                    var date = now.Date.AddDays(-i);
                    await db.WorkerAttendances.AddAsync(new WorkerAttendance { WorkerId = worker.Id, Date = date, CheckIn = date.AddHours(8), CheckOut = date.AddHours(16), Status = AttendanceStatus.Present, HoursWorked = 8, Notes = $"Demo worker attendance {i}." });
                }
                if (await db.Attendances.CountAsync() < target && !await db.Attendances.AnyAsync(a => a.WorkerId == worker.Id && a.Date == now.Date.AddDays(-i)))
                {
                    await db.Attendances.AddAsync(new Attendance { WorkerId = worker.Id, Date = now.Date.AddDays(-i), Status = "Present", MarkedByUserId = admin.Id, MarkedAt = now.AddDays(-i).AddHours(8) });
                }
                if (await db.Payrolls.CountAsync() < target)
                {
                    var baseSalary = worker.Salary;
                    await db.Payrolls.AddAsync(new Payroll { WorkerId = worker.Id, UserId = worker.UserId ?? workerUsers[(i - 1) % workerUsers.Count].Id, FarmId = farm.Id, Year = now.Year, Month = ((now.Month + i - 1) % 12) + 1, BaseSalary = baseSalary, OvertimeHours = i % 8, OvertimePay = (i % 8) * 120, Bonus = i % 3 == 0 ? 500 : 0, Deductions = i % 5 == 0 ? 200 : 0, NetSalary = baseSalary + (i % 8) * 120 + (i % 3 == 0 ? 500 : 0) - (i % 5 == 0 ? 200 : 0), IsPaid = i % 2 == 0, PaidAt = i % 2 == 0 ? now.AddDays(-i) : null });
                }
                if (await db.SalaryHistories.CountAsync() < target)
                {
                    await db.SalaryHistories.AddAsync(new SalaryHistory { FarmId = farm.Id, WorkerId = worker.Id, WorkerUserId = worker.UserId ?? workerUsers[(i - 1) % workerUsers.Count].Id, BaseSalary = worker.Salary, Bonus = i % 3 == 0 ? 500 : 0, TotalSalary = worker.Salary + (i % 3 == 0 ? 500 : 0), Year = now.Year, Month = ((now.Month + i - 1) % 12) + 1, UpdatedByUserId = admin.Id });
                }
                if (await db.Subscriptions.CountAsync() < target)
                {
                    await db.Subscriptions.AddAsync(new Subscription { UserId = user.Id, Plan = (SubscriptionPlan)((i - 1) % Enum.GetValues<SubscriptionPlan>().Length), StartDate = now.AddMonths(-i), ExpiryDate = now.AddMonths(12 - i % 6), IsActive = true, AutoRenew = i % 2 == 0, PricePaid = 500 + i * 100, TransactionRef = $"DEMO-SUB-{i:000}" });
                }
            }
            await db.SaveChangesAsync();

            if (hasHeatRecords && await db.HeatRecords.CountAsync() < target)
            {
                var femaleCattle = cattle.Where(c => c.Gender == Gender.Female).DefaultIfEmpty(cattle[0]).ToList();
                var heatCount = await db.HeatRecords.CountAsync();
                for (var i = heatCount + 1; i <= target; i++)
                {
                    var cow = femaleCattle[(i - 1) % femaleCattle.Count];
                    var farm = farms.FirstOrDefault(f => f.Id == cow.FarmId) ?? farms[0];
                    await db.HeatRecords.AddAsync(new HeatRecord
                    {
                        CattleId = cow.Id,
                        FarmId = farm.Id,
                        ObservationDate = now.Date.AddDays(-i * 2),
                        HeatStatus = i % 2 == 0 ? HeatStatus.InHeat : HeatStatus.NotInHeat,
                        HeatDurationHours = 6 + i,
                        NextExpectedHeatDate = now.Date.AddDays(21 + i),
                        ObservedBy = workers[(i - 1) % workers.Count].FullName,
                        DetectionMethod = "Visual",
                        ReadyForBreeding = i % 2 == 0,
                        Notes = $"Additional demo heat observation {i}."
                    });
                }
            }

            if (hasBullPerformanceRecords && await db.BullPerformanceRecords.CountAsync() < target)
            {
                var maleCattle = cattle.Where(c => c.Gender == Gender.Male).DefaultIfEmpty(cattle[0]).ToList();
                var bullCount = await db.BullPerformanceRecords.CountAsync();
                for (var i = bullCount + 1; i <= target; i++)
                {
                    var bull = maleCattle[(i - 1) % maleCattle.Count];
                    var farm = farms.FirstOrDefault(f => f.Id == bull.FarmId) ?? farms[0];
                    await db.BullPerformanceRecords.AddAsync(new BullPerformanceRecord
                    {
                        CattleId = bull.Id,
                        FarmId = farm.Id,
                        EvaluationDate = now.Date.AddDays(-i * 3),
                        MotilityPercent = 72 + i,
                        MorphologyPercent = 76 + i,
                        ConcentrationMillionPerMl = 950 + i * 15,
                        VolumeML = 4 + i % 4,
                        QualityGrade = SemenQuality.Good,
                        EvaluatedBy = doctors[(i - 1) % doctors.Count].FullName,
                        DosesCollected = 18 + i,
                        Cost = 1100 + i * 90,
                        Notes = $"Additional demo bull performance {i}."
                    });
                }
            }

            await db.SaveChangesAsync();

            var orders = await db.Orders.OrderBy(o => o.Id).Take(100).ToListAsync();
            for (var i = 1; i <= target; i++)
            {
                var farm = farms[(i - 1) % farms.Count];
                var customer = customerUsers[(i - 1) % customerUsers.Count];
                var product = productRows[(i - 1) % productRows.Count];

                if (await db.Orders.CountAsync() < target)
                {
                    var order = new Order
                    {
                        CustomerId = customer.Id,
                        FarmId = farm.Id,
                        OrderStatus = i % 4 == 0 ? OrderStatus.Delivered : OrderStatus.Confirmed,
                        PaymentStatus = i % 4 == 0 ? PaymentStatus.Completed : PaymentStatus.Pending,
                        TotalAmount = product.Price * (1 + i % 5),
                        DeliveryAddress = $"Demo delivery address {i}, Dhaka",
                        Notes = $"Demo order {i}.",
                        OrderDate = now.AddDays(-i),
                        DeliveredAt = i % 4 == 0 ? now.AddDays(-i + 1) : null
                    };
                    await db.Orders.AddAsync(order);
                    await db.SaveChangesAsync();
                    orders.Add(order);
                }

                var currentOrder = orders[(i - 1) % orders.Count];
                if (await db.OrderItems.CountAsync() < target)
                {
                    await db.OrderItems.AddAsync(new OrderItem { OrderId = currentOrder.Id, ProductId = product.Id, Quantity = 1 + i % 5, UnitPrice = product.Price, TotalPrice = product.Price * (1 + i % 5) });
                }
                if (await db.Payments.CountAsync() < target)
                {
                    await db.Payments.AddAsync(new Payment { UserId = customer.Id, OrderId = currentOrder.Id, Amount = currentOrder.TotalAmount, Method = (PaymentMethod)((i - 1) % Enum.GetValues<PaymentMethod>().Length), Status = currentOrder.PaymentStatus, Purpose = PaymentPurpose.Order, TransactionId = $"DEMO-PAY-{i:000}", ReferenceId = currentOrder.Id, ReferenceType = "Order", PaymentDate = now.AddDays(-i), Notes = $"Demo payment {i}." });
                }
            }
            await db.SaveChangesAsync();

            orders = await db.Orders.OrderBy(o => o.Id).Take(100).ToListAsync();
            for (var i = 1; i <= target; i++)
            {
                var farm = farms[(i - 1) % farms.Count];
                var cow = cattle[(i - 1) % cattle.Count];
                var user = users[(i - 1) % users.Count];
                var workerUser = workerUsers[(i - 1) % workerUsers.Count];
                var managerUser = managerUsers[(i - 1) % managerUsers.Count];
                var worker = workers[(i - 1) % workers.Count];

                if (await db.FarmJoinRequests.CountAsync() < target)
                {
                    await db.FarmJoinRequests.AddAsync(new FarmJoinRequest { FarmId = farm.Id, WorkerUserId = workerUser.Id, ApplicantRole = i % 4 == 0 ? JoinApplicantRole.Manager : JoinApplicantRole.Worker, Status = new[] { JoinRequestStatus.Applied, JoinRequestStatus.Pending, JoinRequestStatus.Accepted, JoinRequestStatus.Rejected }[(i - 1) % 4], Message = $"Demo join request {i}.", OwnerNote = i % 4 > 1 ? "Reviewed demo request." : null, AppliedAt = now.AddDays(-i), ReviewedAt = i % 4 > 1 ? now.AddDays(-i + 1) : null, CanReApplyAt = i % 4 == 0 ? now.AddDays(7) : null });
                }
                if (await db.FarmWorkers.CountAsync() < target && !await db.FarmWorkers.AnyAsync(fw => fw.FarmId == farm.Id && fw.WorkerUserId == workerUser.Id))
                {
                    await db.FarmWorkers.AddAsync(new FarmWorker { FarmId = farm.Id, WorkerUserId = workerUser.Id, Position = new[] { WorkerPosition.Feeder, WorkerPosition.Cleaner, WorkerPosition.Milker, WorkerPosition.VeterinaryAssistant }[(i - 1) % 4], WorkerStatus = new[] { WorkerStatusType.Available, WorkerStatusType.Busy, WorkerStatusType.Offline, WorkerStatusType.OnLeave }[(i - 1) % 4], Salary = 11000 + i * 700, JoinedAt = now.AddMonths(-i), IsActive = true });
                }
                if (await db.FarmManagers.CountAsync() < target && !await db.FarmManagers.AnyAsync(fm => fm.FarmId == farm.Id && fm.ManagerUserId == managerUser.Id))
                {
                    await db.FarmManagers.AddAsync(new FarmManager { FarmId = farm.Id, ManagerUserId = managerUser.Id, Position = "Farm Manager", JoinedAt = now.AddMonths(-i), IsActive = true });
                }
                if (await db.LeaveRequests.CountAsync() < target)
                {
                    await db.LeaveRequests.AddAsync(new LeaveRequest { FarmId = farm.Id, WorkerUserId = workerUser.Id, Status = new[] { LeaveRequestStatus.Pending, LeaveRequestStatus.Approved, LeaveRequestStatus.Rejected }[(i - 1) % 3], StartsAt = now.Date.AddDays(i), EndsAt = now.Date.AddDays(i + 2), Reason = $"Demo leave reason {i}.", OwnerNote = i % 3 == 0 ? "Reviewed." : null, ReviewedByUserId = i % 3 == 0 ? admin.Id : null, ReviewedAt = i % 3 == 0 ? now : null });
                }
                if (await db.Breedings.CountAsync() < target)
                {
                    var dam = cattle.FirstOrDefault(c => c.Gender == Gender.Female) ?? cow;
                    var sire = cattle.FirstOrDefault(c => c.Gender == Gender.Male);
                    await db.Breedings.AddAsync(new Breeding { FarmId = farm.Id, CattleId = dam.Id, SireId = sire?.Id, BreedingDate = now.Date.AddDays(-i * 10), ExpectedCalvingDate = now.Date.AddDays(280 - i), Method = i % 2 == 0 ? BreedingMethod.ArtificialInsemination : BreedingMethod.Natural, Outcome = (BreedingOutcome)((i - 1) % Enum.GetValues<BreedingOutcome>().Length), CalvesCount = i % 3 == 0 ? 1 : null, SireBreed = sire?.Breed, InseminationTechnician = "Demo Technician", Cost = 1500 + i * 120, Notes = $"Demo breeding record {i}." });
                }
                if (await db.CattleLikes.CountAsync() < target && !await db.CattleLikes.AnyAsync(l => l.CattleId == cow.Id && l.UserId == user.Id))
                {
                    await db.CattleLikes.AddAsync(new CattleLike { CattleId = cow.Id, UserId = user.Id, CreatedAt = now.AddDays(-i) });
                }
                if (await db.CattleComments.CountAsync() < target)
                {
                    await db.CattleComments.AddAsync(new CattleComment { CattleId = cow.Id, UserId = user.Id, Comment = $"Healthy looking demo cattle #{i}.", CreatedAt = now.AddDays(-i) });
                }
                if (await db.CattleShares.CountAsync() < target)
                {
                    await db.CattleShares.AddAsync(new CattleShare { CattleId = cow.Id, UserId = user.Id, Channel = i % 2 == 0 ? "Facebook" : "Link", ShareUrl = $"https://demo.local/cattle/{cow.Id}?share={i}", CreatedAt = now.AddDays(-i) });
                }
                if (await db.DoctorInvitations.CountAsync() < target)
                {
                    await db.DoctorInvitations.AddAsync(new DoctorInvitation { FarmId = farm.Id, CreatedByUserId = admin.Id, Token = $"demo-token-{i:000}-{Guid.NewGuid():N}".Substring(0, 32), DoctorName = $"Invited Vet {i:00}", Email = $"invitedvet{i:00}@cattlefarm.com", PhoneNumber = $"+88016{44000000 + i:00000000}", Notes = $"Demo invitation {i}.", ExpectedJoiningDate = now.AddDays(10 + i), ExpiresAt = now.AddDays(7 + i), InvitationStatus = InvitationStatus.Pending });
                }
                if (hasFeedInventories && await db.FeedInventories.CountAsync() < target && !await db.FeedInventories.AnyAsync(fi => fi.FarmId == farm.Id && fi.FeedType == (FeedType)((i - 1) % Enum.GetValues<FeedType>().Length)))
                {
                    await db.FeedInventories.AddAsync(new FeedInventory { FarmId = farm.Id, FeedType = (FeedType)((i - 1) % Enum.GetValues<FeedType>().Length), StockQuantityKg = 500 + i * 75, MinStockThresholdKg = 80 + i * 5, LastUpdated = now.AddDays(-i) });
                }
                if (hasSensorReadings && await db.SensorReadings.CountAsync() < target)
                {
                    await db.SensorReadings.AddAsync(new SensorReading { FarmId = farm.Id, DeviceId = $"DEMO-SENSOR-{i:000}", ReadingType = (SensorReadingType)((i - 1) % Enum.GetValues<SensorReadingType>().Length), Value = 20 + i, Unit = i % 2 == 0 ? "C" : "%", RecordedAt = now.AddMinutes(-i * 30), BarnZone = $"Barn-{(i % 4) + 1}" });
                }
                if (hasGpsTrackerSnapshots && await db.GpsTrackerSnapshots.CountAsync() < target)
                {
                    await db.GpsTrackerSnapshots.AddAsync(new GpsTrackerSnapshot { FarmId = farm.Id, CattleId = cow.Id, TrackerId = $"DEMO-GPS-{i:000}", Latitude = 23.700000m + i / 1000m, Longitude = 90.300000m + i / 1000m, SpeedKph = i % 3, RecordedAt = now.AddMinutes(-i * 20) });
                }
                if (hasAutomatedFeedingCommands && await db.AutomatedFeedingCommands.CountAsync() < target)
                {
                    await db.AutomatedFeedingCommands.AddAsync(new AutomatedFeedingCommand { FarmId = farm.Id, CattleId = cow.Id, ControllerId = $"DEMO-FEEDER-{i:000}", FeedName = "Concentrate Mix", QuantityKg = 2 + i % 5, ScheduledAt = now.AddHours(i), Status = (FeedingCommandStatus)((i - 1) % Enum.GetValues<FeedingCommandStatus>().Length), Notes = $"Demo feeding command {i}." });
                }
                if (hasMilkMachineImports && await db.MilkMachineImports.CountAsync() < target)
                {
                    await db.MilkMachineImports.AddAsync(new MilkMachineImport { FarmId = farm.Id, CattleId = cow.Id, MachineId = $"DEMO-MILK-{i:000}", YieldLiters = 7 + i % 6, FatPercentage = 3.5m + (i % 3) * 0.2m, ProteinPercentage = 3.1m + (i % 3) * 0.1m, CollectedAt = now.AddHours(-i), ConvertedToMilkRecord = i % 2 == 0 });
                }
                if (hasOfflineSyncItems && await db.OfflineSyncItems.CountAsync() < target)
                {
                    await db.OfflineSyncItems.AddAsync(new OfflineSyncItem { FarmId = farm.Id, ClientId = $"DEMO-CLIENT-{i:000}", EntityName = "MilkProduction", PayloadJson = $"{{\"demo\":true,\"index\":{i}}}", Status = (OfflineSyncStatus)((i - 1) % Enum.GetValues<OfflineSyncStatus>().Length), ReceivedAt = now.AddMinutes(-i * 15), ErrorMessage = i % 4 == 0 ? "Demo resolved retry case" : null });
                }
            }
            await db.SaveChangesAsync();

            var vehicles = await db.Vehicles.OrderBy(v => v.Id).Take(100).ToListAsync();
            for (var i = 1; i <= target; i++)
            {
                var registration = $"DHK-DEMO-{i:000}";
                if (!await db.Vehicles.AnyAsync(v => v.RegistrationNumber == registration))
                {
                    await db.Vehicles.AddAsync(new Vehicle { Name = $"Demo Vehicle {i:00}", Type = (VehicleType)((i - 1) % Enum.GetValues<VehicleType>().Length), RegistrationNumber = registration, Capacity = 2 + i, CapacityUnit = "tonnes", FuelType = (FuelType)((i - 1) % Enum.GetValues<FuelType>().Length), FuelCostPerKm = 25 + i, Status = VehicleStatus.Available, Notes = $"Demo transport vehicle {i}." });
                }
                var license = $"DRV-DEMO-{i:000}";
                if (!await db.Drivers.AnyAsync(d => d.LicenseNumber == license))
                {
                    await db.Drivers.AddAsync(new Driver { FullName = $"Demo Driver {i:00}", Phone = $"+88015{55000000 + i:00000000}", LicenseNumber = license, LicenseType = i % 2 == 0 ? "Heavy" : "Commercial", ExperienceYears = 2 + i, Address = $"Driver address {i}, Dhaka", Rating = 4.0m + (i % 10) / 10m, Status = DriverStatus.Available, Notes = $"Demo driver {i}." });
                }
            }
            await db.SaveChangesAsync();

            vehicles = await db.Vehicles.OrderBy(v => v.Id).Take(100).ToListAsync();
            var drivers = await db.Drivers.OrderBy(d => d.Id).Take(100).ToListAsync();
            for (var i = 1; i <= target; i++)
            {
                var farm = farms[(i - 1) % farms.Count];
                var requester = users[(i - 1) % users.Count];
                var order = orders.Count > 0 ? orders[(i - 1) % orders.Count] : null;

                if (await db.TransportRequests.CountAsync() < target)
                {
                    var request = new TransportRequest { FarmId = farm.Id, OrderId = order?.Id, RequestedByUserId = requester.Id, RequestType = (TransportType)((i - 1) % Enum.GetValues<TransportType>().Length), PickupLocation = farm.Location, Destination = $"Demo destination {i}, Dhaka", ScheduledDate = now.Date.AddDays(i), ScheduledTime = TimeSpan.FromHours(8 + i % 8), EstimatedDistanceKm = 15 + i * 3, CargoWeight = 1 + i % 4, CargoDescription = $"Demo cargo {i}", Status = TripStatus.Assigned, Notes = $"Demo transport request {i}." };
                    await db.TransportRequests.AddAsync(request);
                    await db.SaveChangesAsync();

                    if (vehicles.Count > 0 && drivers.Count > 0 && await db.Trips.CountAsync() < target)
                    {
                        var vehicle = vehicles[(i - 1) % vehicles.Count];
                        var driver = drivers[(i - 1) % drivers.Count];
                        var distance = 15 + i * 3;
                        var fuelCost = distance * vehicle.FuelCostPerKm;
                        var baseCost = 800 + i * 100;
                        await db.Trips.AddAsync(new Trip { TransportRequestId = request.Id, VehicleId = vehicle.Id, DriverId = driver.Id, Status = i % 3 == 0 ? TripStatus.Completed : TripStatus.Assigned, StartTime = now.AddHours(-i), EndTime = i % 3 == 0 ? now.AddHours(-i + 2) : null, DistanceKm = distance, FuelCostPerKm = vehicle.FuelCostPerKm, BaseCost = baseCost, FuelCost = fuelCost, AdditionalCost = i % 4 == 0 ? 200 : 0, TotalCost = baseCost + fuelCost + (i % 4 == 0 ? 200 : 0), RouteNotes = $"Demo route {i}.", Notes = $"Demo trip {i}." });
                    }
                }
            }
            await db.SaveChangesAsync();

            await EnsureDemoImagesAsync(db);
        }

        private static async Task EnsureDemoImagesAsync(CattleFarmDbContext db)
        {
            static string Pick(string folder, int index)
            {
                return $"/uploads/demo-hd/{folder}/{folder}-{(index % 12) + 1:00}.jpg";
            }

            static bool MissingOrWrong(string? path, string folder)
            {
                return string.IsNullOrWhiteSpace(path)
                    || (!path.StartsWith($"/uploads/demo-hd/{folder}/", StringComparison.OrdinalIgnoreCase)
                        && !path.StartsWith($"/uploads/{folder}/", StringComparison.OrdinalIgnoreCase));
            }

            var users = await db.Users.OrderBy(u => u.Id).Take(200).ToListAsync();
            for (var i = 0; i < users.Count; i++)
            {
                if (MissingOrWrong(users[i].ProfileImagePath, "avatars"))
                {
                    users[i].ProfileImagePath = Pick("avatars", i);
                }
            }

            var farms = await db.Farms.OrderBy(f => f.Id).Take(200).ToListAsync();
            for (var i = 0; i < farms.Count; i++)
            {
                if (MissingOrWrong(farms[i].ImagePath, "farms"))
                {
                    farms[i].ImagePath = Pick("farms", i);
                }
            }

            var cattle = await db.Cattles.OrderBy(c => c.Id).Take(300).ToListAsync();
            for (var i = 0; i < cattle.Count; i++)
            {
                if (MissingOrWrong(cattle[i].ImagePath, "cattle"))
                {
                    cattle[i].ImagePath = Pick("cattle", i);
                }
            }

            var workers = await db.Workers.OrderBy(w => w.Id).Take(200).ToListAsync();
            for (var i = 0; i < workers.Count; i++)
            {
                if (MissingOrWrong(workers[i].ImagePath, "workers"))
                {
                    workers[i].ImagePath = Pick("workers", i);
                }
            }

            var doctors = await db.Doctors.OrderBy(d => d.Id).Take(200).ToListAsync();
            for (var i = 0; i < doctors.Count; i++)
            {
                if (MissingOrWrong(doctors[i].ImagePath, "doctors"))
                {
                    doctors[i].ImagePath = Pick("doctors", i);
                }
            }

            var products = await db.Products.OrderBy(p => p.Id).Take(200).ToListAsync();
            for (var i = 0; i < products.Count; i++)
            {
                if (MissingOrWrong(products[i].ImagePath, "products"))
                {
                    products[i].ImagePath = Pick("products", i);
                }
            }

            var vehicles = await db.Vehicles.OrderBy(v => v.Id).Take(200).ToListAsync();
            for (var i = 0; i < vehicles.Count; i++)
            {
                if (MissingOrWrong(vehicles[i].ImagePath, "vehicles"))
                {
                    vehicles[i].ImagePath = Pick("vehicles", i);
                }
            }

            var drivers = await db.Drivers.OrderBy(d => d.Id).Take(200).ToListAsync();
            for (var i = 0; i < drivers.Count; i++)
            {
                if (MissingOrWrong(drivers[i].ImagePath, "workers"))
                {
                    drivers[i].ImagePath = Pick("workers", i);
                }
            }

            var appointments = await db.Appointments.OrderBy(a => a.Id).Take(200).ToListAsync();
            for (var i = 0; i < appointments.Count; i++)
            {
                if (MissingOrWrong(appointments[i].EvidenceImagePath, "proofs"))
                {
                    appointments[i].EvidenceImagePath = Pick("proofs", i);
                }
            }

            var tasks = await db.TaskAssignments.OrderBy(t => t.Id).Take(200).ToListAsync();
            for (var i = 0; i < tasks.Count; i++)
            {
                if (MissingOrWrong(tasks[i].ProofImagePath, "proofs"))
                {
                    tasks[i].ProofImagePath = Pick("proofs", i);
                }
            }

            await db.SaveChangesAsync();
        }

        private static async Task<bool> TableExistsAsync(CattleFarmDbContext db, string tableName)
        {
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose)
            {
                await connection.OpenAsync();
            }

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT CASE WHEN OBJECT_ID(@tableName, 'U') IS NULL THEN 0 ELSE 1 END";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@tableName";
                parameter.Value = $"dbo.{tableName}";
                command.Parameters.Add(parameter);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) == 1;
            }
            finally
            {
                if (shouldClose)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
