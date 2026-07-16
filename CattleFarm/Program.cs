using CattleFarm.Data;
using CattleFarm.Authorization;
using CattleFarm.Hubs;
using CattleFarm.Models;
using CattleFarm.Services.Background;
using CattleFarm.Services.Implementations;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// ── Serilog ───────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/cattlefarm-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

static string RequiredConfig(IConfiguration configuration, string key, bool rejectPlaceholders = false)
{
    var value = configuration[key];
    if (string.IsNullOrWhiteSpace(value))
        throw new InvalidOperationException($"Missing required configuration value '{key}'.");

    if (rejectPlaceholders &&
        (value.Contains("CHANGE_THIS", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("LOCAL_DEVELOPMENT", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("YOUR_", StringComparison.OrdinalIgnoreCase) ||
         value.Contains("configured-by-environment", StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException($"Configuration value '{key}' is still a placeholder.");
    }

    return value;
}

var jwtKey = RequiredConfig(
    builder.Configuration,
    "Jwt:Key",
    rejectPlaceholders: builder.Environment.IsProduction());
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Configuration value 'Jwt:Key' must be at least 32 bytes for HMAC-SHA256 signing.");

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContextPool<CattleFarmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));

// ── Unit of Work ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, CattleFarm.UnitOfWork.UnitOfWork>();

// ── Infrastructure Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISmsService, SmsService>();

// ── Domain Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<ICattleService, CattleService>();
builder.Services.AddScoped<IWorkerService, WorkerService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<ICattleMedicalRecordService, CattleMedicalRecordService>();
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IVaccinationService, VaccinationService>();
builder.Services.AddScoped<IMilkService, MilkService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITransportService, TransportService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
builder.Services.AddScoped<IFarmJoinService, FarmJoinService>();
builder.Services.AddScoped<IFarmAccessService, FarmAccessService>();
// ── Currency Services ─────────────────────────────────────────────────────────
builder.Services.Configure<CurrencySettings>(builder.Configuration.GetSection("CurrencySettings"));
builder.Services.AddSingleton<ICurrencyService, CurrencyService>();

// ── Email + Payment Services ──────────────────────────────────────────────────
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPaymentGatewayService, SslCommerzService>();
builder.Services.AddScoped<IPdfService, PdfService>();

builder.Services.AddHttpClient("SSLCommerz");
builder.Services.AddHostedService<SystemAlertBackgroundService>();

// ── HTTP Context Accessor (for audit logging in services) ─────────────────────
builder.Services.AddHttpContextAccessor();

// ── Cookie Authentication ─────────────────────────────────────────────────────
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "CattleFarm.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
            : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartCattleFarm",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SmartCattleFarm.Api",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// ── File Upload Size Limit (10 MB) ────────────────────────────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10_485_760; // 10 MB
});

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = ".CattleFarm.Antiforgery";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest
        : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();
builder.Services.AddSignalR();

builder.Services.AddScoped<IAuthorizationHandler, FarmOwnershipHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(FarmPolicyNames.RequireFarmOwnership, policy =>
        policy.Requirements.Add(new FarmOwnershipRequirement()));
    options.AddPolicy(FarmPolicyNames.RequireWorkerRole, policy =>
        policy.RequireRole(AppRoles.Worker));
    options.AddPolicy(FarmPolicyNames.RequireOwnerRole, policy =>
        policy.RequireRole(AppRoles.Owner, AppRoles.Admin));
});

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var app = builder.Build();

// ── Seed Database ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CattleFarmDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    await DbSeeder.SeedAsync(db, env.IsDevelopment());
    // Ensure upload folders exist
    var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
    foreach (var folder in new[] { "avatars", "cattle", "farms", "products", "workers", "doctors", "task-proofs", "licenses" })
        Directory.CreateDirectory(Path.Combine(webRoot, "uploads", folder));
}

// ── Error Handling ────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<FarmDashboardHub>("/hubs/farm-dashboard");

// ── Health Check Endpoint (used by Docker healthcheck) ────────────────────
app.MapGet("/Health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    version = System.Reflection.Assembly.GetExecutingAssembly()
                  .GetName().Version?.ToString() ?? "1.0.0"
})).AllowAnonymous();

app.Run();
