# 🗄️ SQL Server Scripts Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\SQL Server Scripts1`

This folder contains the database scripting setup and database initialization procedures for the **Smart Cattle Farm** project. It is configured for use with SQL Server Management Studio (SSMS).

---

## 🎯 Goal
To provide direct SQL scripting tools for backing up, seeding, and executing manual queries against the SQL Server database. This serves as a secondary seeding mechanism alongside theEntity Framework Core migrations and the code-based `DbSeeder`.

---

## 📅 Roadmap & Milestones
- **Week 1 Integration (Database Seeding)**: Populating basic tables, static lookup data, and sample user profiles directly into the SQL Server database instances to enable initial manual testing of role validation.
- **Weekly Updates**: SQL Scripts are updated as the schema changes to maintain a standalone reference database script for local database instances.

---

## 📋 Tasks & Deliverables
1. **Initialize Local DB**: Set up local database schemas matching the Entity Framework migrations.
2. **Execute Seed Data Script**: Load sample data including Farms, Cattle, Workers, Doctors, and Products to test analytical queries.
3. **Database Maintenance**: Verify indices, constraints, and data integrity relationships.

---

## 🗃️ Files & Modules

| File Name | Description | Link |
| :--- | :--- | :--- |
| `SQL Server Scripts1.ssmssln` | SQL Server Management Studio solution grouping database projects. | [SQL Server Scripts1.ssmssln](file:///f:/VisualStudio/CattleFarm/SQL%20Server%20Scripts1/SQL%20Server%20Scripts1.ssmssln) |
| `SQL Server Scripts1/SQL Server Scripts1.ssmssqlproj` | SSMS project outlining folder organization. | [SQL Server Scripts1.ssmssqlproj](file:///f:/VisualStudio/CattleFarm/SQL%20Server%20Scripts1/SQL%20Server%20Scripts1/SQL%20Server%20Scripts1.ssmssqlproj) |
| `SQL Server Scripts1/SeedData.sql` | The primary SQL Script file with pre-written INSERT queries for tables. | [SeedData.sql](file:///f:/VisualStudio/CattleFarm/SQL%20Server%20Scripts1/SQL%20Server%20Scripts1/SeedData.sql) |

---

## 🏆 Milestone Contribution
This folder contributes to the **Database Foundation Milestone** of the project, establishing the base tables, constraints, and relationships that allow the ASP.NET application to store operational, logistics, financial, and user record models.
