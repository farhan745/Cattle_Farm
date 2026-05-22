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
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly CattleFarmDbContext _context;
        private static readonly List<TaskViewModel> _inMemoryTasks = new List<TaskViewModel>();
        private static int _nextId = 1;

        public TaskAssignmentService(CattleFarmDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskViewModel>> GetAllTasksAsync()
        {
            return await Task.FromResult(_inMemoryTasks.ToList());
        }

        public async Task<IEnumerable<TaskViewModel>> GetTasksByUserIdAsync(int userId)
        {
            var list = _inMemoryTasks.Where(x => x.AssignedUserId == userId).ToList();
            return await Task.FromResult(list);
        }

        public async Task<TaskViewModel?> GetTaskByIdAsync(int id)
        {
            var t = _inMemoryTasks.FirstOrDefault(x => x.Id == id);
            return await Task.FromResult(t);
        }

        public async Task AssignTaskAsync(TaskAssignViewModel model)
        {
            if (model.Id > 0)
            {
                var existing = _inMemoryTasks.FirstOrDefault(x => x.Id == model.Id);
                if (existing != null)
                {
                    var worker = await _context.Workers.FindAsync(model.AssignedWorkerId);
                    
                    existing.AssignedWorkerId = model.AssignedWorkerId;
                    existing.AssignedUserId = worker?.UserId ?? 0;
                    existing.WorkerName = worker?.FullName ?? string.Empty;
                    existing.Title = model.Title;
                    existing.Description = model.Description;
                    existing.DueDate = model.DueDate;
                    existing.Status = model.Status;

                    if (model.Status == "Completed" && !existing.CompletedAt.HasValue)
                    {
                        existing.CompletedAt = DateTime.UtcNow;
                    }
                    else if (model.Status != "Completed")
                    {
                        existing.CompletedAt = null;
                    }
                }
            }
            else
            {
                var worker = await _context.Workers.FindAsync(model.AssignedWorkerId);

                var task = new TaskViewModel
                {
                    Id = _nextId++,
                    AssignedWorkerId = model.AssignedWorkerId,
                    AssignedUserId = worker?.UserId ?? 0,
                    WorkerName = worker?.FullName ?? string.Empty,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    Status = model.Status ?? "Pending",
                    AssignedAt = DateTime.UtcNow,
                    CompletedAt = null
                };
                _inMemoryTasks.Add(task);
            }
        }

        public async Task DeleteTaskAsync(int id)
        {
            var t = _inMemoryTasks.FirstOrDefault(x => x.Id == id);
            if (t != null)
            {
                _inMemoryTasks.Remove(t);
            }
            await Task.CompletedTask;
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, int userId, string status)
        {
            var task = _inMemoryTasks.FirstOrDefault(x => x.Id == taskId && x.AssignedUserId == userId);
            if (task == null) return await Task.FromResult(false);

            task.Status = status;
            if (status == "Completed" && !task.CompletedAt.HasValue)
                task.CompletedAt = DateTime.UtcNow;
            else if (status != "Completed")
                task.CompletedAt = null;

            return await Task.FromResult(true);
        }
    }
}
