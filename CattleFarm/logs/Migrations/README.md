# 🚀 Migrations Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\Migrations`

The **Migrations** folder contains Entity Framework Core database migration classes. These C# files serve as incremental, version-controlled scripts that modify the SQL Server database schema when changes are made to Model classes.

---

## 🎯 Goal
To track changes to the database schema. This allows team members and deployment pipelines to update database tables without data loss.

---

## 📅 Roadmap & Milestones
Migrations are generated chronologically throughout development:
- **Initial Setup**: [20260517165359_InitialCreate.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260517165359_InitialCreate.cs) builds the core tables for Users, Farms, Cattle, and Products.
- **Daily Operations & breeding**: [20260519073748_AddBreedingAndFeed.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260519073748_AddBreedingAndFeed.cs) adds gestation and nutrition logging support.
- **Logistics Module**: [20260519195116_AddTransportModule.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260519195116_AddTransportModule.cs) maps vehicles, drivers, and delivery dispatches.
- **HR & Management**: [20260521065838_AddAttendance.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260521065838_AddAttendance.cs) adds employee attendance records.

---

## 📋 Tasks & Deliverables
1. **Generate Migrations**: Compile model class definitions into migration files using CLI tools:
   ```powershell
   dotnet ef migrations add <MigrationName>
   ```
2. **Execute Database Updates**: Apply migrations to update the local SQL database schema:
   ```powershell
   dotnet ef database update
   ```
3. **Verify Up/Down Actions**: Review generated migrations to ensure the `Up()` and `Down()` methods execute correct DDL commands (table creation, renaming, dropping).
4. **Maintain Database Snapshot**: Keep [CattleFarmDbContextModelSnapshot.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/CattleFarmDbContextModelSnapshot.cs) updated as the current model representation.

---

## 🗃️ Key Migration Files

| Migration Timestamp & Name | Database Adjustments Made | Link |
| :--- | :--- | :--- |
| `20260517165359_InitialCreate.cs` | Initial schema setup: Users, Farms, Cattle, Products. | [InitialCreate.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260517165359_InitialCreate.cs) |
| `20260519073748_AddBreedingAndFeed.cs` | Adds Breeding logs, Feed records, Milk production, and Doctors. | [AddBreedingAndFeed.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260519073748_AddBreedingAndFeed.cs) |
| `20260519195116_AddTransportModule.cs` | Adds Logistics tables: Vehicles, Drivers, Trips. | [AddTransportModule.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260519195116_AddTransportModule.cs) |
| `20260521065838_AddAttendance.cs` | Adds Employee Attendance logs. | [AddAttendance.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260521065838_AddAttendance.cs) |
| `20260521174348_AddPasswordResetTokenFields.cs`| Adds fields for password reset tokens. | [AddPasswordResetTokenFields.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/20260521174348_AddPasswordResetTokenFields.cs) |
| `CattleFarmDbContextModelSnapshot.cs` | Current snapshot metadata representing the DB schema. | [CattleFarmDbContextModelSnapshot.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/CattleFarmDbContextModelSnapshot.cs) |

---

## 🏆 Milestone Contribution
This folder implements the **Database Migration Versioning Milestone**, ensuring the database schema matches the C# class definitions.
