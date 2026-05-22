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
    public class EmployeeService : IEmployeeService
    {
        private readonly CattleFarmDbContext _context;

        public EmployeeService(CattleFarmDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmployeeViewModel>> GetAllEmployeesAsync()
        {
            var users = await _context.Users
                .Where(u => u.Role == AppRoles.Worker && !u.IsDeleted)
                .ToListAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var workers = await _context.Workers
                .Where(w => w.UserId.HasValue && userIds.Contains(w.UserId.Value) && !w.IsDeleted)
                .ToListAsync();

            var workerMap = workers.ToDictionary(w => w.UserId!.Value);

            var list = new List<EmployeeViewModel>();
            foreach (var u in users)
            {
                workerMap.TryGetValue(u.Id, out var w);
                list.Add(new EmployeeViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Department = w?.Notes, // Store department in Notes
                    Designation = w?.Role ?? "Worker",
                    BaseSalary = w?.Salary ?? 0,
                    IsActive = u.IsActive,
                    HiredAt = w?.HiredAt ?? u.CreatedAt,
                    Role = u.Role
                });
            }
            return list;
        }

        public async Task<EmployeeViewModel?> GetEmployeeByIdAsync(int id)
        {
            var u = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == AppRoles.Worker && !u.IsDeleted);

            if (u == null) return null;

            var w = await _context.Workers
                .FirstOrDefaultAsync(w => w.UserId == id && !w.IsDeleted);

            return new EmployeeViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Department = w?.Notes,
                Designation = w?.Role ?? "Worker",
                BaseSalary = w?.Salary ?? 0,
                IsActive = u.IsActive,
                HiredAt = w?.HiredAt ?? u.CreatedAt,
                Role = u.Role
            };
        }

        public async Task CreateEmployeeAsync(EmployeeCreateViewModel model)
        {
            // 1. Create User
            var user = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role ?? AppRoles.Worker,
                PhoneNumber = model.PhoneNumber,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync(); // Get user.Id

            // 2. Determine FarmId — use selected farm, or fall back to first active farm
            int farmId;
            if (model.FarmId.HasValue && model.FarmId.Value > 0)
            {
                farmId = model.FarmId.Value;
            }
            else
            {
                var defaultFarm = await _context.Farms.FirstOrDefaultAsync(f => f.IsActive && !f.IsDeleted);
                if (defaultFarm == null) return; // No farm available — skip Worker record
                farmId = defaultFarm.Id;
            }

            // 3. Create Worker record
            var worker = new Worker
            {
                FarmId = farmId,
                FullName = model.FullName,
                Role = model.Designation ?? "Worker",
                Phone = model.PhoneNumber,
                Email = model.Email,
                Salary = model.BaseSalary,
                Notes = model.Department,
                IsActive = true,
                UserId = user.Id,
                HiredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Workers.AddAsync(worker);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(EmployeeEditViewModel model)
        {
            var user = await _context.Users.FindAsync(model.Id);
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.IsActive = model.IsActive;
                user.Role = model.Role;
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
            }

            var worker = await _context.Workers.FirstOrDefaultAsync(w => w.UserId == model.Id);
            if (worker != null)
            {
                worker.FullName = model.FullName;
                worker.Role = model.Designation ?? "Worker";
                worker.Phone = model.PhoneNumber;
                worker.Email = model.Email;
                worker.Salary = model.BaseSalary;
                worker.Notes = model.Department;
                worker.IsActive = model.IsActive;
                worker.UpdatedAt = DateTime.UtcNow;
                _context.Workers.Update(worker);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                _context.Users.Update(user);
            }

            var worker = await _context.Workers.FirstOrDefaultAsync(w => w.UserId == id);
            if (worker != null)
            {
                worker.IsDeleted = true;
                worker.DeletedAt = DateTime.UtcNow;
                _context.Workers.Update(worker);
            }

            await _context.SaveChangesAsync();
        }
    }
}
