-- ==========================================
-- CATTLE FARM SYSTEM DATABASE SEED DATA SCRIPT
-- ==========================================
-- This script seeds all 28 database tables with exactly 6 realistic sample records each.
-- Values are modeled in BDT (Bangladeshi Taka), with realistic past dates (last 6 months),
-- authentic Unsplash image assets, and 100% relational integrity.
--
-- Target Database: CattleFarmDB
-- ==========================================

USE [CattleFarmDB];
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ────────── CLEAN UP EXISTING DATA (Reverse Dependency Order) ──────────
PRINT 'Cleaning up existing records...';

DELETE FROM [Trips];
DELETE FROM [TransportRequests];
DELETE FROM [Drivers];
DELETE FROM [Vehicles];
DELETE FROM [MilkProductions];
DELETE FROM [Vaccinations];
DELETE FROM [MedicineRecords];
DELETE FROM [HealthRecords];
DELETE FROM [FeedRecords];
DELETE FROM [Breedings];
DELETE FROM [WorkerAttendances];
DELETE FROM [Appointments];
DELETE FROM [ActivityLogs];
DELETE FROM [AuditLogs];
DELETE FROM [Notifications];
DELETE FROM [Reviews];
DELETE FROM [Revenues];
DELETE FROM [Expenses];
DELETE FROM [Payments];
DELETE FROM [OrderItems];
DELETE FROM [Orders];
DELETE FROM [Products];
DELETE FROM [Doctors];
DELETE FROM [Workers];
DELETE FROM [Cattles];
DELETE FROM [Farms];
DELETE FROM [Subscriptions];
DELETE FROM [Users];

PRINT 'Database cleaned successfully.';
GO

-- ────────── 1. USERS ──────────
PRINT 'Seeding Users...';
SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] (
    [Id], [Username], [FullName], [Email], [PasswordHash], [Role], 
    [PhoneNumber], [ProfileImagePath], [Address], [IsEmailVerified], [IsActive], 
    [FailedLoginCount], [LockedUntil], [TwoFactorEnabled], [TwoFactorSecret], 
    [PreferredLanguage], [SubscriptionType], [SubscriptionExpiry], [IsDeleted], 
    [DeletedAt], [CreatedAt], [UpdatedAt], [LastLoginAt]
) VALUES 
(1, 'farhan_owner', N'Farhan Zawad', 'farhan@gmail.com', '$2a$11$e0MYzXy5c1bd2GgG1Xg1Xe1Xy5c1bd2GgG', 'Owner', '01712-345678', 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&q=80', 'Dhanmondi, Dhaka', 1, 1, 0, NULL, 0, NULL, 'en', 'Enterprise', DATEADD(month, 6, GETUTCDATE()), 0, NULL, DATEADD(month, -5, GETUTCDATE()), NULL, GETUTCDATE()),
(2, 'tariq_owner', N'Tariq Anam', 'tariq@gmail.com', '$2a$11$e0MYzXy5c1bd2GgG1Xg1Xe1Xy5c1bd2GgG', 'Owner', '01823-456789', 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=400&q=80', 'Halishahar, Chattogram', 1, 1, 0, NULL, 0, NULL, 'en', 'Owner', DATEADD(month, 3, GETUTCDATE()), 0, NULL, DATEADD(month, -4, GETUTCDATE()), NULL, GETUTCDATE()),
(3, 'anika_owner', N'Anika Rahman', 'anika@gmail.com', '$2a$11$e0MYzXy5c1bd2GgG1Xg1Xe1Xy5c1bd2GgG', 'Owner', '01934-567890', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=400&q=80', 'Zindabazar, Sylhet', 1, 1, 0, NULL, 0, NULL, 'en', 'Enterprise', DATEADD(month, 12, GETUTCDATE()), 0, NULL, DATEADD(month, -6, GETUTCDATE()), NULL, GETUTCDATE()),
(4, 'milon_worker', N'Milon Miah', 'milon@gmail.com', '$2a$11$e0MYzXy5c1bd2GgG1Xg1Xe1Xy5c1bd2GgG', 'Worker', '01545-678901', 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=400&q=80', 'Savar, Dhaka', 1, 1, 0, NULL, 0, NULL, 'bn', NULL, NULL, 0, NULL, DATEADD(month, -3, GETUTCDATE()), NULL, GETUTCDATE()),
(5, 'shafiq_vet', N'Dr. Shafiqul Islam', 'shafiq@gmail.com', '$2a$11$e0MYzXy5c1bd2GgG1Xg1Xe1Xy5c1bd2GgG', 'Doctor', '01656-789012', 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=400&q=80', 'Uttara, Dhaka', 1, 1, 0, NULL, 0, NULL, 'en', NULL, NULL, 0, NULL, DATEADD(month, -4, GETUTCDATE()), NULL, GETUTCDATE()),
(6, 'rahim_customer', N'Abdur Rahim', 'rahim@gmail.com', '$2a$11$e0MYzXy5c1bd2GgG1Xg1Xe1Xy5c1bd2GgG', 'User', '01767-890123', 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=400&q=80', 'Banani, Dhaka', 1, 1, 0, NULL, 0, NULL, 'en', NULL, NULL, 0, NULL, DATEADD(month, -2, GETUTCDATE()), NULL, GETUTCDATE());
SET IDENTITY_INSERT [Users] OFF;
GO

-- ────────── 2. SUBSCRIPTIONS ──────────
PRINT 'Seeding Subscriptions...';
SET IDENTITY_INSERT [Subscriptions] ON;
INSERT INTO [Subscriptions] (
    [Id], [Plan], [StartDate], [ExpiryDate], [IsActive], [AutoRenew], 
    [PricePaid], [TransactionRef], [CreatedAt], [UpdatedAt], [UserId]
) VALUES 
(1, 3, DATEADD(month, -6, GETUTCDATE()), DATEADD(month, 6, GETUTCDATE()), 1, 1, 50000.00, 'TXN-SUB-001', DATEADD(month, -6, GETUTCDATE()), GETUTCDATE(), 1),
(2, 2, DATEADD(month, -3, GETUTCDATE()), DATEADD(month, 9, GETUTCDATE()), 1, 1, 20000.00, 'TXN-SUB-002', DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 2),
(3, 3, DATEADD(month, -6, GETUTCDATE()), DATEADD(month, 6, GETUTCDATE()), 1, 1, 50000.00, 'TXN-SUB-003', DATEADD(month, -6, GETUTCDATE()), GETUTCDATE(), 3),
(4, 0, DATEADD(month, -2, GETUTCDATE()), DATEADD(month, 10, GETUTCDATE()), 1, 0, 0.00, NULL, DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 4),
(5, 1, DATEADD(month, -1, GETUTCDATE()), DATEADD(month, 11, GETUTCDATE()), 1, 1, 5000.00, 'TXN-SUB-005', DATEADD(month, -1, GETUTCDATE()), GETUTCDATE(), 5),
(6, 1, DATEADD(month, -2, GETUTCDATE()), DATEADD(month, 4, GETUTCDATE()), 1, 0, 5000.00, 'TXN-SUB-006', DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 6);
SET IDENTITY_INSERT [Subscriptions] OFF;
GO

-- ────────── 3. FARMS ──────────
PRINT 'Seeding Farms...';
SET IDENTITY_INSERT [Farms] ON;
INSERT INTO [Farms] (
    [Id], [Name], [Location], [FarmType], [SizeInAcres], [Capacity], 
    [Description], [ImagePath], [Latitude], [Longitude], [ApprovalStatus], 
    [IsActive], [IsDeleted], [DeletedAt], [CreatedAt], [UpdatedAt], [OwnerId]
) VALUES 
(1, N'বরকত ফার্ম', 'Savar, Dhaka', 0, 15.5, 120, 'Premium Dairy Farm in Savar, supplying fresh organic milk across Dhaka.', 'https://images.unsplash.com/photo-1500937386664-56d15943747d?w=400&q=80', 23.8583, 90.2667, 1, 1, 0, NULL, DATEADD(month, -5, GETUTCDATE()), GETUTCDATE(), 1),
(2, N'সোনালী এগ্রো', 'Hathazari, Chattogram', 1, 20.0, 150, 'High-grade beef fattening farm, specializing in healthy Brahman & Shahiwal cows.', 'https://images.unsplash.com/photo-1516467508483-a7212febe31a?w=400&q=80', 22.5085, 91.8083, 1, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 1),
(3, N'গ্রীন ভ্যালি ফার্ম', 'Sreemangal, Sylhet', 2, 35.0, 200, 'Integrated crop-livestock pasture farm producing high yield dairy and organic compost.', 'https://images.unsplash.com/photo-1574323347407-f5e1ad6d020b?w=400&q=80', 24.3083, 91.7333, 1, 1, 0, NULL, DATEADD(month, -6, GETUTCDATE()), GETUTCDATE(), 2),
(4, N'নদীর পাড় ফার্ম', 'Paba, Rajshahi', 1, 12.0, 80, 'Beef breed ranch along the Padma riverbed, experiencing temporary transport overheads.', 'https://images.unsplash.com/photo-1500595046783-cd211893c574?w=400&q=80', 24.3733, 88.6049, 1, 1, 0, NULL, DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 2),
(5, N'সূর্যমুখী ফার্ম', 'Trishal, Mymensingh', 3, 18.2, 100, 'State of the art cattle breeding farm focusing on artificial insemination and high genetics.', 'https://images.unsplash.com/photo-1599381412411-d104e7baf04e?w=400&q=80', 24.5167, 90.3958, 1, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 3),
(6, N'মেঘনা এগ্রো', 'Daudkandi, Comilla', 0, 10.0, 70, 'Local milk collection center and dairy farm with top quality Holstein Friesians.', 'https://images.unsplash.com/photo-1527689368864-3a821dbccc34?w=400&q=80', 23.5333, 90.7167, 1, 1, 0, NULL, DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 3);
SET IDENTITY_INSERT [Farms] OFF;
GO

-- ────────── 4. CATTLES ──────────
PRINT 'Seeding Cattles...';
SET IDENTITY_INSERT [Cattles] ON;
INSERT INTO [Cattles] (
    [Id], [TagId], [Name], [Breed], [DateOfBirth], [Weight], [Gender], 
    [HealthStatus], [Status], [ImagePath], [PurchasePrice], [SalePrice], 
    [SaleDate], [PurchaseDate], [Description], [IsListedForSale], [IsPremiumListing], 
    [ApprovalStatus], [IsDeleted], [DeletedAt], [CreatedAt], [UpdatedAt], [FarmId]
) VALUES 
(1, 'TAG-SAV-101', 'Lal Pagla', 'Shahiwal', DATEADD(month, -36, GETUTCDATE()), 420.5, 0, 0, 0, 'https://images.unsplash.com/photo-1570042225831-d98fa7577f1e?w=400&q=80', 180000.00, NULL, NULL, DATEADD(month, -5, GETUTCDATE()), 'Magnificent Shahiwal bull, robust build, highly responsive.', 0, 0, 1, 0, NULL, DATEADD(month, -5, GETUTCDATE()), GETUTCDATE(), 1),
(2, 'TAG-SAV-102', 'Dhabali', 'Friesian', DATEADD(month, -28, GETUTCDATE()), 550.0, 1, 0, 0, 'https://images.unsplash.com/photo-1563559170765-a041f974867e?w=400&q=80', 250000.00, NULL, NULL, DATEADD(month, -5, GETUTCDATE()), 'Holstein Friesian cow producing average 25 Liters of milk daily.', 0, 0, 1, 0, NULL, DATEADD(month, -5, GETUTCDATE()), GETUTCDATE(), 1),
(3, 'TAG-CTG-201', 'Bahadur', 'Brahman', DATEADD(month, -40, GETUTCDATE()), 680.0, 0, 0, 0, 'https://images.unsplash.com/photo-1524178232363-1fb2b075b655?w=400&q=80', 320000.00, NULL, NULL, DATEADD(month, -4, GETUTCDATE()), 'Heavyweight Brahman bull, perfect for breeding and Qurbani listing.', 1, 1, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 2),
(4, 'TAG-SYL-301', 'Rani', 'Jersey', DATEADD(month, -24, GETUTCDATE()), 380.0, 1, 0, 0, 'https://images.unsplash.com/photo-1484557985045-edf25e08da73?w=400&q=80', 160000.00, NULL, NULL, DATEADD(month, -6, GETUTCDATE()), 'High fat-yield Jersey heifer, calm temperament.', 0, 0, 1, 0, NULL, DATEADD(month, -6, GETUTCDATE()), GETUTCDATE(), 3),
(5, 'TAG-RAJ-401', 'Kalo Manik', 'Gir', DATEADD(month, -30, GETUTCDATE()), 460.0, 0, 2, 0, 'https://images.unsplash.com/photo-1596733430284-f7437764b1a9?w=400&q=80', 175000.00, NULL, NULL, DATEADD(month, -3, GETUTCDATE()), 'Gir steer, under active veterinary monitoring for mild hoof infection.', 0, 0, 1, 0, NULL, DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 4),
(6, 'TAG-MYM-501', 'Sunila', 'Red Chittagong', DATEADD(month, -18, GETUTCDATE()), 310.0, 1, 0, 0, 'https://images.unsplash.com/photo-1545468241-11d2e1c95353?w=400&q=80', 110000.00, NULL, NULL, DATEADD(month, -4, GETUTCDATE()), 'Indigenous breed heifer, extremely disease-resistant.', 1, 0, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 5);
SET IDENTITY_INSERT [Cattles] OFF;
GO

-- ────────── 5. WORKERS ──────────
PRINT 'Seeding Workers...';
SET IDENTITY_INSERT [Workers] ON;
INSERT INTO [Workers] (
    [Id], [FullName], [Role], [Phone], [Email], [Skills], [ExperienceYears], 
    [Salary], [ImagePath], [IsAvailable], [IsActive], [IsDeleted], [DeletedAt], 
    [HiredAt], [CreatedAt], [UpdatedAt], [Notes], [FarmId], [UserId]
) VALUES 
(1, 'Milon Miah', 'Lead Milker', '01733-445566', 'milon@gmail.com', 'Machine milking, cattle cleaning, feed preparation', 5, 18000.00, 'https://images.unsplash.com/photo-1560250097-0b93528c311a?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -3, GETUTCDATE()), DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 'Highly dedicated lead worker.', 1, 4),
(2, 'Rafiq Ahmed', 'Cattle Feeder', '01844-556677', 'rafiq@gmail.com', 'Diet scheduling, feed quality control', 3, 16000.00, 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 'Very punctual worker.', 2, NULL),
(3, 'Selim Chowdhury', 'General Herdsman', '01955-667788', 'selim@gmail.com', 'Calving assistance, behavior observation', 6, 20000.00, 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -5, GETUTCDATE()), DATEADD(month, -5, GETUTCDATE()), GETUTCDATE(), 'Expert in large herd management.', 3, NULL),
(4, 'Kalam Sheikh', 'Cattle Groomer', '01566-778899', 'kalam@gmail.com', 'Hoof trimming, cattle washing, stall cleaning', 4, 15000.00, 'https://images.unsplash.com/photo-1508214751196-bcfd4ca60f91?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -3, GETUTCDATE()), DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 'Works diligently.', 4, NULL),
(5, 'Jahangir Alam', 'Assistant Breeder', '01677-889900', 'jahangir@gmail.com', 'Heat detection, artificial insemination helper', 2, 17000.00, 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 'Enthusiastic and eager to learn.', 5, NULL),
(6, 'Abul Hossain', 'Security Guard', '01788-990011', 'abul@gmail.com', 'Night watch, visitor logging, facility patrolling', 7, 14000.00, 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -2, GETUTCDATE()), DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 'Responsible for farm security.', 6, NULL);
SET IDENTITY_INSERT [Workers] OFF;
GO

-- ────────── 6. DOCTORS ──────────
PRINT 'Seeding Doctors...';
SET IDENTITY_INSERT [Doctors] ON;
INSERT INTO [Doctors] (
    [Id], [FullName], [Specialization], [Phone], [Email], [LicenseNumber], 
    [ConsultationFee], [ImagePath], [IsAvailable], [IsActive], [IsDeleted], 
    [DeletedAt], [CreatedAt], [UpdatedAt], [Notes], [FarmId], [UserId]
) VALUES 
(1, 'Dr. Shafiqul Islam', 'Cattle Breeding & IVF', '01711-223344', 'shafiq@gmail.com', 'DVM-BD-5043', 2500.00, 'https://images.unsplash.com/photo-1537368910025-700350fe46c7?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 'Chief Vet Consultant for Savar area.', 1, 5),
(2, 'Dr. Latifur Rahman', 'Large Animal Surgery', '01822-334455', 'latif@gmail.com', 'DVM-BD-3920', 2000.00, 'https://images.unsplash.com/photo-1612349317150-e413f6a5b16d?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -5, GETUTCDATE()), GETUTCDATE(), 'Expert surgical consultant in Chittagong region.', 2, NULL),
(3, 'Dr. Tanzina Akhter', 'Dairy Herd Immunology', '01933-445566', 'tanzina@gmail.com', 'DVM-BD-6022', 2200.00, 'https://images.unsplash.com/photo-1594824813573-246434de83fb?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -6, GETUTCDATE()), GETUTCDATE(), 'Regular vet inspector for Sylhet tea valley farms.', 3, NULL),
(4, 'Dr. Mahbubul Alam', 'Ruminant Nutrition', '01544-556677', 'mahbub@gmail.com', 'DVM-BD-4122', 1500.00, 'https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -3, GETUTCDATE()), GETUTCDATE(), 'Formulates custom high-yield diet charts.', 4, NULL),
(5, 'Dr. Nazmul Hasan', 'Infectious Disease Control', '01655-667788', 'nazmul@gmail.com', 'DVM-BD-5390', 1800.00, 'https://images.unsplash.com/photo-1559839734-2b71ea197ec2?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 'Specialist in FMD prevention schedules.', 5, NULL),
(6, 'Dr. Farhana Yasmin', 'Ultrasonography & Breeding', '01766-778899', 'farhana@gmail.com', 'DVM-BD-6101', 2000.00, 'https://images.unsplash.com/photo-1614608682850-e0d6ed316d47?w=400&q=80', 1, 1, 0, NULL, DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 'Performs early pregnancy detection checks.', 6, NULL);
SET IDENTITY_INSERT [Doctors] OFF;
GO

-- ────────── 7. PRODUCTS ──────────
PRINT 'Seeding Products...';
SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] (
    [Id], [Name], [Category], [Description], [Price], [StockQuantity], 
    [Unit], [MinStockLevel], [ImagePath], [IsAvailable], [IsDeleted], 
    [DeletedAt], [CreatedAt], [UpdatedAt], [IsFeatured], [FarmId]
) VALUES 
(1, N'খাটি তরল দুধ (Fresh Raw Milk)', 0, 'Pure whole milk sourced daily from our grass-fed Friesian herd.', 90.00, 500.0, 'Liters', 50.0, 'https://images.unsplash.com/photo-1550583724-b2692b85b150?w=400&q=80', 1, 0, NULL, DATEADD(month, -5, GETUTCDATE()), GETUTCDATE(), 1, 1),
(2, N'প্রিমিয়াম গরুর মাংস (Premium Beef)', 1, 'Naturally fattened premium tender beef, slaughtered and processed under strict hygienic conditions.', 750.00, 100.0, 'Kg', 10.0, 'https://images.unsplash.com/photo-1544025162-d76694265947?w=400&q=80', 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 1, 2),
(3, N'জৈব সার (Organic Compost Manure)', 2, 'Rich organic compost fertilizer processed from cow dung, perfect for agricultural use.', 15.00, 2000.0, 'Kg', 200.0, 'https://images.unsplash.com/photo-1592417817098-8f3d6eb19675?w=400&q=80', 1, 0, NULL, DATEADD(month, -6, GETUTCDATE()), GETUTCDATE(), 0, 3),
(4, N'হোলস্টাইন প্রজনন সেবা (Holstein Breeding Service)', 3, 'High pedigree Holstein Friesian bull straw breeding service for local farmers.', 3500.00, 50.0, 'Service', 5.0, 'https://images.unsplash.com/photo-1527153857715-3908f2bac5e8?w=400&q=80', 1, 0, NULL, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE(), 0, 4),
(5, N'খাঁটি গাওয়া ঘি (Pure Cow Ghee)', 4, 'Traditional hand-churned cow milk ghee, highly aromatic and tasty.', 1400.00, 80.0, 'Kg', 10.0, 'https://images.unsplash.com/photo-1628191139360-408a56e2e216?w=400&q=80', 1, 0, NULL, DATEADD(month, -5, GETUTCDATE()), GETUTCDATE(), 1, 5),
(6, N'পাস্তুরিত বোতলজাত দুধ (Pasteurized Bottled Milk)', 0, 'Clean pasteurized cow milk bottled in glass jars for direct consumption.', 110.00, 300.0, 'Liters', 30.0, 'https://images.unsplash.com/photo-1563636619-e9143da7973b?w=400&q=80', 1, 0, NULL, DATEADD(month, -2, GETUTCDATE()), GETUTCDATE(), 0, 6);
SET IDENTITY_INSERT [Products] OFF;
GO

-- ────────── 8. ORDERS ──────────
PRINT 'Seeding Orders...';
SET IDENTITY_INSERT [Orders] ON;
INSERT INTO [Orders] (
    [Id], [OrderStatus], [PaymentStatus], [TotalAmount], [DeliveryAddress], 
    [Notes], [OrderDate], [DeliveredAt], [CreatedAt], [UpdatedAt], 
    [CustomerId], [FarmId]
) VALUES 
(1, 4, 1, 45000.00, 'Banani, Dhaka', 'Deliver in morning hours please.', DATEADD(day, -10, GETUTCDATE()), DATEADD(day, -9, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE()), GETUTCDATE(), 6, 1),
(2, 4, 1, 75000.00, 'Gulshan, Dhaka', 'Urgent delivery of beef supply.', DATEADD(day, -15, GETUTCDATE()), DATEADD(day, -14, GETUTCDATE()), DATEADD(day, -15, GETUTCDATE()), GETUTCDATE(), 6, 2),
(3, 4, 1, 30000.00, 'Sylhet Town', 'Bulk compost order.', DATEADD(day, -20, GETUTCDATE()), DATEADD(day, -19, GETUTCDATE()), DATEADD(day, -20, GETUTCDATE()), GETUTCDATE(), 6, 3),
(4, 1, 0, 10500.00, 'Rajshahi Cantt', 'Pending shipping cost settlement.', DATEADD(day, -2, GETUTCDATE()), NULL, DATEADD(day, -2, GETUTCDATE()), GETUTCDATE(), 6, 4),
(5, 4, 1, 10500.00, 'Mymensingh Sadar', 'Breeding straw package delivery.', DATEADD(day, -8, GETUTCDATE()), DATEADD(day, -7, GETUTCDATE()), DATEADD(day, -8, GETUTCDATE()), GETUTCDATE(), 6, 5),
(6, 0, 0, 5500.00, 'Comilla Cantt', 'Newly placed milk order.', DATEADD(day, -1, GETUTCDATE()), NULL, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE(), 6, 6);
SET IDENTITY_INSERT [Orders] OFF;
GO

-- ────────── 9. ORDERITEMS ──────────
PRINT 'Seeding OrderItems...';
SET IDENTITY_INSERT [OrderItems] ON;
INSERT INTO [OrderItems] (
    [Id], [Quantity], [UnitPrice], [TotalPrice], [OrderId], [ProductId]
) VALUES 
(1, 500.0, 90.00, 45000.00, 1, 1),
(2, 100.0, 750.00, 75000.00, 2, 2),
(3, 2000.0, 15.00, 30000.00, 3, 3),
(4, 3.0, 3500.00, 10500.00, 4, 4),
(5, 3.0, 3500.00, 10500.00, 5, 5),
(6, 50.0, 110.00, 5500.00, 6, 6);
SET IDENTITY_INSERT [OrderItems] OFF;
GO

-- ────────── 10. PAYMENTS ──────────
PRINT 'Seeding Payments...';
SET IDENTITY_INSERT [Payments] ON;
INSERT INTO [Payments] (
    [Id], [Amount], [Method], [Status], [Purpose], [TransactionId], 
    [ReferenceId], [ReferenceType], [Notes], [PaymentDate], [CreatedAt], 
    [UserId], [OrderId]
) VALUES 
(1, 45000.00, 2, 1, 2, 'TXN-PAY-001', 1, 'Order', 'bKash Payment for Order 1', DATEADD(day, -10, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE()), 6, 1),
(2, 75000.00, 3, 1, 2, 'TXN-PAY-002', 2, 'Order', 'Nagad Payment for Order 2', DATEADD(day, -15, GETUTCDATE()), DATEADD(day, -15, GETUTCDATE()), 6, 2),
(3, 30000.00, 1, 1, 2, 'TXN-PAY-003', 3, 'Order', 'Bank Transfer for Order 3', DATEADD(day, -20, GETUTCDATE()), DATEADD(day, -20, GETUTCDATE()), 6, 3),
(4, 50000.00, 4, 1, 0, 'TXN-PAY-004', 1, 'Subscription', 'Visa Card Payment for Enterprise Plan', DATEADD(month, -6, GETUTCDATE()), DATEADD(month, -6, GETUTCDATE()), 1, NULL),
(5, 10500.00, 2, 1, 2, 'TXN-PAY-005', 5, 'Order', 'bKash Payment for Order 5', DATEADD(day, -8, GETUTCDATE()), DATEADD(day, -8, GETUTCDATE()), 6, 5),
(6, 20000.00, 5, 1, 0, 'TXN-PAY-006', 2, 'Subscription', 'Mastercard Payment for Owner Plan', DATEADD(month, -3, GETUTCDATE()), DATEADD(month, -3, GETUTCDATE()), 2, NULL);
SET IDENTITY_INSERT [Payments] OFF;
GO

-- ────────── 11. EXPENSES ──────────
PRINT 'Seeding Expenses...';
SET IDENTITY_INSERT [Expenses] ON;
INSERT INTO [Expenses] (
    [Id], [Category], [Amount], [Date], [Description], [ReceiptImagePath], 
    [IsApproved], [IsDeleted], [DeletedAt], [CreatedAt], [FarmId], [CreatedByUserId]
) VALUES 
(1, 0, 80000.00, DATEADD(day, -30, GETUTCDATE()), 'Feed purchase of 4000kg Concentrate.', 'https://images.unsplash.com/photo-1554415707-6e8cfc93fe23?w=400&q=80', 1, 0, NULL, DATEADD(day, -30, GETUTCDATE()), 1, 1),
(2, 2, 250000.00, DATEADD(day, -15, GETUTCDATE()), 'Monthly labor wages for 10 hands.', 'https://images.unsplash.com/photo-1454165804606-c3d57bc86b40?w=400&q=80', 1, 0, NULL, DATEADD(day, -15, GETUTCDATE()), 2, 1),
(3, 6, 100000.00, DATEADD(day, -45, GETUTCDATE()), 'Herd vaccination and veterinary services billing.', 'https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=400&q=80', 1, 0, NULL, DATEADD(day, -45, GETUTCDATE()), 3, 2),
(4, 3, 300000.00, DATEADD(day, -10, GETUTCDATE()), 'Purchased premium automated milking equipment.', 'https://images.unsplash.com/photo-1530124560672-99992f164f3d?w=400&q=80', 1, 0, NULL, DATEADD(day, -10, GETUTCDATE()), 4, 2),
(5, 0, 250000.00, DATEADD(day, -20, GETUTCDATE()), 'Imported high-protein feed supplements.', 'https://images.unsplash.com/photo-1554415707-6e8cfc93fe23?w=400&q=80', 1, 0, NULL, DATEADD(day, -20, GETUTCDATE()), 5, 3),
(6, 4, 20000.00, DATEADD(day, -5, GETUTCDATE()), 'Electricity and water bills.', 'https://images.unsplash.com/photo-1590086782792-42dd2350140d?w=400&q=80', 1, 0, NULL, DATEADD(day, -5, GETUTCDATE()), 6, 3);
SET IDENTITY_INSERT [Expenses] OFF;
GO

-- ────────── 12. REVENUES ──────────
PRINT 'Seeding Revenues...';
SET IDENTITY_INSERT [Revenues] ON;
INSERT INTO [Revenues] (
    [Id], [Source], [Amount], [Date], [Description], [IsDeleted], 
    [DeletedAt], [CreatedAt], [FarmId], [OrderId], [CreatedByUserId]
) VALUES 
(1, 0, 500000.00, DATEADD(day, -25, GETUTCDATE()), 'Bulk raw milk sales to local sweetshops.', 0, NULL, DATEADD(day, -25, GETUTCDATE()), 1, NULL, 1),
(2, 1, 800000.00, DATEADD(day, -12, GETUTCDATE()), 'Sold 3 Brahman steers for Eid Qurbani.', 0, NULL, DATEADD(day, -12, GETUTCDATE()), 2, NULL, 1),
(3, 3, 350000.00, DATEADD(day, -40, GETUTCDATE()), 'Commercial organic compost fertilizer supply contract.', 0, NULL, DATEADD(day, -40, GETUTCDATE()), 3, NULL, 2),
(4, 1, 150000.00, DATEADD(day, -8, GETUTCDATE()), 'Emergency sale of 1 older steer.', 0, NULL, DATEADD(day, -8, GETUTCDATE()), 4, NULL, 2),
(5, 2, 100000.00, DATEADD(day, -18, GETUTCDATE()), 'Breeding counseling and insemination straw distribution.', 0, NULL, DATEADD(day, -18, GETUTCDATE()), 5, NULL, 3),
(6, 0, 35000.00, DATEADD(day, -4, GETUTCDATE()), 'Direct consumer milk sales.', 0, NULL, DATEADD(day, -4, GETUTCDATE()), 6, NULL, 3);
SET IDENTITY_INSERT [Revenues] OFF;
GO

-- ────────── 13. REVIEWS ──────────
PRINT 'Seeding Reviews...';
SET IDENTITY_INSERT [Reviews] ON;
INSERT INTO [Reviews] (
    [Id], [TargetType], [TargetId], [Rating], [Comment], [IsApproved], 
    [IsDeleted], [DeletedAt], [CreatedAt], [ReviewerId], [CattleId], 
    [FarmId], [ProductId]
) VALUES 
(1, 0, 1, 5, 'Absolutely top-notch dairy quality. Always fresh!', 1, 0, NULL, DATEADD(day, -15, GETUTCDATE()), 6, NULL, 1, NULL),
(2, 1, 3, 5, 'Highly energetic and healthy Brahman bull. Extremely happy with the breed quality.', 1, 0, NULL, DATEADD(day, -10, GETUTCDATE()), 6, 3, NULL, NULL),
(3, 4, 2, 5, 'Super tender and high-quality beef. Highly recommended!', 1, 0, NULL, DATEADD(day, -12, GETUTCDATE()), 6, NULL, NULL, 2),
(4, 0, 2, 4, 'Very professional beef farm. Punctual communications.', 1, 0, NULL, DATEADD(day, -20, GETUTCDATE()), 6, NULL, 2, NULL),
(5, 4, 3, 5, 'Best organic compost in the market. Flowers are blooming beautifully.', 1, 0, NULL, DATEADD(day, -25, GETUTCDATE()), 6, NULL, NULL, 3),
(6, 4, 1, 5, 'Sweetest milk, perfectly clean packaging.', 1, 0, NULL, DATEADD(day, -5, GETUTCDATE()), 6, NULL, NULL, 1);
SET IDENTITY_INSERT [Reviews] OFF;
GO

-- ────────── 14. NOTIFICATIONS ──────────
PRINT 'Seeding Notifications...';
SET IDENTITY_INSERT [Notifications] ON;
INSERT INTO [Notifications] (
    [Id], [Title], [Message], [Type], [IsRead], [ReadAt], [CreatedAt], 
    [RelatedEntityType], [RelatedEntityId], [UserId]
) VALUES 
(1, 'Vaccination Reminder', 'Foot and mouth vaccination due for Cattle 2 next week.', 0, 0, NULL, DATEADD(day, -2, GETUTCDATE()), 'Cattle', 2, 1),
(2, 'Subscription Activated', 'Welcome to smart enterprise plan!', 4, 1, DATEADD(day, -3, GETUTCDATE()), DATEADD(day, -3, GETUTCDATE()), 'Subscription', 1, 1),
(3, 'New Order Placed', 'Customer Rahim has ordered bulk milk.', 2, 0, NULL, DATEADD(day, -1, GETUTCDATE()), 'Order', 1, 1),
(4, 'Cattle Health Alert', 'Cattle 5 (Kalo Manik) reported sick with hoof infection.', 1, 0, NULL, DATEADD(day, -3, GETUTCDATE()), 'Cattle', 5, 2),
(5, 'Appointment Scheduled', 'Dr. Tanzina scheduled vet visit for Rani.', 6, 1, DATEADD(day, -5, GETUTCDATE()), DATEADD(day, -6, GETUTCDATE()), 'Appointment', 3, 3),
(6, 'Payment Received', 'Payment of ৳30,000 for order 3 completed.', 3, 1, DATEADD(day, -20, GETUTCDATE()), DATEADD(day, -20, GETUTCDATE()), 'Payment', 3, 3);
SET IDENTITY_INSERT [Notifications] OFF;
GO

-- ────────── 15. AUDITLOGS ──────────
PRINT 'Seeding AuditLogs...';
SET IDENTITY_INSERT [AuditLogs] ON;
INSERT INTO [AuditLogs] (
    [Id], [Action], [EntityName], [EntityId], [OldValues], [NewValues], 
    [IPAddress], [UserAgent], [Timestamp], [UserId]
) VALUES 
(1, 'INSERT', 'User', 1, NULL, '{"Username":"farhan_owner"}', '192.168.1.10', 'Mozilla/5.0 Chrome/120.0', DATEADD(month, -5, GETUTCDATE()), 1),
(2, 'INSERT', 'Farm', 1, NULL, '{"Name":"Savar Farm"}', '192.168.1.10', 'Mozilla/5.0 Chrome/120.0', DATEADD(month, -5, GETUTCDATE()), 1),
(3, 'UPDATE', 'Cattle', 2, '{"Weight":540}', '{"Weight":550}', '192.168.1.11', 'Mozilla/5.0 Chrome/120.0', DATEADD(day, -5, GETUTCDATE()), 1),
(4, 'INSERT', 'User', 2, NULL, '{"Username":"tariq_owner"}', '192.168.2.15', 'Mozilla/5.0 Edge/120.0', DATEADD(month, -4, GETUTCDATE()), 2),
(5, 'INSERT', 'Farm', 3, NULL, '{"Name":"Green Valley"}', '192.168.2.16', 'Mozilla/5.0 Edge/120.0', DATEADD(month, -6, GETUTCDATE()), 2),
(6, 'UPDATE', 'Worker', 1, '{"Salary":17000}', '{"Salary":18000}', '192.168.1.10', 'Mozilla/5.0 Chrome/120.0', DATEADD(day, -10, GETUTCDATE()), 1);
SET IDENTITY_INSERT [AuditLogs] OFF;
GO

-- ────────── 16. ACTIVITYLOGS ──────────
PRINT 'Seeding ActivityLogs...';
SET IDENTITY_INSERT [ActivityLogs] ON;
INSERT INTO [ActivityLogs] (
    [Id], [Description], [EntityName], [EntityId], [IPAddress], [Timestamp], [UserId]
) VALUES 
(1, 'Logged into the system', 'User', 1, '192.168.1.10', DATEADD(hour, -2, GETUTCDATE()), 1),
(2, 'Added a new Cattle: Lal Pagla', 'Cattle', 1, '192.168.1.10', DATEADD(month, -5, GETUTCDATE()), 1),
(3, 'Updated health status for Cattle 5 to Sick', 'Cattle', 5, '192.168.2.15', DATEADD(day, -3, GETUTCDATE()), 2),
(4, 'Registered a new farm worker: Milon Miah', 'Worker', 1, '192.168.1.10', DATEADD(month, -3, GETUTCDATE()), 1),
(5, 'Scheduled appointment with Dr. Tanzina', 'Appointment', 3, '192.168.3.10', DATEADD(day, -6, GETUTCDATE()), 3),
(6, 'Logged into the system', 'User', 2, '192.168.2.15', DATEADD(hour, -1, GETUTCDATE()), 2);
SET IDENTITY_INSERT [ActivityLogs] OFF;
GO

-- ────────── 17. WORKERATTENDANCES ──────────
PRINT 'Seeding WorkerAttendances...';
SET IDENTITY_INSERT [WorkerAttendances] ON;
INSERT INTO [WorkerAttendances] (
    [Id], [Date], [CheckIn], [CheckOut], [Status], [HoursWorked], 
    [Notes], [CreatedAt], [WorkerId]
) VALUES 
(1, DATEADD(day, -1, GETUTCDATE()), DATEADD(hour, -9, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE()), 0, 8.0, 'Punctual entry and exit.', DATEADD(day, -1, GETUTCDATE()), 1),
(2, DATEADD(day, -2, GETUTCDATE()), DATEADD(hour, -9, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE()), 0, 8.0, 'Performed standard milking.', DATEADD(day, -2, GETUTCDATE()), 1),
(3, DATEADD(day, -1, GETUTCDATE()), DATEADD(hour, -9, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE()), 0, 8.0, 'Daily feeding completed successfully.', DATEADD(day, -1, GETUTCDATE()), 2),
(4, DATEADD(day, -2, GETUTCDATE()), DATEADD(hour, -9, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE()), 0, 8.0, 'Checked all diet charts.', DATEADD(day, -2, GETUTCDATE()), 2),
(5, DATEADD(day, -1, GETUTCDATE()), DATEADD(hour, -8, GETUTCDATE()), DATEADD(hour, -4, GETUTCDATE()), 2, 4.0, 'Left early for doctor appointment.', DATEADD(day, -1, GETUTCDATE()), 3),
(6, DATEADD(day, -2, GETUTCDATE()), DATEADD(hour, -9, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE()), 0, 8.0, 'Attended calving birth.', DATEADD(day, -2, GETUTCDATE()), 3);
SET IDENTITY_INSERT [WorkerAttendances] OFF;
GO

-- ────────── 18. APPOINTMENTS ──────────
PRINT 'Seeding Appointments...';
SET IDENTITY_INSERT [Appointments] ON;
INSERT INTO [Appointments] (
    [Id], [ScheduledAt], [Status], [Reason], [Notes], [CompletedAt], 
    [CreatedAt], [UpdatedAt], [CattleId], [DoctorId], [FarmId], [CreatedByUserId]
) VALUES 
(1, DATEADD(day, -5, GETUTCDATE()), 1, 'Routine Pregnancy Scan', 'Cattle is pregnant 3 months, healthy fetus.', DATEADD(day, -5, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE()), GETUTCDATE(), 2, 6, 1, 1),
(2, DATEADD(day, -3, GETUTCDATE()), 1, 'Hoof Infection Treatment', 'Cleaned wound, applied bandage, prescribed antibiotics.', DATEADD(day, -3, GETUTCDATE()), DATEADD(day, -4, GETUTCDATE()), GETUTCDATE(), 5, 4, 4, 2),
(3, DATEADD(day, 2, GETUTCDATE()), 0, 'General Herd Health Assessment', 'Inspect all active heifers.', NULL, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE(), 4, 3, 3, 2),
(4, DATEADD(day, -12, GETUTCDATE()), 1, 'Sire Breeding Assessment', 'Checked bull sperm motility, approved for collection.', DATEADD(day, -12, GETUTCDATE()), DATEADD(day, -15, GETUTCDATE()), GETUTCDATE(), 3, 1, 2, 1),
(5, DATEADD(day, 5, GETUTCDATE()), 0, 'First De-worming Dose', 'Due for young indigenous heifer.', NULL, DATEADD(day, -2, GETUTCDATE()), GETUTCDATE(), 6, 5, 5, 3),
(6, DATEADD(day, -2, GETUTCDATE()), 2, 'Urgent Milk Drop Analysis', 'Cancelled due to vet emergency conflict.', NULL, DATEADD(day, -3, GETUTCDATE()), GETUTCDATE(), 2, 3, 1, 1);
SET IDENTITY_INSERT [Appointments] OFF;
GO

-- ────────── 19. BREEDINGS ──────────
PRINT 'Seeding Breedings...';
SET IDENTITY_INSERT [Breedings] ON;
INSERT INTO [Breedings] (
    [Id], [CattleId], [SireId], [FarmId], [BreedingDate], [ExpectedCalvingDate], 
    [ActualCalvingDate], [Method], [Outcome], [CalvesCount], [SireBreed], 
    [InseminationTechnician], [Cost], [Notes], [CreatedAt]
) VALUES 
(1, 2, 1, 1, DATEADD(month, -9, GETUTCDATE()), DATEADD(month, 0, GETUTCDATE()), GETUTCDATE(), 1, 1, 1, 'Shahiwal', 'Dr. Shafiqul Islam', 3000.00, 'Calved healthy female calf successfully.', DATEADD(month, -9, GETUTCDATE())),
(2, 4, NULL, 3, DATEADD(month, -5, GETUTCDATE()), DATEADD(month, 4, GETUTCDATE()), NULL, 1, 0, NULL, 'Friesian (Straw ID: F-92)', 'Dr. Tanzina Akhter', 2500.00, 'Pregnancy confirmed via ultrasound scan.', DATEADD(month, -5, GETUTCDATE())),
(3, 6, NULL, 5, DATEADD(month, -2, GETUTCDATE()), DATEADD(month, 7, GETUTCDATE()), NULL, 1, 0, NULL, 'Jersey (Straw ID: J-12)', 'Dr. Farhana Yasmin', 2000.00, 'Awaiting 60-day ultrasound pregnancy scan.', DATEADD(month, -2, GETUTCDATE())),
(4, 2, 1, 1, DATEADD(month, -18, GETUTCDATE()), DATEADD(month, -9, GETUTCDATE()), DATEADD(month, -9, GETUTCDATE()), 0, 1, 1, 'Shahiwal', 'Milon Miah', 500.00, 'Natural breeding, result in beautiful bull calf.', DATEADD(month, -18, GETUTCDATE())),
(5, 4, 3, 3, DATEADD(month, -11, GETUTCDATE()), DATEADD(month, -2, GETUTCDATE()), DATEADD(month, -2, GETUTCDATE()), 0, 1, 1, 'Brahman', 'Selim Herdsman', 0.00, 'Natural breeding pasture calf born healthy.', DATEADD(month, -11, GETUTCDATE())),
(6, 6, NULL, 5, DATEADD(month, -1, GETUTCDATE()), DATEADD(month, 8, GETUTCDATE()), NULL, 1, 0, NULL, 'Red Chittagong Straw', 'Jahangir Alam', 1200.00, 'Insemination done successfully, under observation.', DATEADD(month, -1, GETUTCDATE()));
SET IDENTITY_INSERT [Breedings] OFF;
GO

-- ────────── 20. FEEDRECORDS ──────────
PRINT 'Seeding FeedRecords...';
SET IDENTITY_INSERT [FeedRecords] ON;
INSERT INTO [FeedRecords] (
    [Id], [FarmId], [CattleId], [RecordedByWorkerId], [FeedType], [FeedName], 
    [QuantityKg], [CostPerKg], [Date], [Supplier], [Notes], [CreatedAt]
) VALUES 
(1, 1, 1, 1, 2, 'High-Pro Concentrate', 15.5, 45.0000, DATEADD(day, -1, GETUTCDATE()), 'Bengal Feed Ltd.', 'Morning feeding session.', DATEADD(day, -1, GETUTCDATE())),
(2, 2, 3, 2, 0, 'Alfalfa Hay Straw', 30.0, 35.0000, DATEADD(day, -1, GETUTCDATE()), 'Savar Straw Traders', 'Evening feed routine.', DATEADD(day, -1, GETUTCDATE())),
(3, 3, 4, 3, 1, 'Maize Silage Gold', 50.0, 25.0000, DATEADD(day, -2, GETUTCDATE()), 'Integrated Agro Supp.', 'Checked diet formulation.', DATEADD(day, -2, GETUTCDATE())),
(4, 4, 5, 4, 4, 'Mineral Block Booster', 5.0, 150.0000, DATEADD(day, -3, GETUTCDATE()), 'ACME Vet Feeds', 'Placed in licking stall.', DATEADD(day, -3, GETUTCDATE())),
(5, 5, 6, 5, 5, 'Vitamin Premix Supplement', 2.5, 300.0000, DATEADD(day, -4, GETUTCDATE()), 'Lazz Pharma Vet', 'Mixed with grain feed.', DATEADD(day, -4, GETUTCDATE())),
(6, 6, 2, 6, 6, 'Organic Green Pasture Grass', 100.0, 5.0000, DATEADD(day, -1, GETUTCDATE()), 'Local Harvest', 'Direct grazing equivalent.', DATEADD(day, -1, GETUTCDATE()));
SET IDENTITY_INSERT [FeedRecords] OFF;
GO

-- ────────── 21. HEALTHRECORDS ──────────
PRINT 'Seeding HealthRecords...';
SET IDENTITY_INSERT [HealthRecords] ON;
INSERT INTO [HealthRecords] (
    [Id], [RecordDate], [Temperature], [Weight], [HealthStatus], [RiskLevel], 
    [Symptoms], [Notes], [VetRecommendation], [IsDeleted], [DeletedAt], 
    [CreatedAt], [CattleId], [DoctorId]
) VALUES 
(1, DATEADD(day, -10, GETUTCDATE()), 39.5, 460.0, 2, 1, 'Mild limping, swollen hoof', 'Mild hoof rot infection', 'Apply copper sulfate solution and clean bandaging, keep in dry stall.', 0, NULL, DATEADD(day, -10, GETUTCDATE()), 5, 4),
(2, DATEADD(day, -15, GETUTCDATE()), 38.2, 550.0, 0, 0, 'No symptoms, routine check', 'Routine pregnancy evaluation check', 'No special recommendations, ready for pregnancy schedule.', 0, NULL, DATEADD(day, -15, GETUTCDATE()), 2, 6),
(3, DATEADD(day, -30, GETUTCDATE()), 40.1, 380.0, 3, 2, 'High fever, loss of appetite', 'Post-calving metritis, uterine inflammation', 'Intrauterine antibiotic infusion and anti-inflammatories.', 0, NULL, DATEADD(day, -30, GETUTCDATE()), 4, 1),
(4, DATEADD(day, -25, GETUTCDATE()), 38.8, 420.0, 2, 1, 'Bloated stomach, rapid breathing', 'Digestive gas bloat', 'Administered anti-bloat drench, light walking, fast for 12 hours.', 0, NULL, DATEADD(day, -25, GETUTCDATE()), 1, 2),
(5, DATEADD(day, -5, GETUTCDATE()), 38.5, 310.0, 0, 0, 'Routine quarterly check', 'General health and deworming audit', 'Deworming booster oral dose administered successfully.', 0, NULL, DATEADD(day, -5, GETUTCDATE()), 6, 5),
(6, DATEADD(day, -40, GETUTCDATE()), 38.6, 545.0, 0, 0, 'Routine dairy teat health check', 'Passed standard somatic cell count milk checks', 'Teat dipping disinfection protocol audit passed.', 0, NULL, DATEADD(day, -40, GETUTCDATE()), 2, 3);
SET IDENTITY_INSERT [HealthRecords] OFF;
GO

-- ────────── 22. MEDICINERECORDS ──────────
PRINT 'Seeding MedicineRecords...';
SET IDENTITY_INSERT [MedicineRecords] ON;
INSERT INTO [MedicineRecords] (
    [Id], [MedicineName], [Dosage], [StartDate], [EndDate], [Notes], 
    [IsCompleted], [CreatedAt], [CattleId], [PrescribedByDoctorId]
) VALUES 
(1, 'Penicillin G', '20ml daily IM', DATEADD(day, -10, GETUTCDATE()), DATEADD(day, -5, GETUTCDATE()), 'Antibiotic course for hoof infection.', 1, DATEADD(day, -10, GETUTCDATE()), 5, 4),
(2, 'Meloxicam', '10ml daily oral', DATEADD(day, -10, GETUTCDATE()), DATEADD(day, -7, GETUTCDATE()), 'Anti-inflammatory for pain management.', 1, DATEADD(day, -10, GETUTCDATE()), 5, 4),
(3, 'B-Complex Injection', '15ml once', DATEADD(day, -5, GETUTCDATE()), NULL, 'Appetite booster post-deworming.', 1, DATEADD(day, -5, GETUTCDATE()), 6, 5),
(4, 'Intrauterine Oxytetracycline', '100ml single dose', DATEADD(day, -30, GETUTCDATE()), DATEADD(day, -30, GETUTCDATE()), 'Treatment for metritis post-calving.', 1, DATEADD(day, -30, GETUTCDATE()), 4, 1),
(5, 'Tympol Drench', '100ml single dose', DATEADD(day, -25, GETUTCDATE()), DATEADD(day, -25, GETUTCDATE()), 'Oral drench to treat acute rumen bloat.', 1, DATEADD(day, -25, GETUTCDATE()), 1, 2),
(6, 'Calcium Borogluconate 25%', '450ml IV infusion', DATEADD(day, -2, GETUTCDATE()), DATEADD(day, -2, GETUTCDATE()), 'Emergency treatment for milk fever symptoms.', 1, DATEADD(day, -2, GETUTCDATE()), 2, 6);
SET IDENTITY_INSERT [MedicineRecords] OFF;
GO

-- ────────── 23. VACCINATIONS ──────────
PRINT 'Seeding Vaccinations...';
SET IDENTITY_INSERT [Vaccinations] ON;
INSERT INTO [Vaccinations] (
    [Id], [VaccineName], [VaccinationDate], [NextDueDate], [AdministeredBy], 
    [DoseNumber], [Notes], [BatchNumber], [CreatedAt], [CattleId], [DoctorId]
) VALUES 
(1, 'FMD (Foot and Mouth Disease) Vaccine', DATEADD(day, -100, GETUTCDATE()), DATEADD(day, 80, GETUTCDATE()), 'Dr. Nazmul Hasan', 1, 'Regular bi-annual FMD schedule vaccination.', 'FMD-B-9022', DATEADD(day, -100, GETUTCDATE()), 1, 5),
(2, 'Anthrax Spore Vaccine', DATEADD(day, -120, GETUTCDATE()), DATEADD(day, 245, GETUTCDATE()), 'Dr. Tanzina Akhter', 1, 'Annual anthrax spore immunization.', 'ANT-B-5011', DATEADD(day, -120, GETUTCDATE()), 2, 3),
(3, 'Black Quarter (BQ) Vaccine', DATEADD(day, -80, GETUTCDATE()), DATEADD(day, 100, GETUTCDATE()), 'Dr. Shafiqul Islam', 1, 'BQ prevention dose.', 'BQ-B-4322', DATEADD(day, -80, GETUTCDATE()), 3, 1),
(4, 'Brucellosis Vaccine (S19)', DATEADD(day, -150, GETUTCDATE()), NULL, 'Dr. Farhana Yasmin', 1, 'Lifetime heifer brucellosis immunity dose.', 'BRU-B-1088', DATEADD(day, -150, GETUTCDATE()), 4, 6),
(5, 'FMD Booster Dose', DATEADD(day, -45, GETUTCDATE()), DATEADD(day, 135, GETUTCDATE()), 'Dr. Latifur Rahman', 2, 'Booster FMD shot given to steer.', 'FMDB-B-1122', DATEADD(day, -45, GETUTCDATE()), 5, 2),
(6, 'HS (Hemorrhagic Septicemia) Vaccine', DATEADD(day, -60, GETUTCDATE()), DATEADD(day, 305, GETUTCDATE()), 'Dr. Mahbubul Alam', 1, 'HS prevention dose.', 'HS-B-8902', DATEADD(day, -60, GETUTCDATE()), 6, 4);
SET IDENTITY_INSERT [Vaccinations] OFF;
GO

-- ────────── 24. MILKPRODUCTIONS ──────────
PRINT 'Seeding MilkProductions...';
SET IDENTITY_INSERT [MilkProductions] ON;
INSERT INTO [MilkProductions] (
    [Id], [Date], [MorningYieldLiters], [EveningYieldLiters], [Notes], 
    [CreatedAt], [CattleId], [FarmId], [RecordedByWorkerId]
) VALUES 
(1, DATEADD(day, -1, GETUTCDATE()), 15.5, 12.0, 'Excellent morning yield. Normal behavior.', DATEADD(day, -1, GETUTCDATE()), 2, 1, 1),
(2, DATEADD(day, -2, GETUTCDATE()), 14.8, 11.5, 'Slightly hot weather reduced evening yield.', DATEADD(day, -2, GETUTCDATE()), 2, 1, 1),
(3, DATEADD(day, -1, GETUTCDATE()), 12.2, 9.8, 'Standard Jersey milk fat content high.', DATEADD(day, -1, GETUTCDATE()), 4, 3, 3),
(4, DATEADD(day, -2, GETUTCDATE()), 11.9, 9.5, 'Consistent production cycle.', DATEADD(day, -2, GETUTCDATE()), 4, 3, 3),
(5, DATEADD(day, -1, GETUTCDATE()), 8.5, 6.2, 'Indigenous Red Chittagong yield.', DATEADD(day, -1, GETUTCDATE()), 6, 5, 5),
(6, DATEADD(day, -2, GETUTCDATE()), 8.2, 6.0, 'First calving cycle milk yield record.', DATEADD(day, -2, GETUTCDATE()), 6, 5, 5);
SET IDENTITY_INSERT [MilkProductions] OFF;
GO

-- ────────── 25. VEHICLES ──────────
PRINT 'Seeding Vehicles...';
SET IDENTITY_INSERT [Vehicles] ON;
INSERT INTO [Vehicles] (
    [Id], [Name], [Type], [RegistrationNumber], [Capacity], [CapacityUnit], 
    [FuelType], [FuelCostPerKm], [Status], [ImagePath], [Notes], [IsDeleted], 
    [CreatedAt], [UpdatedAt]
) VALUES 
(1, 'Savar Farm Delivery Truck', 0, 'DHAKA-METRO-TA-12-3456', 5.5, 'tonnes', 0, 18.50, 0, 'https://images.unsplash.com/photo-1516576885230-eaadb65f769b?w=400&q=80', 'Heavy duty white delivery truck, fully air conditioned.', 0, DATEADD(month, -6, GETUTCDATE()), GETUTCDATE()),
(2, 'Chattogram Livestock Trailer', 4, 'CHATTOGRAM-METRO-HA-15-7890', 10.0, 'tonnes', 0, 25.00, 0, 'https://images.unsplash.com/photo-1601584115197-04ecc0da31d7?w=400&q=80', 'Custom cattle transport deck trailer with dual hydraulic ramps.', 0, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE()),
(3, 'Sylhet Pasture Tractor', 5, 'SYLHET-METRO-YA-11-2233', 3.0, 'tonnes', 0, 30.00, 0, 'https://images.unsplash.com/photo-1500937386664-56d15943747d?w=400&q=80', 'John Deere agricultural pasture tractor.', 0, DATEADD(month, -6, GETUTCDATE()), GETUTCDATE()),
(4, 'Rajshahi Utility Pickup', 1, 'RAJSHAHI-METRO-GA-14-1122', 2.0, 'tonnes', 2, 12.00, 0, 'https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=400&q=80', 'Double cabin CNG utility pickup.', 0, DATEADD(month, -3, GETUTCDATE()), GETUTCDATE()),
(5, 'Mymensingh Breeding Van', 2, 'MYMENSINGH-METRO-MA-13-8899', 1.5, 'tonnes', 1, 15.00, 0, 'https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?w=400&q=80', 'Equipped with liquid nitrogen tank storage for insemination straws.', 0, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE()),
(6, 'Comilla Milk Courier Bike', 3, 'COMILLA-METRO-HA-19-4455', 0.2, 'tonnes', 1, 4.50, 0, 'https://images.unsplash.com/photo-1558981806-ec527fa84c39?w=400&q=80', 'Motorbike with heavy-duty side racks for milk cans.', 0, DATEADD(month, -2, GETUTCDATE()), GETUTCDATE());
SET IDENTITY_INSERT [Vehicles] OFF;
GO

-- ────────── 26. DRIVERS ──────────
PRINT 'Seeding Drivers...';
SET IDENTITY_INSERT [Drivers] ON;
INSERT INTO [Drivers] (
    [Id], [FullName], [Phone], [LicenseNumber], [LicenseType], [ExperienceYears], 
    [Address], [Rating], [Status], [AssignedVehicleId], [Notes], [ImagePath], 
    [IsDeleted], [CreatedAt], [UpdatedAt]
) VALUES 
(1, 'Kabir Hashim', '01712-112233', 'DL-DHAKA-8902A', 'Heavy Commercial', 12, 'Mirpur-12, Dhaka', 4.8, 0, 1, 'Highly experienced heavy truck driver, clean record.', 'https://images.unsplash.com/photo-1542909168-82c3e7fdca5c?w=400&q=80', 0, DATEADD(month, -5, GETUTCDATE()), GETUTCDATE()),
(2, 'Sujon Mia', '01823-223344', 'DL-CTG-3122B', 'Heavy Commercial', 8, 'Halisahar, Chattogram', 4.7, 0, 2, 'Expert in livestock highway hauling.', 'https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=400&q=80', 0, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE()),
(3, 'Dulal Sheikh', '01934-334455', 'DL-SYL-9022C', 'Light Commercial', 10, 'Zindabazar, Sylhet', 4.9, 0, 3, 'Drives local tractors and feed trucks.', 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=400&q=80', 0, DATEADD(month, -6, GETUTCDATE()), GETUTCDATE()),
(4, 'Azizul Haque', '01545-445566', 'DL-RAJ-5512D', 'Light Commercial', 6, 'Paba, Rajshahi', 4.5, 0, 4, 'Very dependable pickup driver.', 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=400&q=80', 0, DATEADD(month, -3, GETUTCDATE()), GETUTCDATE()),
(5, 'Abdur Razzaq', '01656-556677', 'DL-MYM-8890E', 'Light Commercial', 5, 'Trishal, Mymensingh', 4.6, 0, 5, 'Maintains cold chain breeding straw security.', 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=400&q=80', 0, DATEADD(month, -4, GETUTCDATE()), GETUTCDATE()),
(6, 'Mofizur Rahman', '01767-667788', 'DL-COM-4431F', 'Motorcycle Light', 4, 'Daudkandi, Comilla', 4.8, 0, 6, 'Swift dairy can motorcycle operator.', 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=400&q=80', 0, DATEADD(month, -2, GETUTCDATE()), GETUTCDATE());
SET IDENTITY_INSERT [Drivers] OFF;
GO

-- ────────── 27. TRANSPORTREQUESTS ──────────
PRINT 'Seeding TransportRequests...';
SET IDENTITY_INSERT [TransportRequests] ON;
INSERT INTO [TransportRequests] (
    [Id], [RequestType], [PickupLocation], [Destination], [ScheduledDate], 
    [ScheduledTime], [EstimatedDistanceKm], [CargoWeight], [CargoDescription], 
    [Status], [Notes], [OrderId], [FarmId], [RequestedByUserId], [IsDeleted], 
    [CreatedAt], [UpdatedAt]
) VALUES 
(1, 2, 'Savar Farm 1', 'Banani Customer Stop', DATEADD(day, -9, GETUTCDATE()), '08:00:00', 25.5, 0.50, 'Fresh Raw Milk 500 Liters cans.', 4, 'Keep temperature cold.', 1, 1, 6, 0, DATEADD(day, -10, GETUTCDATE()), GETUTCDATE()),
(2, 2, 'Hathazari Farm 2', 'Gulshan Customer Stop', DATEADD(day, -14, GETUTCDATE()), '07:30:00', 260.0, 0.10, 'Premium Beef steak meat cartons.', 4, 'Express freezer shipment.', 2, 2, 6, 0, DATEADD(day, -15, GETUTCDATE()), GETUTCDATE()),
(3, 2, 'Sreemangal Farm 3', 'Sylhet Customer Stop', DATEADD(day, -19, GETUTCDATE()), '09:00:00', 85.0, 2.00, 'Organic Compost fertilizer sacks.', 4, 'Cover with tarp if raining.', 3, 3, 6, 0, DATEADD(day, -20, GETUTCDATE()), GETUTCDATE()),
(4, 0, 'Paba Farm 4', 'Rajshahi Cantt Stop', DATEADD(day, -1, GETUTCDATE()), '10:00:00', 12.0, 1.20, 'Pedigree steer transport.', 1, 'Handle with extra care.', 4, 4, 6, 0, DATEADD(day, -2, GETUTCDATE()), GETUTCDATE()),
(5, 2, 'Trishal Farm 5', 'Mymensingh Sadar Stop', DATEADD(day, -7, GETUTCDATE()), '08:30:00', 35.0, 0.05, 'Breeding straw distribution chest.', 4, 'Liquid nitrogen storage check.', 5, 5, 6, 0, DATEADD(day, -8, GETUTCDATE()), GETUTCDATE()),
(6, 2, 'Daudkandi Farm 6', 'Comilla Cantt Stop', DATEADD(day, 1, GETUTCDATE()), '07:00:00', 45.0, 0.06, 'Fresh Raw Bottled Milk 50 Liters.', 0, 'Pending vehicle confirmation.', 6, 6, 6, 0, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE());
SET IDENTITY_INSERT [TransportRequests] OFF;
GO

-- ────────── 28. TRIPS ──────────
PRINT 'Seeding Trips...';
SET IDENTITY_INSERT [Trips] ON;
INSERT INTO [Trips] (
    [Id], [TransportRequestId], [VehicleId], [DriverId], [Status], 
    [StartTime], [EndTime], [DistanceKm], [FuelCostPerKm], [BaseCost], 
    [FuelCost], [AdditionalCost], [TotalCost], [AdditionalCostNote], 
    [RouteNotes], [Notes], [IsDeleted], [CreatedAt], [UpdatedAt]
) VALUES 
(1, 1, 1, 1, 4, DATEADD(hour, -14, GETUTCDATE()), DATEADD(hour, -12, GETUTCDATE()), 26.2, 18.50, 1500.00, 484.70, 200.00, 2184.70, 'Toll plaza charges.', 'Dhaka-Aricha highway via Gabtoli flyover.', 'Arrived on schedule.', 0, DATEADD(day, -9, GETUTCDATE()), GETUTCDATE()),
(2, 2, 2, 2, 4, DATEADD(hour, -20, GETUTCDATE()), DATEADD(hour, -14, GETUTCDATE()), 265.5, 25.00, 8000.00, 6637.50, 500.00, 15137.50, 'Bridge tolls and municipal taxes.', 'Dhaka-Chittagong highway via Comilla bypass.', 'Excellent freezer cargo state.', 0, DATEADD(day, -14, GETUTCDATE()), GETUTCDATE()),
(3, 3, 3, 3, 4, DATEADD(hour, -15, GETUTCDATE()), DATEADD(hour, -11, GETUTCDATE()), 88.0, 30.00, 3000.00, 2640.00, 100.00, 5740.00, 'Unloading labor tip.', 'Sreemangal-Sylhet highway.', 'Delivered safely.', 0, DATEADD(day, -19, GETUTCDATE()), GETUTCDATE()),
(4, 4, 4, 4, 2, NULL, NULL, 12.0, 12.00, 1000.00, 144.00, 0.00, 1144.00, NULL, 'Rajshahi bypass road.', 'Driver assigned, preparing cabin.', 0, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE()),
(5, 5, 5, 5, 4, DATEADD(hour, -14, GETUTCDATE()), DATEADD(hour, -12, GETUTCDATE()), 36.5, 15.00, 2000.00, 547.50, 0.00, 2547.50, NULL, 'Trishal-Mymensingh regional highway.', 'High nitrogen security maintained.', 0, DATEADD(day, -7, GETUTCDATE()), GETUTCDATE()),
(6, 6, 6, 6, 0, NULL, NULL, 45.0, 4.50, 500.00, 202.50, 0.00, 702.50, NULL, 'Dhaka-Comilla local route.', 'Awaiting schedule confirmation.', 0, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE());
SET IDENTITY_INSERT [Trips] OFF;
GO

PRINT 'Database successfully seeded with 6 high-fidelity records per table (28 tables total).';
GO
