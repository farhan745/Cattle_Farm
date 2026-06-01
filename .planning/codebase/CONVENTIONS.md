# Coding Conventions

**Analysis Date:** 2026-06-01

## Naming Patterns

**Files:**
- **C# Classes:** PascalCase (e.g. `CattleController.cs`, `AuthService.cs`, `CattleFarmDbContext.cs`).
- **Razor Pages:** PascalCase (e.g. `Drivers.cshtml`, `CreateRequest.cshtml`).
- **Web Resources:** lowercase and kebab-case for CSS, JS, and uploads (e.g. `site.css`, `site.js`).

**Methods & Functions:**
- **Methods:** PascalCase (e.g. `GetDisplayName()`, `InitiatePaymentAsync()`).
- **Async Suffix:** Async operations always terminate with the `Async` suffix (e.g., `ValidatePaymentAsync()`, `HandleRequirementAsync()`).
- **Handlers:** `Handle[Action]` or `On[Event]` format for request actions or authorization handlers (e.g. `HandleRequirementAsync`).

**Variables:**
- **Local variables & Parameters:** camelCase (e.g. `user`, `isPersistent`, `sessionKey`, `amount`).
- **Private Fields:** camelCase prefixed with an underscore (e.g. `_db`, `_config`, `_http`, `_logger`).
- **Constants:** UPPER_SNAKE_CASE or PascalCase for public constants (e.g. `RequireFarmOwnership`, `DefaultConnection`).

**Types (Interfaces, Classes, Enums):**
- **Interfaces:** PascalCase prefixed with `I` (e.g. `IUnitOfWork`, `IAuthService`, `IDriverRepository`).
- **Classes & Structs:** PascalCase (e.g. `AppRoles`, `FarmOwnershipHandler`).
- **Enums:** PascalCase for names and values (e.g. `AppRoles.Worker`, `AppRoles.Owner`).

## Code Style

**Formatting:**
- **Braces Layout:** Standard Allman style where braces open on a new line.
  ```csharp
  if (ownsFarm)
  {
      context.Succeed(requirement);
  }
  ```
- **Indentation:** 4 spaces (standard C# default).
- **File Structure:** C# block namespaces are preferred:
  ```csharp
  namespace CattleFarm.Authorization
  {
      // classes here
  }
  ```

**Import Organization:**
- **Order:**
  1. System and Microsoft base namespaces (`System`, `System.Text.Json`).
  2. ASP.NET Core framework namespaces (`Microsoft.AspNetCore.Mvc`).
  3. Third-party library namespaces (`Serilog`, `MailKit`, `AutoMapper`).
  4. Project-specific namespaces (`CattleFarm.Models`, `CattleFarm.Services.Interfaces`).

## Error Handling

**Patterns:**
- **Exceptions:** Catch specific exception types at service boundaries and log the trace before returning failed response DTOs or bubbling custom exceptions.
- **Async try/catch:** Standard try-catch blocks wrap API queries, HTTP client integrations, and database operations.
  ```csharp
  try
  {
      var response = await _http.PostAsync(baseUrl, new FormUrlEncodedContent(form));
      // process success
  }
  catch (Exception ex)
  {
      _logger.LogError(ex, "SSLCommerz initiation failed");
      return new PaymentInitResponse { Success = false, Error = ex.Message };
  }
  ```

## Logging

**Framework:**
- `Microsoft.Extensions.Logging.ILogger<T>` injected via Constructor DI.
- Static Serilog logger `Log` used at boot startup.

**Patterns:**
- **Structured Logging:** Use parameterised messages rather than string interpolation for correct indexing:
  ```csharp
  _logger.LogDebug("SSLCommerz response: {Content}", content);
  ```
- **Levels:** `LogDebug` for request/response bodies, `LogInformation` for standard events, `LogError` for exceptions.

## Comments

**Documentation:**
- **XML Comments:** XML `/// <summary>` tags are used to document services and interfaces:
  ```csharp
  /// <summary>
  /// Integrates with SSLCommerz payment gateway via REST API.
  /// Uses sandbox mode when IsSandbox=true in appsettings.
  /// </summary>
  ```
- **Visual Section Dividers:** Comments with long hyphens or divider symbols block off logical sections in major boot files (like `Program.cs`):
  ```csharp
  // ── Cookie Authentication ─────────────────────────────────────────────────────
  ```

---

*Convention analysis: 2026-06-01*
*Update when patterns change*
