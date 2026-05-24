using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CattleFarm.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFinancialService _fin;
        private readonly ITaskAssignmentService _tasks;
        private readonly CattleFarmDbContext _db;

        public DashboardService(IUnitOfWork uow, IFinancialService fin, ITaskAssignmentService tasks, CattleFarmDbContext db)
        {
            _uow   = uow;
            _fin   = fin;
            _tasks = tasks;
            _db    = db;
        }

        public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
        {
            var users          = await _uow.Users.GetAllAsync();
            var farms          = await _uow.Farms.GetAllAsync();
            var cattles        = await _uow.Cattles.GetAllAsync();
            var workers        = await _uow.Workers.GetAllAsync();
            var doctors        = await _uow.Doctors.GetAllAsync();
            var recentActivity = await _uow.ActivityLogs.GetRecentAsync(10);
            var auditLogs      = (await _uow.AuditLogs.GetPagedAsync(1, 10)).Items;

            var today    = DateTime.UtcNow.Date;
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var now      = DateTime.UtcNow;

            var allRevenues = (await _uow.Revenues.GetAllAsync()).Where(r => !r.IsDeleted).ToList();
            var allExpenses = (await _uow.Expenses.GetAllAsync()).Where(e => !e.IsDeleted).ToList();
            var allMilk = (await _uow.MilkProductions.GetAllAsync()).ToList();

            var revenue = allRevenues.Where(r => r.Date >= monthStart && r.Date <= now).Sum(r => r.Amount);
            var expenses = allExpenses.Where(e => e.Date >= monthStart && e.Date <= now).Sum(e => e.Amount);
            var netProfit = revenue - expenses;

            var activeCount   = cattles.Count(c => c.Status == CattleStatus.Active && c.HealthStatus == HealthStatus.Healthy);
            var sickCount     = cattles.Count(c => c.HealthStatus is HealthStatus.Sick or HealthStatus.Critical);
            var atRiskCount   = cattles.Count(c => c.HealthStatus == HealthStatus.AtRisk);
            var soldCount     = cattles.Count(c => c.Status == CattleStatus.Sold);
            var deceasedCount = cattles.Count(c => c.Status == CattleStatus.Deceased);

            var milkTrend = new List<DailyMilkTrend>();
            for (int i = 6; i >= 0; i--)
            {
                var day     = today.AddDays(-i);
                var dayStart = day;
                var dayEnd   = day.AddDays(1);
                var dayYield = allMilk.Where(m => m.Date >= dayStart && m.Date < dayEnd).Sum(m => m.MorningYieldLiters + m.EveningYieldLiters);
                milkTrend.Add(new DailyMilkTrend
                {
                    Day    = day.ToString("ddd"),
                    Liters = dayYield
                });
            }

            var trend = new List<MonthlyTrendItem>();
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var from = new DateTime(date.Year, date.Month, 1);
                var to   = from.AddMonths(1).AddDays(-1);
                var rev  = allRevenues.Where(r => r.Date >= from && r.Date <= to).Sum(r => r.Amount);
                var exp  = allExpenses.Where(e => e.Date >= from && e.Date <= to).Sum(e => e.Amount);
                trend.Add(new MonthlyTrendItem { Month = from.ToString("MMM yyyy"), Revenue = rev, Expense = exp });
            }

            return new AdminDashboardViewModel
            {
                TotalUsers      = users.Count(),
                TotalFarms      = farms.Count(),
                TotalCattle     = cattles.Count(),
                TotalWorkers    = workers.Count(),
                TotalDoctors    = doctors.Count(),
                PendingFarms    = farms.Count(f => f.ApprovalStatus == ApprovalStatus.Pending),
                RecentActivity  = recentActivity.ToList(),
                RecentAuditLogs = auditLogs.ToList(),
                TotalRevenue    = revenue,
                TotalExpenses   = expenses,
                NetProfit       = netProfit,
                ActiveCount     = activeCount,
                SickCount       = sickCount + atRiskCount,
                SoldCount       = soldCount,
                DeceasedCount   = deceasedCount,
                MilkWeeklyTrend = milkTrend,
                MonthlyTrend    = trend,
                Farms           = farms.ToList()
            };
        }

        public async Task<OwnerDashboardViewModel> GetOwnerDashboardAsync(int ownerId, int? farmId = null)
        {
            var farms = (await _uow.Farms.GetByOwnerIdAsync(ownerId)).ToList();
            var selectedFarm = farmId.HasValue
                ? farms.FirstOrDefault(f => f.Id == farmId.Value)
                : farms.FirstOrDefault();

            if (selectedFarm is null)
                return new OwnerDashboardViewModel { Farms = farms };

            var fid      = selectedFarm.Id;
            var cattle   = (await _uow.Cattles.GetByFarmIdAsync(fid)).ToList();
            var workers  = (await _uow.Workers.GetByFarmIdAsync(fid)).ToList();
            var today    = DateTime.UtcNow.Date;
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var now      = DateTime.UtcNow;

            // Financial
            var milkToday  = await _uow.MilkProductions.GetTotalYieldByFarmAsync(fid, today, today.AddDays(1));
            var netProfit  = await _fin.GetNetProfitAsync(fid, monthStart, now);
            var revenue    = await _fin.GetTotalRevenueAsync(fid, monthStart, now);
            var expenses   = await _fin.GetTotalExpensesAsync(fid, monthStart, now);
            var trend      = await _fin.GetMonthlyTrendAsync(fid, 6);

            // Health
            var highRisk    = (await _uow.HealthRecords.GetHighRiskAsync(fid)).ToList();
            var upcoming    = (await _uow.Vaccinations.GetUpcomingAsync(30)).ToList();
            var appointments = (await _uow.Appointments.GetByFarmIdAsync(fid)).ToList();

            // 7-day milk trend
            var milkTrend = new List<DailyMilkTrend>();
            for (int i = 6; i >= 0; i--)
            {
                var day     = today.AddDays(-i);
                var dayYield = await _uow.MilkProductions.GetTotalYieldByFarmAsync(fid, day, day.AddDays(1));
                milkTrend.Add(new DailyMilkTrend
                {
                    Day    = day.ToString("ddd"),
                    Liters = dayYield
                });
            }

            // Top 5 producers (30-day window)
            var thirtyDaysAgo = today.AddDays(-30);
            var allMilk = (await _uow.MilkProductions.GetByFarmIdAsync(fid))
                .Where(m => m.Date >= thirtyDaysAgo)
                .ToList();

            var topProducers = allMilk
                .GroupBy(m => m.CattleId)
                .Select(g =>
                {
                    var cow = cattle.FirstOrDefault(c => c.Id == g.Key);
                    var days = (g.Max(m => m.Date) - g.Min(m => m.Date)).Days + 1;
                    return new TopProducerItem
                    {
                        TagId          = cow?.TagId ?? g.Key.ToString(),
                        Name           = cow?.Name  ?? "Unknown",
                        Breed          = cow?.Breed ?? "-",
                        DailyAvgLiters = days > 0 ? g.Sum(m => m.TotalYieldLiters) / days : 0,
                        HealthStatus   = cow?.HealthStatus ?? HealthStatus.Healthy
                    };
                })
                .OrderByDescending(x => x.DailyAvgLiters)
                .Take(5)
                .ToList();

            // Cattle status counts
            var activeCount   = cattle.Count(c => c.Status == CattleStatus.Active && c.HealthStatus == HealthStatus.Healthy);
            var sickCount     = cattle.Count(c => c.HealthStatus is HealthStatus.Sick or HealthStatus.Critical);
            var atRiskCount   = cattle.Count(c => c.HealthStatus == HealthStatus.AtRisk);
            var soldCount     = cattle.Count(c => c.Status == CattleStatus.Sold);
            var deceasedCount = cattle.Count(c => c.Status == CattleStatus.Deceased);

            // Profitability
            var totalMilkLiters = (double)(allMilk.Sum(m => m.TotalYieldLiters));
            var costPerLiter = (totalMilkLiters > 0 && expenses > 0)
                ? expenses / (decimal)totalMilkLiters : 0m;
            var roi = expenses > 0 ? (netProfit / expenses) * 100m : 0m;

            // High-risk cattle objects
            var highRiskCattle = highRisk
                .Select(h => h.Cattle)
                .Where(c => c != null)
                .Distinct()
                .Take(5)
                .ToList()!;

            return new OwnerDashboardViewModel
            {
                TotalFarms           = farms.Count,
                TotalCattle          = cattle.Count,
                ActiveCattle         = cattle.Count(c => c.Status == CattleStatus.Active),
                SickCattle           = sickCount,
                TotalWorkers         = workers.Count,
                MilkTodayLiters      = milkToday,
                NetProfit            = netProfit,
                TotalRevenue         = revenue,
                TotalExpenses        = expenses,
                UpcomingAppointments = appointments.Count(a => a.ScheduledAt >= now && a.Status == AppointmentStatus.Scheduled),
                HighRiskCattle       = highRiskCattle!,
                HighRiskCount        = highRiskCattle!.Count,
                UpcomingVaccinations = upcoming.Take(5).ToList(),
                VaccinationDueCount  = upcoming.Count,
                MilkWeeklyTrend      = milkTrend,
                MonthlyTrend         = trend,
                TopProducers         = topProducers,
                ActiveCount          = activeCount,
                SickCount            = sickCount + atRiskCount,
                SoldCount            = soldCount,
                DeceasedCount        = deceasedCount,
                CostPerLiter         = costPerLiter,
                Roi                  = roi,
                Farms                = farms,
                SelectedFarm         = selectedFarm
            };
        }

        public async Task<WorkerDashboardViewModel> GetWorkerDashboardAsync(int userId)
        {
            var worker = await _db.Workers
                .Include(w => w.Farm)
                .FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
            var membership = await _db.FarmWorkers
                .Include(fw => fw.Farm)
                .FirstOrDefaultAsync(fw => fw.WorkerUserId == userId && fw.IsActive && !fw.IsDeleted);
            var farmId = worker?.FarmId ?? membership?.FarmId;
            var milkLogs     = await _uow.MilkProductions.GetByFarmIdAsync(farmId ?? 0);
            var myTasks      = (await _tasks.GetTasksByUserIdAsync(userId)).ToList();
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var presentDays  = 0;
            if (worker != null)
            {
                presentDays = await _db.Set<Attendance>()
                    .CountAsync(a => a.WorkerId == worker.Id && a.Date >= startOfMonth && a.Status == "Present");
            }
            return new WorkerDashboardViewModel
            {
                WorkerProfile    = worker,
                MyFarmId         = farmId,
                MyFarmName       = worker?.Farm?.Name ?? membership?.Farm?.Name,
                RecentMilkLogs   = milkLogs.Take(10).ToList(),
                PresentThisMonth = presentDays,
                MyTasks          = myTasks,
                PendingTasks     = myTasks.Count(t => t.Status == "Pending"),
                InProgressTasks  = myTasks.Count(t => t.Status == "InProgress"),
                CompletedTasks   = myTasks.Count(t => t.Status == "Completed")
            };
        }

        public async Task<DoctorDashboardViewModel> GetDoctorDashboardAsync(int doctorId)
        {
            var doctor = await _uow.Doctors.GetByIdAsync(doctorId);
            var appts  = await _uow.Appointments.GetByDoctorIdAsync(doctorId);
            var health = await _uow.HealthRecords.GetPagedAsync(1, 10, null, null);
            return new DoctorDashboardViewModel
            {
                DoctorProfile        = doctor,
                TotalAppointments    = appts.Count(),
                TodayAppointments    = appts.Count(a => a.ScheduledAt.Date == DateTime.UtcNow.Date),
                UpcomingAppointments = appts.Where(a => a.ScheduledAt >= DateTime.UtcNow && a.Status == AppointmentStatus.Scheduled).Take(5).ToList(),
                RecentHealthRecords  = health.Items.ToList()
            };
        }

        public async Task<CustomerDashboardViewModel> GetCustomerDashboardAsync(int customerId)
        {
            var orders   = await _uow.Orders.GetByCustomerIdAsync(customerId);
            // Show all available products on the website (not just featured ones)
            var products = await _db.Products
                .Include(p => p.Farm)
                .Where(p => p.IsAvailable && !p.IsDeleted)
                .OrderByDescending(p => p.IsFeatured)
                .ThenBy(p => p.Name)
                .Take(8).ToListAsync();
            return new CustomerDashboardViewModel
            {
                RecentOrders     = orders.Take(5).ToList(),
                TotalOrders      = orders.Count(),
                TotalSpent       = orders.Where(o => o.PaymentStatus == PaymentStatus.Completed).Sum(o => o.TotalAmount),
                FeaturedProducts = products
            };
        }
    }
}
