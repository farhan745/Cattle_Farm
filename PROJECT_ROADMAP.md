# 📅 Smart Cattle Farm - 8-Week Implementation Roadmap

This document provides a clean, structured, and chronological breakdown of the development cycle for the **Smart Cattle Farm** project. It splits the application into 8 logical phases, mapping specific tasks, files, modules, and milestones for each week.

---

## 📈 Roadmap Execution Strategy

```
[ W1: Core & Auth ] ➔ [ W2: Livestock & Farms ] ➔ [ W3: Daily Operations ] ➔ [ W4: Veterinary & Health ]
                                                                                   │
[ W8: Analytics & Audit ] ◀ [ W7: Logistics & Payments ] ◀ [ W6: Marketplace ] ◀ [ W5: Breeding Lifecycles ]
```

---

## 📋 Weekly Phase Breakdown

### 🎯 Week 1: Foundation Setup, Database & Authentication
*   **Goal**: Establish the relational database schema, seed system-wide security roles, and build the secure user registration and login infrastructure.
*   **Progress Status**: `[▓░░░░░░░] 12.5%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Enable role-based cookie authentication and user session persistence. |
| **Tasks & Deliverables** | 1. Initialize EF Core context, connection strings, and run initial migrations.<br/>2. Code standard system roles inside compile-time rules (`AppRoles.cs`).<br/>3. Develop secure BCrypt-based password hashing, verification, and session state controllers.<br/>4. Populate seed data (Roles, Admin, Manager, Owner, and standard profiles) via database seeding scripts. |
| **Files & Modules** | 📁 `Models/` ➔ `CattleFarmDbContext.cs`, `User.cs`<br/>📁 `Services/` ➔ `AuthService.cs`, `IAuthService.cs`<br/>📁 `Controllers/` ➔ `AccountController.cs`<br/>📁 `Views/` ➔ `Account/Login.cshtml`, `Account/Register.cshtml`<br/>📄 Root ➔ `Program.cs`, `appsettings.json`, `AppRoles.cs`, `Data/DbSeeder.cs` |
| **Technologies Used** | ASP.NET Core 10.0 Web SDK, Entity Framework Core, SQL Server, BCrypt.Net-Next, Cookie Middleware |
| **Milestone** | A working authentication system where users can register, log in securely with cookies, and redirect to dynamic dashboards based on their role (`Admin`, `Owner`, `Manager`, etc.). |

---

### 🎯 Week 2: Farm Profiles & Livestock Inventory Setup
*   **Goal**: Create dynamic multi-farm facility profiles and build the core livestock inventory database with media storage capabilities.
*   **Progress Status**: `[▓▓░░░░░░] 25.0%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Implement active Cattle profile CRUD operations and support image asset uploads. |
| **Tasks & Deliverables** | 1. Implement Farm Management views and services to configure independent farm facilities.<br/>2. Develop Cattle model structure to track tag IDs, age, weight, status, breed, and gender.<br/>3. Wire repository patterns for query actions.<br/>4. Set up an isolated helper service to resize, crop, and store user-uploaded cattle pictures in local storage directories. |
| **Files & Modules** | 📁 `Models/` ➔ `Farm.cs`, `Cattle.cs`, `Enums.cs` (CattleStatus)<br/>📁 `Repositories/` ➔ `FarmRepository.cs`, `CattleRepository.cs`<br/>📁 `Services/` ➔ `FarmService.cs`, `CattleService.cs`, `ImageService.cs`<br/>📁 `Controllers/` ➔ `FarmController.cs`, `CattleController.cs`<br/>📁 `Views/` ➔ `Farm/`, `Cattle/` (Index, Create, Edit, Details views) |
| **Technologies Used** | SixLabors.ImageSharp (image optimization & disk writes), generic Repository Pattern, Model validation |
| **Milestone** | Managers and Owners can register distinct physical farms, upload new cattle profiles complete with processed thumbnail photos, and filter active inventory records. |

---

### 🎯 Week 3: Daily Operations (Feeding & Milk Production)
*   **Goal**: Build daily operational logging trackers for recording individual cattle nutrition consumption and daily session-based milk yields.
*   **Progress Status**: `[▓▓▓░░░░░] 37.5%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Enable workers to log milk sessions and animal feed consumption logs in real time. |
| **Tasks & Deliverables** | 1. Develop Milk Production models and record daily morning/evening session yields.<br/>2. Create Feed Log entities to record daily food type rations (hay, grains, silage) and quantity consumed.<br/>3. Design input grids tailored to tablet sizes for quick data entry in physical barns.<br/>4. Write LINQ queries to aggregate daily production yields per animal and per farm. |
| **Files & Modules** | 📁 `Models/` ➔ `MilkProduction.cs`, `FeedRecord.cs`<br/>📁 `Repositories/` ➔ `MilkProductionRepository.cs`, `FeedRepository.cs`<br/>📁 `Services/` ➔ `MilkService.cs`, `IMilkService.cs`<br/>📁 `Controllers/` ➔ `MilkProductionController.cs`, `FeedController.cs`<br/>📁 `Views/` ➔ `MilkProduction/`, `Feed/` |
| **Technologies Used** | ASP.NET Core MVC (dynamic views), LINQ data aggregations, Repository Pattern |
| **Milestone** | Farm staff can log feeding volumes and daily milk outputs per cow, with the system calculating running production statistics. |

---

### 🎯 Week 4: Health, Treatment & Veterinary Coordination
*   **Goal**: Manage professional veterinarian directories, coordinate medical appointments, and track livestock immunizations.
*   **Progress Status**: `[▓▓▓▓░░░░] 50.0%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Centralize animal medical records, vaccination cycles, and diagnostic histories. |
| **Tasks & Deliverables** | 1. Set up doctor registration, professional specialties, and available contact schedules.<br/>2. Build Appointment scheduler linking cows, farms, and assigned doctors.<br/>3. Design prescription pads to log administered medicines, dosages, diagnostic notes, and withdrawal periods.<br/>4. Implement upcoming vaccination event logs with countdown timers for required boosters. |
| **Files & Modules** | 📁 `Models/` ➔ `Doctor.cs`, `Appointment.cs`, `HealthRecord.cs`, `MedicineRecord.cs`, `Vaccination.cs`<br/>📁 `Services/` ➔ `DoctorService.cs`, `AppointmentService.cs`, `HealthService.cs`, `VaccinationService.cs`<br/>📁 `Controllers/` ➔ `DoctorController.cs`, `AppointmentController.cs`, `HealthController.cs`<br/>📁 `Views/` ➔ `Doctor/`, `Appointment/`, `Health/` |
| **Technologies Used** | ASP.NET MVC Form Bindings, jQuery scripts, HTML appointment logs |
| **Milestone** | Managers can schedule vet appointments, and doctors can view upcoming visits, update patient diagnostic charts, and record administered vaccines. |

---

### 🎯 Week 5: Breeding Lifecycles & Gestation Tracking
*   **Goal**: Automate breeding logs, track pregnancy timelines, calculate estimated calving dates, and send proactive notifications.
*   **Progress Status**: `[▓▓▓▓▓░░░] 62.5%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Implement gestation tracking with alerts for cows nearing birth. |
| **Tasks & Deliverables** | 1. Build breeding logs detailing sire, dam, breed type, success status, and insemination dates.<br/>2. Set up gestation calendars to automatically calculate expected delivery deadlines.<br/>3. Design in-app Notification rules to flag critical events (e.g., cow entering final trimester).<br/>4. Link newly registered calves automatically to their dam in the primary inventory database. |
| **Files & Modules** | 📁 `Models/` ➔ `Breeding.cs`, `Notification.cs`<br/>📁 `Repositories/` ➔ `BreedingRepository.cs`, `NotificationRepository.cs`<br/>📁 `Services/` ➔ `NotificationService.cs`, `INotificationService.cs`<br/>📁 `Controllers/` ➔ `BreedingController.cs`, `NotificationController.cs`<br/>📁 `Views/` ➔ `Breeding/`, `Notification/` |
| **Technologies Used** | Advanced DateTime computations, in-memory event alerting patterns |
| **Milestone** | Cow pregnancy progress is tracked via a visual timeline, and automatic alerts notify farm staff when a cow is approaching calving. |

---

### 🎯 Week 6: Storefront Marketplace & Order Pipelines
*   **Goal**: Build a digital marketplace catalog for selling farm commodities and manage customer order checkout processes.
*   **Progress Status**: `[▓▓▓▓▓▓░░] 75.0%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Allow public users to purchase milk, dairy items, and other farm products online. |
| **Tasks & Deliverables** | 1. Construct product catalogs with inventory stock tracking and uploaded photos.<br/>2. Implement virtual shopping cart checkouts for public customers.<br/>3. Code the Order processing system to handle item quantities, delivery addresses, and discount fields.<br/>4. Build administrative order fulfillment pipelines (Pending ➔ Processing ➔ Shipped ➔ Completed). |
| **Files & Modules** | 📁 `Models/` ➔ `Product.cs`, `Order.cs`, `OrderItem.cs`<br/>📁 `Repositories/` ➔ `ProductRepository.cs`, `OrderRepository.cs`<br/>📁 `Services/` ➔ `ProductService.cs`, `OrderService.cs`<br/>📁 `Controllers/` ➔ `ProductController.cs`, `OrderController.cs`<br/>📁 `Views/` ➔ `Product/`, `Order/` |
| **Technologies Used** | ASP.NET Model Binding validation, AutoMapper profile definitions |
| **Milestone** | Customers can browse live products, add items to a cart, submit orders, and track fulfillment statuses updated by the administration. |

---

### 🎯 Week 7: Logistics, Transport & Payment Gateway Integration
*   **Goal**: Integrate secure credit card payments via SSLCommerz and manage delivery routes, vehicles, and driver dispatches.
*   **Progress Status**: `[▓▓▓▓▓▓▓░] 87.5%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Enable checkout transactions via SSLCommerz and dispatch logistics tracking records. |
| **Tasks & Deliverables** | 1. Integrate the `SSLCommerz` sandbox payment engine, webhooks, and status callbacks.<br/>2. Build vehicle records and driver logs with real-time status attributes.<br/>3. Program dispatch workflows: paid orders automatically request transit dispatches.<br/>4. Manage trips, tracking vehicle, assigned driver, fuel usage, and destination logs. |
| **Files & Modules** | 📁 `Models/` ➔ `Vehicle.cs`, `Driver.cs`, `Trip.cs`, `TransportRequest.cs`, `Payment.cs`<br/>📁 `Services/` ➔ `SslCommerzService.cs`, `TransportService.cs`, `IPaymentGatewayService.cs`<br/>📁 `Controllers/` ➔ `TransportController.cs`<br/>📁 `Views/` ➔ `Transport/`<br/>📄 Root ➔ `appsettings.json` (SSLCommerz fields) |
| **Technologies Used** | SSLCommerz Payment Gateway APIs, HttpClient requests, logistics assignment algorithms |
| **Milestone** | Orders can be paid for securely using card credentials, automatically generating shipping requests that assign vehicles and drivers for delivery. |

---

### 🎯 Week 8: Analytics, Auditing & Financial Dashboards
*   **Goal**: Consolidate farm activities into financial metrics, build audit trails, export Excel reports, and construct targeted role-based dashboards.
*   **Progress Status**: `[▓▓▓▓▓▓▓▓] 100%`

| Attribute | Details |
| :--- | :--- |
| **Weekly Objective** | Launch operational analytics dashboards and audit logs for farm oversight. |
| **Tasks & Deliverables** | 1. Build revenue and expense logging matrices (feed costs, wages, milk revenue, calf sales).<br/>2. Implement ClosedXML script queries to generate and export spreadsheets.<br/>3. Hook Serilog log writing streams and database `AuditLog` capture triggers.<br/>4. Construct targeted dashboards customized for specific roles (e.g., financial graphs for Owners, daily tasks for Workers). |
| **Files & Modules** | 📁 `Models/` ➔ `Revenue.cs`, `Expense.cs`, `AuditLog.cs`, `ActivityLog.cs`<br/>📁 `Services/` ➔ `DashboardService.cs`, `FinancialService.cs`, `AuditService.cs`<br/>📁 `Controllers/` ➔ `DashboardController.cs`, `FinancialController.cs`, `ReportsController.cs`<br/>📁 `Views/` ➔ `Dashboard/`, `Financial/`, `Reports/` |
| **Technologies Used** | ClosedXML Excel APIs, Chart.js canvas interfaces, Serilog file loggers |
| **Milestone** | System completion: Owners can review profit/loss graphs, generate dynamic Excel reports, and audit all critical application activities. |

---

## 📐 Project Quality Checklists

### 🧪 Automated Testing Plan
*   **Repository Tests**: Validate basic entity operations (insert, query, modify, delete) on in-memory db configurations.
*   **Authentication Tests**: Verify user session cookies are issued, expired, and enforce authorization policies correctly.
*   **Integration Tests**: Mock checkout operations, validating payment completion updates.

### 🧹 Deployment Checklist
1.  **Environment Setup**: Register SQL Server databases and apply EF Core migrations in staging environments.
2.  **Configuration Check**: Update SMTP authentication credentials and replace sandbox SSLCommerz tokens with production keys.
3.  **Filesystem Check**: Ensure physical directory folders (`uploads/cattle`, `uploads/products`, etc.) exist and have write permissions on host servers.
