# Architecture

**Analysis Date:** 2026-06-01

## Pattern Overview

**Overall:** Layered Monolithic MVC (Model-View-Controller) application with Repository and Unit of Work patterns.

**Key Characteristics:**
- **Monolithic Architecture** - All layers (UI, business logic, data access) are housed within a single project compile target.
- **Unit of Work & Repository Pattern** - Decouples domain logic from database access, ensuring transactional integrity.
- **Role-based & Policy-based Security** - Advanced custom handlers for farm ownership checks and staff access.
- **Real-Time Synchronization** - Built-in ASP.NET Core SignalR hub syncing live updates to farm dashboards.

## Layers

**Presentation Layer (Controllers & Views):**
- Purpose: Handle incoming HTTP requests, orchestrate views, validate user inputs, and parse query payloads.
- Contains: ASP.NET Controllers under [Controllers](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers) and Razor Page Views under [Views](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views).
- Depends on: Service Layer for business orchestration, ViewModels for view data schemas.
- Used by: User Client browsers (via routing mappings defined in `Program.cs`).

**Service Layer (Domain Logic):**
- Purpose: Execute business rules, handle data calculations, orchestrate workflows, and manage transactional borders.
- Contains: Interfaces under [Services/Interfaces](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Interfaces) and concrete Service Implementations under [Services/Implementations](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations).
- Depends on: Unit of Work for data storage actions, external clients (Email, Payment) for utility.
- Used by: Controllers.

**Unit of Work & Repository Layer (Data Abstraction):**
- Purpose: Abstract data querying operations and group database updates into unified transactions.
- Contains: [IUnitOfWork](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/IUnitOfWork.cs), [UnitOfWork](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/UnitOfWork.cs), and repositories under [Repositories](file:///f:/VisualStudio/CattleFarm/CattleFarm/Repositories).
- Depends on: EF Core DbContext and database models.
- Used by: Service Layer.

**Data Access & Model Layer (Entities):**
- Purpose: Define core entity schemas and direct relationships mapped directly to database tables.
- Contains: Entities under [Models](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models) and the primary DbContext [CattleFarmDbContext](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/CattleFarmDbContext.cs).
- Depends on: System and EF Core frameworks.
- Used by: Repositories, Unit of Work, and AutoMapper DTO configs.

## Data Flow

**Standard Web Request / Controller Invocation:**

1. User clicks "Drivers" on the Transport Hub page.
2. Web browser requests: `GET /Transport/Drivers`
3. Routing engine maps URL to [TransportController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/TransportController.cs) -> `Drivers()` action.
4. Controller verifies user identity. If authorised, calls `ITransportService.GetDriversAsync(farmId)`.
5. [TransportService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/TransportService.cs) coordinates with the Unit of Work: `_unitOfWork.Drivers.GetAllAsync()`.
6. Repository returns domain entities. Service applies business rules and returns structured models/DTOs.
7. Controller binds the service results to a ViewModel and forwards it to `Views/Transport/Drivers.cshtml`.
8. Razor engine renders the HTML page server-side.
9. Browser receives the HTML and displays the interactive UI.

**State Management:**
- **Stateless Requests:** No transient state is stored in memory between individual HTTP requests.
- **Persistence:** All farm, cattle, staff, and financial records reside persistently in Microsoft SQL Server.
- **User Sessions:** User state (such as full name, profile image, current roles) is securely parsed from the encrypted Cookie Authentication header during each request.

## Key Abstractions

**IUnitOfWork:**
- Purpose: Exposes repositories (e.g. `Cattle`, `Farms`, `Tasks`) and facilitates transactional commits (`CompleteAsync()`).
- Pattern: Unit of Work, coordinating transactions across multiple repositories.
- Implementation: [UnitOfWork.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/UnitOfWork.cs).

**Repository (e.g. IDriverRepository):**
- Purpose: Encapsulates specific database query logic and entity transformations.
- Pattern: Repository Pattern.
- Example: `DriverRepository` implementing `IDriverRepository`.

**Domain Service (e.g. ITaskAssignmentService):**
- Purpose: Houses complex business algorithms like task completion verification and proof validation.
- Example: `TaskAssignmentService` implementing `ITaskAssignmentService`.

## Entry Points

**Web Host Startup:**
- Location: [Program.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Program.cs)
- Triggers: Application boot (server startup).
- Responsibilities: Registers all dependencies, sets up DB connection pool, binds authentication cookies, enables SignalR hubs, builds the ASP.NET Core middleware pipeline, and seeds initial data.

## Error Handling

**Strategy:** Clean bubble-up exceptions with centralized request pipeline handlers.

**Patterns:**
- Local try/catch blocks within services to capture and log errors with contextual detail using Serilog.
- centralized exception handling middleware mapping errors to `/Home/Error` in production environments, showing a clean user view without exposing stack traces.
- DeveloperExceptionPage activated automatically when running in local development mode.

## Cross-Cutting Concerns

**Logging:**
- Powered by Serilog. Log configurations are built at start and write structured telemetry data to standard Console and rolling daily files inside the `/logs` directory.

**Validation:**
- UI & Model constraints configured directly via C# DataAnnotations (e.g., `[Required]`, `[StringLength]`) on ViewModels. ASP.NET MVC model-binder checks `ModelState.IsValid` before handling actions.

**Authentication & Security:**
- Built-in Cookie Authentication manages sessions. Claims are compiled at login via [UserClaimsHelper.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Authorization/UserClaimsHelper.cs).
- Policy-based authorization controls access (e.g., `FarmOwnershipRequirement` evaluated by [FarmOwnershipHandler.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Authorization/FarmAuthorizationPolicies.cs)).

---

*Architecture analysis: 2026-06-01*
*Update when major patterns change*
