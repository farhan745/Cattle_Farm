# Codebase Concerns

**Analysis Date:** 2026-06-01

## Tech Debt

**Zero Automated Test Coverage:**
- Issue: The solution contains no unit, integration, or E2E tests whatsoever.
- Why: Focus on rapid delivery of functional features and views.
- Impact: Introducing changes or refactoring core business logic (e.g. transport bookings, payroll processing, farm join requests) is highly error-prone and could lead to silent database regressions or accounting flaws.
- Fix approach: Set up xUnit test suites and require comprehensive test coverage for domain services, starting with critical calculations in `PayrollService` and `SslCommerzService`.

**Hardcoded Secrets in Source Control:**
- Issue: Real SMTP email credentials and payment gateway details are hardcoded directly in [appsettings.json](file:///f:/VisualStudio/CattleFarm/CattleFarm/appsettings.json).
- Why: Ease of configuration and rapid testing on dev machines.
- Impact: Exposing these credentials (e.g. Google App Password `lwxh gyey odqo qtqr` and SSLCommerz StorePassword) in public or shared git repositories compromise the system's security.
- Fix approach: Remove all active secrets from `appsettings.json`, swap them with environment variables or utilize ASP.NET Core User Secrets (`dotnet user-secrets`) during local development.

## Security Considerations

**Hardcoded Credentials:**
- Risk: Compromised accounts, unauthorized email delivery via Gmail SMTP, and potential fraudulent transactions or redirection bypasses on SSLCommerz.
- Current mitigation: Sandbox environment mode set to true for payment gateway, but SMTP utilizes real Google App Passwords.
- Recommendations: Migrate credentials to environment variables immediately. Add `.env` support or configure deployment environment secrets in IIS/Azure dashboards.

**Local File Storage and Size Limits:**
- Risk: Program.cs allows a massive 50MB file size limit (`FormOptions.MultipartBodyLengthLimit = 52_428_800`). Uploading massive uncompressed media directly to server disk exposes the application to Disk Denial of Service (DoS) attacks.
- Current mitigation: Images are processed by ImageSharp in `ImageService`, but raw files must still be buffered to local disk.
- Recommendations: Set realistic request size thresholds (e.g. 5MB for images), block non-image extensions in profile upload fields, and transition file storage away from local disk to highly scalable object stores like AWS S3 or Azure Blob Storage.

## Performance Bottlenecks

**Synchronous DB Seeding on Server Boot:**
- Problem: [Program.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Program.cs) executes async DB migration and seeding (`DbSeeder.SeedAsync(...)`) blocks the main server thread at startup before mapping Kestrel routing endpoints.
- Cause: Simple seeding mechanism embedded directly in the server boot pipeline.
- Impact: Slower startup times and potential connection timeouts when the SQL Server instance is slow or cold-starting.
- Improvement path: Decouple database migrations and seed operations into a separate CLI tool, or run seeding in a background task after server startup.

**Local Image Resizing Overhead:**
- Problem: Resizing large image uploads synchronously.
- Cause: [ImageService.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Services/Implementations/ImageService.cs) uses `SixLabors.ImageSharp` to process, validate, and resize uploaded profile pictures on the fly.
- Impact: High CPU usage under concurrent upload operations, potentially causing thread pool exhaustion on modest server VMs.
- Improvement path: Offload image resizing tasks to serverless background functions or use a content delivery network (CDN) with dynamic resizing capabilities.

## Fragile Areas

**SignalR Hub Group Join Mechanisms:**
- File: [FarmDashboardHub.cs](file:///f:/VisualStudio/CattleFarm/CattleFarm/Hubs/FarmDashboardHub.cs)
- Why fragile: Joining client groups (`JoinUserGroup(userId)` and `JoinFarmGroup(farmId)`) relies on arbitrary client parameters without validating that the authenticated user actually owns or is associated with the passed `farmId` or `userId`.
- Common failures: A compromised user could listen in on real-time SignalR notifications for any farm by simply invoking `JoinFarmGroup(victimFarmId)` on the client connection.
- Safe modification: Check user claims in the hub methods before calling `Groups.AddToGroupAsync`.

---

*Concerns audit: 2026-06-01*
*Update as issues are fixed or new ones discovered*
