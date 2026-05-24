# 📂 Models & DbContext Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\Models`

The **Models** folder defines the domain entities, relationships, database configurations, validation rules, and system enums. These classes map directly to SQL Server database tables using Entity Framework Core (EF Core).

---

## 🎯 Goal
To serve as the single source of truth for the application's domain model, configuring relationships, schema constraints, and database connections via the [CattleFarmDbContext.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/CattleFarmDbContext.cs).

---

## 📅 Roadmap & Milestones
Model schema additions align with the weekly features of the project:
- **Week 1**: Core identity models: [User.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/User.cs).
- **Week 2**: Inventory models: [Farm.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Farm.cs) and [Cattle.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Cattle.cs).
- **Week 3**: Yields models: [MilkProduction.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/MilkProduction.cs) and [FeedRecord.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/FeedRecord.cs).
- **Week 4**: Vet models: [Doctor.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Doctor.cs), [Appointment.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Appointment.cs), [HealthRecord.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/HealthRecord.cs), [MedicineRecord.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/MedicineRecord.cs), and [Vaccination.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Vaccination.cs).
- **Week 5**: Reproduction models: [Breeding.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Breeding.cs) and [Notification.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Notification.cs).
- **Week 6**: Catalog models: [Product.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Product.cs), [Order.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Order.cs), and [OrderItem.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/OrderItem.cs).
- **Week 7**: Logistics models: [Vehicle.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Vehicle.cs), [Driver.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Driver.cs), [Trip.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Trip.cs), [TransportRequest.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/TransportRequest.cs), and [Payment.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Payment.cs).
- **Week 8**: Analytics and Logs models: [Revenue.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Revenue.cs), [Expense.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Expense.cs), [AuditLog.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/AuditLog.cs), and [ActivityLog.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/ActivityLog.cs).
- **HR Support**: [Worker.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Worker.cs), [Attendance.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Attendance.cs), [WorkerAttendance.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/WorkerAttendance.cs), and [Subscription.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Subscription.cs).

---

## 📋 Tasks & Deliverables
1. **Model Property Validation**: Standardize annotations like `[Required]`, `[StringLength]`, and `[Range]` to ensure correct database inputs.
2. **Relationships Configuration**: Define foreign keys, index structures, composite keys, and delete behaviors in [CattleFarmDbContext.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/CattleFarmDbContext.cs) using EF Core Fluent API rules.
3. **Design-Time Context Factory**: Enable Entity Framework CLI tools to discover migrations via [CattleFarmDbContextFactory.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/CattleFarmDbContextFactory.cs).
4. **Standardize System Constants**: Configure unified types, status flags, and gender enumerations inside [Enums.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Enums.cs).

---

## 🗃️ Files & Modules

| Class File | Database Mapping Table | Key Fields & Responsibilities | Link |
| :--- | :--- | :--- | :--- |
| `CattleFarmDbContext.cs` | *None (EF Context)* | Maps database sets, executes Fluent configurations, and overrides `SaveChanges` to capture audit logs. | [CattleFarmDbContext.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/CattleFarmDbContext.cs) |
| `User.cs` | `Users` | Security accounts with names, emails, roles, and hashed passwords. | [User.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/User.cs) |
| `Cattle.cs` | `Cattles` | Livestock inventory tracking age, gender, status, tag IDs, and photo paths. | [Cattle.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Cattle.cs) |
| `Farm.cs` | `Farms` | Physical location addresses, boundaries, and farm managers. | [Farm.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Farm.cs) |
| `MilkProduction.cs` | `MilkProductions` | Daily yields logged per animal, including date and session timings (Morning/Evening). | [MilkProduction.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/MilkProduction.cs) |
| `FeedRecord.cs` | `FeedRecords` | Livestock nutrition logs detailing feed types, amounts, and dates. | [FeedRecord.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/FeedRecord.cs) |
| `Breeding.cs` | `Breedings` | Breeding logs linking sire, dam, insemination dates, and delivery estimates. | [Breeding.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Breeding.cs) |
| `Doctor.cs` | `Doctors` | Registries for external veterinarians, specialties, and clinics. | [Doctor.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Doctor.cs) |
| `Appointment.cs` | `Appointments` | Scheduling for cattle vet checkups. | [Appointment.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Appointment.cs) |
| `HealthRecord.cs` | `HealthRecords` | Diagnostic history charts, logs, and medical descriptions. | [HealthRecord.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/HealthRecord.cs) |
| `MedicineRecord.cs` | `MedicineRecords` | Log details of administered medicine, dosage volumes, and withdrawal windows. | [MedicineRecord.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/MedicineRecord.cs) |
| `Vaccination.cs` | `Vaccinations` | Records immunization sessions and booster intervals. | [Vaccination.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Vaccination.cs) |
| `Product.cs` | `Products` | Storefront catalog items, pricing, inventory stocks, and pictures. | [Product.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Product.cs) |
| `Order.cs` | `Orders` | Customer transaction details, total totals, and delivery destinations. | [Order.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Order.cs) |
| `OrderItem.cs` | `OrderItems` | Linking individual products, order quantities, and unit price values. | [OrderItem.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/OrderItem.cs) |
| `Vehicle.cs` | `Vehicles` | Fleet assets, tracking fuel, license tags, and operation status. | [Vehicle.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Vehicle.cs) |
| `Driver.cs` | `Drivers` | Delivery driver details and contact logs. | [Driver.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Driver.cs) |
| `Trip.cs` | `Trips` | Driver transport logs monitoring transit progress from farm to customer. | [Trip.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Trip.cs) |
| `TransportRequest.cs` | `TransportRequests` | Outlines the shipment payload requirements for logistics scheduling. | [TransportRequest.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/TransportRequest.cs) |
| `Payment.cs` | `Payments` | Payment log storing gateway transaction references and status. | [Payment.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Payment.cs) |
| `Revenue.cs` | `Revenues` | Cash flows tracking store sales, milk deliveries, and livestock auctions. | [Revenue.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Revenue.cs) |
| `Expense.cs` | `Expenses` | Cost logs tracking salaries, feed purchases, logistics, and vet bills. | [Expense.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Expense.cs) |
| `AuditLog.cs` | `AuditLogs` | Stores automated security logs tracking record creation, modification, and deletion. | [AuditLog.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/AuditLog.cs) |
| `Worker.cs` | `Workers` | Profiles for farm labor and specialized field hands. | [Worker.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Worker.cs) |
| `Enums.cs` | *None* | Definitions for standard statuses (CattleStatus, BreedingStatus, OrderStatus, etc.). | [Enums.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/Enums.cs) |

---

## 🏆 Milestone Contribution
This folder implements the **Domain Schema Milestone**. It provides the core structures, indexes, and tables required by Entity Framework Core to build and interact with the database tables.
