# Testing Patterns

**Analysis Date:** 2026-06-01

## Test Framework

**Current Status:**
- **No Automated Tests Configured:** There are currently no automated unit tests, integration tests, or end-to-end (E2E) tests present in the codebase.
- **Solution Mapping:** The Visual Studio Solution maps only the primary web application project [CattleFarm.csproj](file:///f:/VisualStudio/CattleFarm/CattleFarm/CattleFarm.csproj). No test projects exist.
- **Run Commands:** Running the standard test command returns no tests:
  ```bash
  dotnet test
  ```

## Target Testing Strategy (Proposed)

To build robust test coverage in the future, the following frameworks and structures should be adopted:

### 1. Test Project Structure
We propose introducing two test projects to the solution:
1. **CattleFarm.Tests.Unit** - For testing isolated business logic services, helper functions, and custom authorization rules in isolation.
2. **CattleFarm.Tests.Integration** - For testing database operations (using an in-memory SQL SQLite driver or Respawn), repositories, and API-controller integrations.

**Proposed Directory structure:**
```
[project-root]/
├── CattleFarm/                 # Application codebase
├── CattleFarm.Tests.Unit/      # Unit test project (.csproj)
│   ├── Services/               # Tests for core domain services
│   └── Helpers/                # Tests for claim and view utilities
└── CattleFarm.Tests.Integration/ # Integration test project (.csproj)
    ├── Controllers/            # Web application factory integration tests
    └── Repositories/           # Database integration tests
```

### 2. Proposed Frameworks & Tools

- **Runner:** **xUnit** (v2.x) - Standard lightweight test runner for .NET core applications.
- **Assertion Library:** **FluentAssertions** - Encourages natural, human-readable assertion formats:
  ```csharp
  result.Success.Should().BeTrue();
  result.GatewayUrl.Should().NotBeNullOrWhiteSpace();
  ```
- **Mocking Library:** **Moq** or **NSubstitute** - Mocking dependent interfaces (like `IUnitOfWork`, `IEmailService`, `IConfiguration`) passed to services.
- **Integration Test Server:** `Microsoft.AspNetCore.Mvc.Testing` - Facilitates bootstrap of a test server using `WebApplicationFactory` to fire actual HTTP clients against controller endpoints.

### 3. Proposed Unit Testing Pattern

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using CattleFarm.Services.Implementations;

public class SslCommerzServiceTests
{
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IHttpClientFactory> _httpFactoryMock;
    private readonly Mock<ILogger<SslCommerzService>> _loggerMock;

    public SslCommerzServiceTests()
    {
        _configMock = new Mock<IConfiguration>();
        _httpFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<SslCommerzService>>();
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        // (Configure mock behaviors and setup the service instance)
        var service = new SslCommerzService(_configMock.Object, _httpFactoryMock.Object, _loggerMock.Object);
        var request = new PaymentInitRequest { Amount = 100.00m, Currency = "BDT" };

        // Act
        var result = await service.InitiatePaymentAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
}
```

### 4. Proposed Mocking Targets

- **External APIs:** Stub out `SslCommerzService` during E2E flow testing to prevent hitting the real Sandbox URL.
- **Email Delivery:** Mock `IEmailService` or `MailKit` SMTP execution during password reset and veterinarian registration testing to bypass real SMTP network checks.
- **Database Context:** Mock repository layers when testing complex domain logic service methods (like `PayrollService.CalculatePayrollAsync`), but use real test DB contexts for repository-specific data integration checks.

---

*Testing analysis: 2026-06-01*
*Update when test projects are introduced*
