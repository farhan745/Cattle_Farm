# 🎮 Controllers Directory Guide
### 📂 Folder Location: `f:\VisualStudio\CattleFarm\CattleFarm\Controllers`

The **Controllers** folder contains ASP.NET Core MVC controllers. These classes act as the entry point for HTTP requests, handling route requests, validating user input payloads, invoking business logic via domain services, and returning MVC Razor Views or JSON responses.

---

## 🎯 Goal
To orchestrate application flow by handling user requests, enforcing routing, checking authentication and role-based permissions, and mapping model data onto Views or ViewModels.

---

## 📅 Roadmap & Milestones
Controllers are developed incrementally aligned with the **8-Week Project Roadmap**:
- **Week 1 (Security & Identity)**: [AccountController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/AccountController.cs) controls logins, signups, and credential validations.
- **Week 2 (Core Inventory)**: [FarmController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/FarmController.cs) and [CattleController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/CattleController.cs) manage cattle profiles and physical farms.
- **Week 3 (Daily Production)**: [FeedController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/FeedController.cs) and [MilkProductionController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/MilkProductionController.cs) capture daily milk harvests and livestock feed consumption logs.
- **Week 4 (Veterinary Care)**: [DoctorController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/DoctorController.cs), [AppointmentController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/AppointmentController.cs), and [HealthController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/HealthController.cs) coordinate vet logs.
- **Week 5 (Breeding & Alerts)**: [BreedingController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/BreedingController.cs) and [NotificationController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/NotificationController.cs) handle livestock pregnancy tracking.
- **Week 6 (Marketplace Commerce)**: [ProductController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/ProductController.cs) and [OrderController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/OrderController.cs) manage customer storefront catalog items and order tracking.
- **Week 7 (Logistics & Transport)**: [TransportController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/TransportController.cs) handles driver assignments and delivery route monitoring.
- **Week 8 (Analytics & Reports)**: [DashboardController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/DashboardController.cs), [FinancialController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/FinancialController.cs), and [ReportsController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/ReportsController.cs) compile operational data.
- **HR & Management (Supporting)**: [EmployeeController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/EmployeeController.cs), [WorkerController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/WorkerController.cs), [AttendanceController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/AttendanceController.cs), [PayrollController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/PayrollController.cs), and [SubscriptionController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/SubscriptionController.cs) handle staff.

---

## 📋 Tasks & Deliverables
1. **Request Routing**: Define custom route rules for dashboard pages and catalog actions.
2. **Access Control**: Annotate endpoints with `[Authorize(Roles = AppRoles.Owner)]` or other security roles to restrict access.
3. **Data Binding & Validation**: Validate incoming view model forms using MVC model binding validators.
4. **File Attachment Handling**: Process uploaded profile images for cattle, workers, or store items using image helpers.

---

## 🗃️ Files & Modules

| Controller | Primary Function | Link |
| :--- | :--- | :--- |
| `AccountController.cs` | Handles login sessions, password validations, and registration. | [AccountController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/AccountController.cs) |
| `AdminController.cs` | Manages global administrative functions and systems settings. | [AdminController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/AdminController.cs) |
| `CattleController.cs` | Manages livestock records, status flags, and photo listings. | [CattleController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/CattleController.cs) |
| `FarmController.cs` | Configures and lists physical farm locations. | [FarmController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/FarmController.cs) |
| `MilkProductionController.cs` | Captures daily milk production statistics per cow and farm. | [MilkProductionController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/MilkProductionController.cs) |
| `FeedController.cs` | Records daily feed rations and food items consumed. | [FeedController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/FeedController.cs) |
| `DoctorController.cs` | Manages profiles and schedules for visiting veterinarians. | [DoctorController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/DoctorController.cs) |
| `AppointmentController.cs` | Schedules checkups between vets and specific cattle. | [AppointmentController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/AppointmentController.cs) |
| `HealthController.cs` | Logs diagnostic charts, treatments, and prescriptions. | [HealthController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/HealthController.cs) |
| `BreedingController.cs` | Tracks inseminations, pregnancy status, and calving dates. | [BreedingController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/BreedingController.cs) |
| `ProductController.cs` | Handles catalog stock levels and product images. | [ProductController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/ProductController.cs) |
| `OrderController.cs` | Manages client shopping orders and delivery progress. | [OrderController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/OrderController.cs) |
| `TransportController.cs` | Manages vehicle fleets, dispatches drivers, and tracks trips. | [TransportController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/TransportController.cs) |
| `DashboardController.cs` | Gathers statistics to display tailored KPI summaries to logged-in users. | [DashboardController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/DashboardController.cs) |
| `FinancialController.cs` | Logs and monitors feed expenses, salaries, and sales revenues. | [FinancialController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/FinancialController.cs) |
| `ReportsController.cs` | Builds ClosedXML reports to download spreadsheets. | [ReportsController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/ReportsController.cs) |

---

## 🏆 Milestone Contribution
This folder fulfills the **Client-Interaction Layer Milestone** for all feature areas. By translating customer clicks and form submissions into state changes, it provides the main interface for users to perform management actions.
