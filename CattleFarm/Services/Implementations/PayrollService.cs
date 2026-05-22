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
    public class PayrollService : IPayrollService
    {
        private readonly CattleFarmDbContext _context;
        private static readonly List<PayrollViewModel> _inMemoryPayrolls = new List<PayrollViewModel>();
        private static int _nextId = 1;

        public PayrollService(CattleFarmDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PayrollViewModel>> GetAllPayrollsAsync()
        {
            return await Task.FromResult(_inMemoryPayrolls.ToList());
        }

        public async Task<PayrollViewModel?> GetPayrollByIdAsync(int id)
        {
            var p = _inMemoryPayrolls.FirstOrDefault(x => x.Id == id);
            return await Task.FromResult(p);
        }

        public async Task<IEnumerable<PayrollViewModel>> GetPayrollsByUserIdAsync(int userId)
        {
            var list = _inMemoryPayrolls.Where(x => x.UserId == userId).ToList();
            return await Task.FromResult(list);
        }

        public async Task<IEnumerable<PayrollViewModel>> GetPayrollsByFarmIdsAsync(IEnumerable<int> farmIds)
        {
            var hashSet = farmIds.ToHashSet();
            var list = _inMemoryPayrolls.Where(x => hashSet.Contains(x.FarmId)).ToList();
            return await Task.FromResult(list);
        }

        public async Task GenerateMonthlyPayrollAsync(int year, int month)
        {
            var workers = await _context.Workers
                .Where(w => w.IsActive && !w.IsDeleted)
                .ToListAsync();

            foreach (var w in workers)
            {
                var exists = _inMemoryPayrolls.Any(x => x.WorkerId == w.Id && x.Year == year && x.Month == month);
                if (exists) continue;

                var baseSalary = w.Salary;
                var netSalary = baseSalary;

                var p = new PayrollViewModel
                {
                    Id = _nextId++,
                    UserId = w.UserId ?? 0,
                    WorkerId = w.Id,
                    WorkerName = w.FullName,
                    FarmId = w.FarmId,
                    Year = year,
                    Month = month,
                    OvertimeHours = 0,
                    BaseSalary = baseSalary,
                    OvertimePay = 0,
                    Deductions = 0,
                    Bonus = 0,
                    NetSalary = netSalary,
                    IsPaid = false,
                    GeneratedAt = DateTime.UtcNow
                };
                _inMemoryPayrolls.Add(p);
            }
        }

        public async Task UpdatePayrollAsync(PayrollEditViewModel model)
        {
            var p = _inMemoryPayrolls.FirstOrDefault(x => x.Id == model.Id);
            if (p != null)
            {
                p.OvertimeHours = model.OvertimeHours;
                p.OvertimePay = model.OvertimePay;
                p.Deductions = model.Deductions;
                p.Bonus = model.Bonus;
                p.NetSalary = model.NetSalary;
                p.IsPaid = model.IsPaid;
            }
            await Task.CompletedTask;
        }

        public async Task DeletePayrollAsync(int id)
        {
            var p = _inMemoryPayrolls.FirstOrDefault(x => x.Id == id);
            if (p != null)
            {
                _inMemoryPayrolls.Remove(p);
            }
            await Task.CompletedTask;
        }
    }
}
