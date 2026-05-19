using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.UnitOfWork;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFinancialService _fin;

        public DashboardService(IUnitOfWork uow, IFinancialService fin)
        {
            _uow = uow;
            _fin = fin;
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

            return new AdminDashboardViewModel
            {
                TotalUsers      = users.Count(),
                TotalFarms      = farms.Count(),
                TotalCattle     = cattles.Count(),
                TotalWorkers    = workers.Count(),
                TotalDoctors    = doctors.Count(),
                PendingFarms    = farms.Count(f => f.ApprovalStatus == ApprovalStatus.Pending),
                RecentActivity  = recentActivity.ToList(),
                RecentAuditLogs = auditLogs.ToList()
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

        public async Task<WorkerDashboardViewModel> GetWorkerDashboardAsync(int workerId)
        {
            var worker   = await _uow.Workers.GetByIdAsync(workerId);
            var milkLogs = await _uow.MilkProductions.GetByFarmIdAsync(worker?.FarmId ?? 0);
            return new WorkerDashboardViewModel
            {
                WorkerProfile   = worker,
                RecentMilkLogs  = milkLogs.Take(10).ToList()
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
            var featured = (await _uow.Products.GetAllAsync())
                .Where(p => p.IsFeatured && p.IsAvailable)
                .Take(8).ToList();
            return new CustomerDashboardViewModel
            {
                RecentOrders     = orders.Take(5).ToList(),
                TotalOrders      = orders.Count(),
                TotalSpent       = orders.Where(o => o.PaymentStatus == PaymentStatus.Completed).Sum(o => o.TotalAmount),
                FeaturedProducts = featured
            };
        }
    }
}
