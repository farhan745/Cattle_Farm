# 💾 Data Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\Data`

The **Data** folder manages compile-time database seeding and data initialization scripts. This ensures that the application has a valid schema and initial datasets when launched for development or testing.

---

## 🎯 Goal
To verify that the database is populated with initial security roles, default administrative accounts, and standard lookup tables during startup.

---

## 📅 Roadmap & Milestones
- **Week 1 Setup**: The [DbSeeder.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Data/DbSeeder.cs) script is configured and executed inside `Program.cs` under the database initialization block.
- **Continuous Integration**: Used to seed test records (e.g. mock cattle, farms, products, veterinarians, and workers) to let developers test features locally without manual entry.

---

## 📋 Tasks & Deliverables
1. **Ensure Database Schema**: Confirm that database migrations are applied before running seed queries.
2. **Inject Core Roles**: Register standard system authorization roles defined in `AppRoles.cs`.
3. **Generate Default Accounts**: Seed credential profiles for roles: `Admin`, `Owner`, `Manager`, and `Customer` with secure BCrypt-hashed passwords.
4. **Seed System Operations**: Generate mock data representing farms, cattle inventory, products, veterinarians, and workers.

---

## 🗃️ Files & Modules

| File Name | Purpose | Target Functions | Link |
| :--- | :--- | :--- | :--- |
| `DbSeeder.cs` | Executes database seeding routines asynchronously during app startup. | `SeedAsync(CattleFarmDbContext db, bool isDev)` | [DbSeeder.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Data/DbSeeder.cs) |

---

## 🏆 Milestone Contribution
This folder implements the **Database Seeding Milestone**, ensuring the database is populated with essential system roles, default user accounts, and test records when the application runs.
