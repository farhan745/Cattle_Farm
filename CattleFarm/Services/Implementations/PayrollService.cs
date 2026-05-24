using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Implementations
{
    /// <summary>
    /// Database-backed payroll service.
    /// Owner sees only their farm(s) payroll.
    /// Owner with multiple farms sees total across all their farms.
    /// Admin sees everything.
    /// </summary>
    public class PayrollService : IPayrollService
    {
        private readonly CattleFarmDbContext _context;

        public PayrollService(CattleFarmDbContext context)
        {
            _context = context;
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public async Task<IEnumerable<PayrollViewModel>> GetAllPayrollsAsync()
        {
            return await _context.Payrolls
                .Where(p => !p.IsDeleted)
                .Include(p => p.Worker)
                .Include(p => p.Farm)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .Select(p => MapToViewModel(p))
                .ToListAsync();
        }

        public async Task<PayrollViewModel?> GetPayrollByIdAsync(int id)
        {
            var p = await _context.Payrolls
                .Include(p => p.Worker)
                .Include(p => p.Farm)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return p == null ? null : MapToViewModel(p);
        }

        public async Task<IEnumerable<PayrollViewModel>> GetPayrollsByUserIdAsync(int userId)
        {
            return await _context.Payrolls
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Include(p => p.Worker)
                .Include(p => p.Farm)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .Select(p => MapToViewModel(p))
                .ToListAsync();
        }

        /// <summary>
        /// Returns payrolls for the given farm IDs only.
        /// Used by Owner role — sees only their own farm(s).
        /// If owner has multiple farms, all are included (total shown in view).
        /// </summary>
        public async Task<IEnumerable<PayrollViewModel>> GetPayrollsByFarmIdsAsync(IEnumerable<int> farmIds)
        {
            var ids = farmIds.ToHashSet();
            return await _context.Payrolls
                .Where(p => !p.IsDeleted && ids.Contains(p.FarmId))
                .Include(p => p.Worker)
                .Include(p => p.Farm)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .Select(p => MapToViewModel(p))
                .ToListAsync();
        }

        // ── Generate ──────────────────────────────────────────────────────────

        public async Task GenerateMonthlyPayrollAsync(int year, int month)
        {
            var workers = await _context.Workers
                .Where(w => w.IsActive && !w.IsDeleted && w.FarmId.HasValue)
                .ToListAsync();

            foreach (var w in workers)
            {
                var farmId = w.FarmId.GetValueOrDefault();

                // Skip if already generated for this month
                var exists = await _context.Payrolls.AnyAsync(
                    p => p.WorkerId == w.Id && p.Year == year && p.Month == month && !p.IsDeleted);
                if (exists) continue;

                var netSalary = w.Salary;

                var payroll = new Payroll
                {
                    WorkerId      = w.Id,
                    UserId        = w.UserId ?? 0,
                    FarmId        = farmId,
                    Year          = year,
                    Month         = month,
                    BaseSalary    = w.Salary,
                    OvertimePay   = 0,
                    OvertimeHours = 0,
                    Bonus         = 0,
                    Deductions    = 0,
                    NetSalary     = netSalary,
                    IsPaid        = false,
                    GeneratedAt   = DateTime.UtcNow
                };

                await _context.Payrolls.AddAsync(payroll);
                await SyncSalaryHistoryAsync(payroll, w);
            }

            await _context.SaveChangesAsync();
        }

        // ── Update ────────────────────────────────────────────────────────────

        public async Task UpdatePayrollAsync(PayrollEditViewModel model)
        {
            var p = await _context.Payrolls
                .Include(x => x.Worker)
                .FirstOrDefaultAsync(x => x.Id == model.Id && !x.IsDeleted);
            if (p == null) return;

            p.OvertimeHours = model.OvertimeHours;
            p.OvertimePay   = model.OvertimePay;
            p.Deductions    = model.Deductions;
            p.Bonus         = model.Bonus;
            p.NetSalary     = model.NetSalary;
            p.IsPaid        = model.IsPaid;
            p.UpdatedAt     = DateTime.UtcNow;

            if (model.IsPaid && !p.PaidAt.HasValue)
                p.PaidAt = DateTime.UtcNow;
            else if (!model.IsPaid)
                p.PaidAt = null;

            await SyncSalaryHistoryAsync(p, p.Worker);
            await _context.SaveChangesAsync();
        }

        // ── Delete ────────────────────────────────────────────────────────────

        public async Task DeletePayrollAsync(int id)
        {
            var p = await _context.Payrolls
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (p == null) return;

            p.IsDeleted = true;
            p.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // ── Mapper ────────────────────────────────────────────────────────────

        private static PayrollViewModel MapToViewModel(Payroll p) => new PayrollViewModel
        {
            Id            = p.Id,
            WorkerId      = p.WorkerId,
            UserId        = p.UserId,
            FarmId        = p.FarmId,
            FarmName      = p.Farm?.Name ?? string.Empty,
            WorkerName    = p.Worker?.FullName ?? string.Empty,
            Year          = p.Year,
            Month         = p.Month,
            BaseSalary    = p.BaseSalary,
            OvertimePay   = p.OvertimePay,
            OvertimeHours = p.OvertimeHours,
            Bonus         = p.Bonus,
            Deductions    = p.Deductions,
            NetSalary     = p.NetSalary,
            IsPaid        = p.IsPaid,
            PaidAt        = p.PaidAt,
            GeneratedAt   = p.GeneratedAt
        };

        private async Task SyncSalaryHistoryAsync(Payroll payroll, Worker? worker)
        {
            var workerUserId = payroll.UserId > 0 ? payroll.UserId : worker?.UserId ?? 0;
            var history = await _context.SalaryHistories
                .FirstOrDefaultAsync(s => s.WorkerId == payroll.WorkerId &&
                                          s.Year == payroll.Year &&
                                          s.Month == payroll.Month);

            if (history == null)
            {
                history = new SalaryHistory
                {
                    FarmId = payroll.FarmId,
                    WorkerId = payroll.WorkerId,
                    WorkerUserId = workerUserId,
                    Year = payroll.Year,
                    Month = payroll.Month,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.SalaryHistories.AddAsync(history);
            }

            history.FarmId = payroll.FarmId;
            history.WorkerUserId = workerUserId;
            history.BaseSalary = payroll.BaseSalary;
            history.Bonus = payroll.Bonus + payroll.OvertimePay;
            history.TotalSalary = payroll.BaseSalary + payroll.Bonus + payroll.OvertimePay;
        }
    }
}
