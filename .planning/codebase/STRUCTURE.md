# Codebase Structure

**Analysis Date:** 2026-06-01

## Directory Layout

```
CattleFarm/
├── .agent/                 # Agent workspace capabilities & GSD skills
├── .planning/              # Project planning, state, and codebase maps
├── get-shit-done/          # GSD internal workflows, templates, and references
├── CattleFarm/             # Primary C# ASP.NET Core MVC web application
│   ├── Authorization/      # Custom authorization policies and handler implementations
│   ├── Controllers/        # HTTP Controllers handling request routing
│   ├── Data/               # Database seeder scripts and initial database state
│   ├── Hubs/               # SignalR Hubs for real-time dashboard sync
│   ├── Migrations/         # EF Core migrations tracking schema versioning
│   ├── Models/             # Database context (DbContext) and entity models
│   ├── Properties/         # Visual Studio deployment configurations (launchSettings.json)
│   ├── Repositories/       # Data-access repository layers (interfaces & implementations)
│   ├── Services/           # Domain and infrastructure service logic (interfaces & implementations)
│   ├── UnitOfWork/         # Unit of Work pattern interfaces and implementations
│   ├── ViewModels/         # Input binding models and View Transfer Objects (DTOs)
│   ├── Views/              # Razor view templates rendered server-side (.cshtml)
│   │   ├── Shared/         # Shared layouts, footers, and partial components
│   │   └── [Controller]/   # Action-specific Razor views grouped by controller
│   └── wwwroot/            # Static public web assets (js, css, and image uploads)
│       ├── css/            # Vanilla application stylesheets (site.css)
│       ├── js/             # Frontend script files (site.js)
│       └── uploads/        # Uploaded avatars, cattle media, and certificates
├── CattleFarm.slnx         # XML-based Visual Studio Solution structure mapping
├── PROJECT_GUIDE.md        # Reference guide outlining CattleFarm components
└── README.md               # Quick-start setup instructions
```

## Directory Purposes

**CattleFarm/Controllers/:**
- Purpose: HTTP request processors routing inputs and returning view structures.
- Contains: `*Controller.cs` files (e.g. `CattleController.cs`, `TransportController.cs`).
- Key files: [AccountController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/AccountController.cs) for logins, [CattleController.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Controllers/CattleController.cs) for inventory.

**CattleFarm/Services/:**
- Purpose: House core business logical execution.
- Contains: Separate directories for interfaces and implementations (`Services/Interfaces/` and `Services/Implementations/`).
- Key files: [FarmJoinService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/FarmJoinService.cs), [TaskAssignmentService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/TaskAssignmentService.cs).

**CattleFarm/Repositories/:**
- Purpose: Decouple database query composition and raw entity loading from business workflows.
- Contains: Interface files under `Interfaces/` and EF implementations under `Implementations/`.

**CattleFarm/Models/:**
- Purpose: Represent underlying physical tables and map relationships.
- Contains: Database context definition and schema C# class structures.
- Key files: [CattleFarmDbContext.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Models/CattleFarmDbContext.cs).

**CattleFarm/Views/:**
- Purpose: Standard markup views displaying dynamic information to browser clients.
- Contains: HTML templates structured in subdirectories named after their respective Controllers.
- Key files: [Views/Shared/_Layout.cshtml](file:///f:/VisualStudio/CattleFarm/CattleFarm/Views/Shared/_Layout.cshtml).

**CattleFarm/wwwroot/:**
- Purpose: Deliver client resources and house user uploads.
- Contains: Static scripts, standard stylesheets, icon sets, and folder-mapped uploads (`uploads/`).

## Key File Locations

**Entry Points:**
- [CattleFarm/Program.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Program.cs) - Main application bootstrapper.

**Configuration:**
- [CattleFarm/appsettings.json](file:///f:/VisualStudio/CattleFarm/CattleFarm/appsettings.json) - Environment configurations, connection strings, credentials.
- [CattleFarm/CattleFarm.csproj](file:///f:/VisualStudio/CattleFarm/CattleFarm/CattleFarm.csproj) - Main project definitions, MSBuild commands, NuGet targets.
- [CattleFarm.slnx](file:///f:/VisualStudio/CattleFarm/CattleFarm.slnx) - IDE solution layout.

**Core Logic:**
- [CattleFarm/UnitOfWork/UnitOfWork.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/UnitOfWork/UnitOfWork.cs) - DB Transaction orchestrator.

## Naming Conventions

**Files:**
- `*Controller.cs` - Controller files.
- `I*Repository.cs` / `*Repository.cs` - Repository interfaces and implementations.
- `I*Service.cs` / `*Service.cs` - Service interfaces and implementations.
- `*ViewModel.cs` - DTO classes binding Razor Views.
- `*.cshtml` - Razor pages.
- `*.cs` - Models, Hubs, and general utilities in PascalCase.

**Directories:**
- PascalCase for all C# package namespace directories (e.g. `ViewModels`, `UnitOfWork`).
- lowercase for frontend web assets (e.g. `wwwroot/css`, `wwwroot/js`, `wwwroot/uploads`).

## Where to Add New Code

**New Business Module/Feature:**
1. **Model:** Declare in `Models/` (e.g., `NewEntity.cs`) and register inside `CattleFarmDbContext.cs`.
2. **Migration:** Run `dotnet ef migrations add AddNewEntity` and apply.
3. **Repository:** Declare `INewEntityRepository.cs` under `Repositories/Interfaces/` and implement `NewEntityRepository.cs` under `Repositories/Implementations/`. Expose it inside `IUnitOfWork` and bind inside `UnitOfWork.cs`.
4. **Service:** Declare `INewEntityService.cs` and implement `NewEntityService.cs` under `Services/`. Register the scoped mapping in `Program.cs`.
5. **ViewModels & UI:** Build input binding schemas under `ViewModels/`. Build Controller under `Controllers/` and create folder with respective Razor pages (`.cshtml`) inside `Views/NewEntity/`.

---

*Structure analysis: 2026-06-01*
*Update when directory structure changes*
