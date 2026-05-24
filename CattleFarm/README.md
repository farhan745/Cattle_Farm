# 🏗️ CattleFarm Project Root Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm`

This directory is the core of the **CattleFarm** web application project, containing compile-time settings, dependency packages, application startup files, and system-wide security configurations.

---

## 🎯 Goal
To initialize the ASP.NET Core 10.0 runtime environment, configure dependency injection services (EF DbContext, repositories, services, email, and payment providers), set up cookie authentication, and manage application settings.

---

## 📅 Roadmap & Milestones
- **Week 1 Setup (Core Startup & Security)**: Coding database initializers and security roles.
- **Continuous Integration**: Maintaining package dependencies and configurations for all modules.

---

## 📋 Tasks & Deliverables
1. **Initialize Request Pipeline**: Configure middlewares (routing, static files, authentication, authorization, Serilog) inside [Program.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Program.cs).
2. **Define Security Roles**: Centralize role names inside [AppRoles.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/AppRoles.cs) to enforce authorization rules.
3. **Register NuGet Dependencies**: Manage library versions in [CattleFarm.csproj](file:///f:/VisualStudio/CattleFarm/CattleFarm/CattleFarm.csproj).
4. **Manage Environment Configurations**: Maintain connection strings and API tokens in [appsettings.json](file:///f:/VisualStudio/CattleFarm/CattleFarm/appsettings.json).

---

## 🗃️ Project Root Files

| File Name | Purpose | Link |
| :--- | :--- | :--- |
| `Program.cs` | Application startup file. Registers services and defines middleware pipelines. | [Program.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Program.cs) |
| `AppRoles.cs` | Definiton of system roles: `Admin`, `Owner`, `Manager`, `Worker`, `Customer`. | [AppRoles.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/AppRoles.cs) |
| `CattleFarm.csproj` | C# project configuration file outlining package dependencies. | [CattleFarm.csproj](file:///f:/VisualStudio/CattleFarm/CattleFarm/CattleFarm.csproj) |
| `appsettings.json` | Configuration file storing DB connection strings, Serilog levels, and payment gateway tokens. | [appsettings.json](file:///f:/VisualStudio/CattleFarm/CattleFarm/appsettings.json) |
| `gmail.md` | Reference file showing seeded test user login credentials. | [gmail.md](file:///f:/VisualStudio/CattleFarm/CattleFarm/gmail.md) |

---

## 🏆 Milestone Contribution
This folder represents the **Application Core Configuration Milestone**, providing the foundational setup required to execute database, service, repository, and controller operations.
