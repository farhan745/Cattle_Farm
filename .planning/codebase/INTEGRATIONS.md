# External Integrations

**Analysis Date:** 2026-06-01

## APIs & External Services

**Payment Processing:**
- **SSLCommerz** - Primary payment gateway used for managing premium subscription billing and orders.
  - SDK/Client: Integrated via standard `HttpClient` (Named Client `"SSLCommerz"` registered in [Program.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Program.cs)).
  - Auth: Credentials stored in the `SSLCommerz` configuration block: `StoreId`, `StorePassword` (currently using sandbox credentials).
  - Endpoints used:
    - Initiator Url: Configured in `BaseUrl` parameter (`https://sandbox.sslcommerz.com/gwprocess/v4/api.php` for sandbox).
    - Validator Url: Configured in `ValidationUrl` parameter (`https://sandbox.sslcommerz.com/validator/api/validationserverAPI.php` for sandbox).
  - Implementation: [SslCommerzService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/SslCommerzService.cs).

**Email:**
- **Gmail / SMTP** - Delivery of system notifications, veterinarian invitations, and account password resets.
  - SDK/Client: `MailKit` library (via SMTP protocol).
  - Auth: Host (`smtp.gmail.com`), Port (`587`), Username (`farhanzawad187@gmail.com`), and Password stored in the `Email` section of configuration.
  - Implementation: [EmailService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/EmailService.cs).

## Data Storage

**Databases:**
- **Microsoft SQL Server** - Primary database store for cattle data, farm entities, user roles, logs, products, and financial ledgers.
  - Connection: Connection string defined under `ConnectionStrings:DefaultConnection` in `appsettings.json`.
  - Client: Entity Framework Core (`Microsoft.EntityFrameworkCore.SqlServer` 10.0.5) ORM.
  - Migrations: Database schema versioning managed via Entity Framework Core Migrations under the [Migrations](file:///f:/VisualStudio/CattleFarm/CattleFarm/Migrations) directory.

**File Storage:**
- **Local File System** - Storage for avatars, cattle images, product media, tasks proof, and veterinarian licenses.
  - Location: Under the standard web root directory `wwwroot/uploads/` (including subfolders `avatars/`, `cattle/`, `farms/`, `products/`, `workers/`, `doctors/`, `task-proofs/`, `licenses/`).
  - Creation: Directory folders are auto-created at server startup within [Program.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Program.cs) via `DbSeeder.SeedAsync`.
  - Client: Standard ASP.NET Core `IWebHostEnvironment` and standard system `Directory.CreateDirectory`.

## Authentication & Identity

**Auth Provider:**
- **ASP.NET Core Cookie Authentication** - Locally managed user authentication utilizing claims-based cookies.
  - Cookie Settings:
    - Name: `CattleFarm.Auth`
    - Paths: `/Account/Login`, `/Account/Logout`, `/Account/AccessDenied`
    - Security: `HttpOnly = true`, `SecurePolicy = CookieSecurePolicy.SameAsRequest`, `SlidingExpiration = true`, `ExpireTimeSpan = 8 hours`.
  - Claims Assembly: Defined in [UserClaimsHelper.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Authorization/UserClaimsHelper.cs) which loads user ID, FullName, Email, Role, and ProfileImage.

**CSRF/Antiforgery Protection:**
- **ASP.NET Core Antiforgery Middleware**
  - Cookie Name: `.CattleFarm.Antiforgery`
  - Header Name: `X-CSRF-TOKEN` for API/Ajax calls.

## Monitoring & Observability

**Logs:**
- **Serilog** - Used for diagnostic, auditing, and server-side log capturing.
  - Sinks: Captures logs to both the System Console and rolling files under the `logs/` directory.
  - File Naming: `logs/cattlefarm-.log` with daily rotation and a 30-day retention limit.

## CI/CD & Deployment

**Hosting:**
- Runs as a standard self-contained web service executable.
- Can be hosted via Kestrel behind IIS, Nginx, or directly deployed as a Docker container.

## Webhooks & Callbacks

**Incoming Callbacks:**
- **SSLCommerz IPN (Instant Payment Notification)** - Callback endpoints registered during payment initialization.
  - Success callback: `/Subscription/Success` or `/Order/Success`
  - Fail callback: `/Subscription/Fail` or `/Order/Fail`
  - Cancel callback: `/Subscription/Cancel` or `/Order/Cancel`
  - IPN callback: `/Subscription/Ipn` or `/Order/Ipn`

---

*Integration audit: 2026-06-01*
*Update when adding/removing external services*
