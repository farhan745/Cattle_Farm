# Technology Stack

**Analysis Date:** 2026-06-01

## Languages

**Primary:**
- C# 10 / .NET 10.0 - All backend application code, including controllers, services, repositories, database models, Hubs, and DTOs.

**Secondary:**
- HTML5 / Razor (CSHTML) - Frontend user interface structure and views.
- JavaScript (ES6+) - Client-side logic, real-time SignalR notifications, Chart.js integrations, and page interactions.
- Vanilla CSS3 - Application stylesheet (`wwwroot/css/site.css`), dynamic animations, layouts (sidebar-overlay, flex box), and custom styling.

## Runtime

**Environment:**
- ASP.NET Core 10.0 (LTS) Runtime
- Target Framework: `net10.0`

**Package Manager:**
- NuGet - Used for managing C# external dependencies and frameworks.
- Dependencies listed in [CattleFarm.csproj](file:///f:/VisualStudio/CattleFarm/CattleFarm/CattleFarm.csproj).

## Frameworks

**Core:**
- ASP.NET Core MVC (Model-View-Controller) - Core web server, routing, Razor page rendering, and API endpoints.
- ASP.NET Core SignalR - Real-time client-server communication used for dashboard sync.

**Testing:**
- None - No automated test frameworks (xUnit, NUnit, MSTest) configured or files present.

**Build/Dev:**
- MSBuild / .NET SDK 10.0 - Build and development platform compilation.
- Razor Runtime Compilation (`Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation`) - Enables hot reload for Razor views during local development.

## Key Dependencies

**Critical:**
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.5) - Entity Framework Core provider for Microsoft SQL Server.
- `AutoMapper` (16.1.1) - Object-to-object mapping library for DTOs and database models.
- `BCrypt.Net-Next` (4.2.0) - Standard secure hashing algorithm for user password encryption.
- `MailKit` (4.8.0) - SMTP email delivery client library.
- `ClosedXML` (0.102.3) - Excel workbook generation for financials and reports.
- `SixLabors.ImageSharp` (3.1.5) - Image validation and resizing client library for uploaded media.

**Infrastructure:**
- `Serilog.AspNetCore` (8.0.3) - Structured diagnostic logging provider.
- `Serilog.Sinks.File` (5.0.0) - File-based rolling logging sink.
- `Microsoft.AspNetCore.Authentication.Cookies` - Built-in cookie authentication middleware.

## Configuration

**Environment:**
- Configured in [appsettings.json](file:///f:/VisualStudio/CattleFarm/CattleFarm/appsettings.json) and [appsettings.Development.json](file:///f:/VisualStudio/CattleFarm/CattleFarm/appsettings.Development.json).
- Key configs include SQL Server `DefaultConnection`, SMTP parameters (`Email`), payment gateway keys (`SSLCommerz`), and system localization (`CurrencySettings`).

**Build:**
- `CattleFarm.csproj` - MSBuild project file outlining target framework and NuGet dependencies.
- `CattleFarm.slnx` - New XML-based Visual Studio Solution structure mapping project locations.

## Platform Requirements

**Development:**
- Windows/macOS/Linux running .NET 10 SDK.
- Local instance of Microsoft SQL Server (e.g. LocalDB or SQLEXPRESS).
- IDE supporting C# 10 development (e.g. Visual Studio 2022, JetBrains Rider, VS Code).

**Production:**
- Any environment supporting .NET 10.0 runtime (Windows IIS, Linux/Docker, Azure App Service, AWS ECS).
- Microsoft SQL Server (2019+ or Azure SQL).

---

*Stack analysis: 2026-06-01*
*Update after major dependency changes*
