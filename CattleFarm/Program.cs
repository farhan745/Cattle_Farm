using CattleFarm.Data;
using CattleFarm.Models;
using CattleFarm.Services.Implementations;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ── Serilog ───────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/cattlefarm-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<CattleFarmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Unit of Work ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUnitOfWork, CattleFarm.UnitOfWork.UnitOfWork>();

// ── Infrastructure Services ───────────────────────────────────────────────────
builder.Services.AddScoped<IImageService,        ImageService>();
builder.Services.AddScoped<IAuditService,        AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ── Domain Services ───────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService,            AuthService>();
builder.Services.AddScoped<IUserManagementService,  UserManagementService>();
builder.Services.AddScoped<IFarmService,            FarmService>();
builder.Services.AddScoped<ICattleService,          CattleService>();
builder.Services.AddScoped<IWorkerService,          WorkerService>();
builder.Services.AddScoped<IDoctorService,          DoctorService>();
builder.Services.AddScoped<IHealthService,          HealthService>();
builder.Services.AddScoped<IVaccinationService,     VaccinationService>();
builder.Services.AddScoped<IMilkService,            MilkService>();
builder.Services.AddScoped<IProductService,         ProductService>();
builder.Services.AddScoped<IOrderService,           OrderService>();
builder.Services.AddScoped<IFinancialService,       FinancialService>();
builder.Services.AddScoped<ISubscriptionService,    SubscriptionService>();
builder.Services.AddScoped<IAppointmentService,     AppointmentService>();
builder.Services.AddScoped<IDashboardService,       DashboardService>();

// ── Email + Payment Services ───────────────────────────────────────
builder.Services.AddScoped<IEmailService,           EmailService>();
builder.Services.AddScoped<IPaymentGatewayService,  SslCommerzService>();
builder.Services.AddHttpClient("SSLCommerz");

// ── HTTP Context Accessor (for audit logging in services) ─────────────────────
builder.Services.AddHttpContextAccessor();

// ── Cookie Authentication ─────────────────────────────────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Account/Login";
        options.LogoutPath       = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name      = "CattleFarm.Auth";
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        options.SlidingExpiration   = true;
        options.ExpireTimeSpan      = TimeSpan.FromHours(8);
    });

// ── File Upload Size Limit (50 MB) ────────────────────────────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 52_428_800; // 50 MB
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Seed Database ─────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db  = scope.ServiceProvider.GetRequiredService<CattleFarmDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    await DbSeeder.SeedAsync(db);
    // Ensure upload folders exist
    foreach (var folder in new[] { "avatars", "cattle", "farms", "products" })
        Directory.CreateDirectory(Path.Combine(env.WebRootPath, "uploads", folder));
}

// ── Error Handling ────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
