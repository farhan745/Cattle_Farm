# 🚜 Smart Cattle Farm — System Architecture & Hub Guide
### 📂 Workspace Location: `f:\VisualStudio\CattleFarm`

> [!NOTE]
> Welcome to the **Smart Cattle Farm** master workspace documentation. This guide maps out the database, business logic, presentation controllers, and client assets of this ASP.NET Core 10.0 MVC platform.

---

## 📐 System Architecture Flowchart
The application utilizes an N-tier clean architecture pattern. The diagram below illustrates how a request flows through each layer:

```mermaid
graph TD
    Client["🌐 Client Web Browser"] 
    Controllers["🎮 Controllers Layer<br/>(Request Routing & Actions)"]
    ViewModels["📋 ViewModels Layer<br/>(Data Validation & DTOs)"]
    Services["⚙️ Services Layer<br/>(Business Logic & Rules)"]
    UnitOfWork["🤝 Unit of Work Layer<br/>(Transaction Boundaries)"]
    Repositories["📦 Repositories Layer<br/>(Data Query Abstraction)"]
    DbContext["💾 DbContext & Models<br/>(EF Core ORM)"]
    SQLServer[("🗄️ SQL Server Database<br/>(Physical Tables)"]
    ExternalAPIs["⚡ External Gateways<br/>(SSLCommerz & MailKit)"]
    Views["🖥️ Razor Views<br/>(Dynamic UI Generation)"]
    wwwroot["🌐 Client Assets & Media<br/>(CSS, JS, Uploads)"]

    %% Flow lines
    Client -->|1. HTTP Request| Controllers
    Controllers -->|2. Binds Data| ViewModels
    Controllers -->|3. Invokes Operations| Services
    Services -->|4. Coordinates DB Tasks| UnitOfWork
    UnitOfWork -->|5. Queries & Updates| Repositories
    Repositories -->|6. Manipulates Entities| DbContext
    DbContext -->|7. Persists Changes| SQLServer
    Services -->|8. Integrates API Requests| ExternalAPIs
    Controllers -->|9. Returns View Context| Views
    Views -->|10. Ingests Assets| wwwroot
    Views -->|11. Dynamic HTML Output| Client
```

---

## 📁 Workspace Directory Map & Layer Responsibilities

Here is the architectural function of each directory in this project. Click on the folder names or guide links to explore their specific documentation:

| Directory Path | Architectural Layer | Primary Function | Documentation Guide |
| :--- | :--- | :--- | :--- |
| 📁 [CattleFarm/Controllers](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers) | **Presentation (Routing & Action)** | Intercepts HTTP requests, handles session cookies, checks permissions, and maps data payload onto services and views. | [🎮 Controllers Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/README.md) |
| 📁 [CattleFarm/Models](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models) | **Domain Models & DbContext** | Represents database schemas, entities (Cattle, Farms, Orders), relationships, and state enums. | [📂 Models Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/README.md) |
| 📁 [CattleFarm/Services](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services) | **Business Logic Layer** | Implements core validation rules, computes operational metrics, and connects external API clients. | [⚙️ Services Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/README.md) |
| 📁 [CattleFarm/Repositories](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories) | **Data Query Layer** | Standardizes and isolates EF Core database queries using generic and custom repository patterns. | [📦 Repositories Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories/README.md) |
| 📁 [CattleFarm/UnitOfWork](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork) | **Transaction Boundary** | Manages repository transactions under a single DB connection session to protect data integrity. | [🤝 Unit of Work Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/README.md) |
| 📁 [CattleFarm/ViewModels](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels) | **DTO & Input Validation** | Safeguards domain model schemas by mapping forms inputs to security-checked binding schemas. | [📋 ViewModels Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/ViewModels/README.md) |
| 📁 [CattleFarm/Views](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views) | **User Interface (Razor Templates)** | Renders HTML elements, styles, forms, and charts dynamically based on user context. | [🖥️ Views Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/README.md) |
| 📁 [CattleFarm/Data](file:///f:/VisualStudio/CattleFarm/CattleFarm/Data) | **Seed Configurations** | Installs system security roles, baseline administrators, and mock store logs during database boot. | [💾 Data Seeding Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/Data/README.md) |
| 📁 [CattleFarm/Migrations](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations) | **Database Versioning** | Translates model C# property additions into incremental SQL database update scripts. | [🚀 Migrations Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations/README.md) |
| 📁 [CattleFarm/wwwroot](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot) | **Static Files & Media Storage** | Delivers compiled site stylesheets, Javascript libraries, and hosts physical upload folders for photos. | [🌐 Static Assets Guide](file:///f:/VisualStudio/CattleFarm/CattleFarm/wwwroot/README.md) |
| 📁 [SQL Server Scripts1](file:///f:/VisualStudio/CattleFarm/SQL Server Scripts1) | **Database Management** | Contains native `.sql` seeding scripts for direct execution inside SQL Server Management Studio (SSMS). | [🗄️ SQL Scripts Guide](file:///f:/VisualStudio/CattleFarm/SQL%20Server%20Scripts1/README.md) |

---

## 📅 Chronological Phase Milestones

This project structure directly correlates with an 8-week system build-out:

```
[ Week 1: Identity & Security ] ──▶ [ Week 2: Inventory & Facilities ] ──▶ [ Week 3: Production Logs ] ──▶ [ Week 4: Vet Care & Checkups ]
                                                                                                            │
[ Week 8: Analytics & Serilog ] ◀── [ Week 7: Payments & Dispatches ] ◀── [ Week 6: E-Store Checkout ] ◀── [ Week 5: breeding Schedules ]
```

### 🗓️ Weekly Milestones Table

| Phase | Goal | Key Modules & Files | Target Deliverable |
| :--- | :--- | :--- | :--- |
| **Week 1** | Authentication & Security | `AccountController`, `User`, `AuthService`, `DbSeeder` | Secure role-based cookie authentication for Admin, Owner, Manager, and Customer. |
| **Week 2** | Livestock Inventory | `FarmController`, `CattleController`, `ImageService` | Farm creation cards and cattle records with auto-cropped image thumbnails. |
| **Week 3** | Daily Production | `FeedController`, `MilkProductionController` | Tablet-optimized data-entry grids for registering milk yields and feed quantities. |
| **Week 4** | Veterinary Care | `DoctorController`, `AppointmentController`, `HealthRecord` | Clinical records tracking diagnostic history, treatment logs, and vaccine schedule countdowns. |
| **Week 5** | reproduction Calendar | `BreedingController`, `NotificationService` | Pregnancy calendars showing expected calving dates and automated status alerts. |
| **Week 6** | E-Store Commerce | `ProductController`, `OrderController`, `OrderItem` | Dynamic product listings with stock levels and multi-item shopping carts. |
| **Week 7** | Logistics & Payments | `SslCommerzService`, `TransportController`, `Trip` | SSLCommerz payment validation with vehicle driver dispatch routes. |
| **Week 8** | Financial Dashboard | `DashboardController`, `FinancialService`, `ReportsController` | ClosedXML spreadsheet generation, database audit logging, and performance dashboard charts. |

---

> [!TIP]
> **System Seeding Credentials**: Standard login parameters have been set up for system analysis. You can check the credentials for each role inside the [gmail.md](file:///f:/VisualStudio/CattleFarm/CattleFarm/gmail.md) file.
