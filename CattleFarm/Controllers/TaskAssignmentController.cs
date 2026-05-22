using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CattleFarm.Models;
using System.Linq;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class TaskAssignmentController : Controller
    {
        private readonly ITaskAssignmentService _taskService;
        private readonly CattleFarmDbContext _context;

        public TaskAssignmentController(ITaskAssignmentService taskService, CattleFarmDbContext context)
        {
            _taskService = taskService;
            _context = context;
        }

        private async Task PopulateWorkersViewBagAsync()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            string userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var query = _context.Workers.Where(w => w.IsActive && !w.IsDeleted);

            if (userRole != AppRoles.Admin)
            {
                query = query.Where(w => w.Farm != null && w.Farm.OwnerId == currentUserId);
            }

            ViewBag.Workers = await query.ToListAsync();
        }

        // GET: TaskAssignment
        [Authorize(Roles = AppRoles.AdminManagerOrOwner + "," + AppRoles.Worker)]
        public async Task<IActionResult> Index()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);

            if (User.IsInRole(AppRoles.Worker))
            {
                // Workers automatically filtered to their own tasks
                var workerTasks = await _taskService.GetTasksByUserIdAsync(currentUserId);
                return View(workerTasks);
            }

            var allTasks = await _taskService.GetAllTasksAsync();
            return View(allTasks);
        }

        // GET: TaskAssignment/Details/{id}
        [Authorize(Roles = AppRoles.AdminManagerOrOwner + "," + AppRoles.Worker)]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Ownership check: Workers can only view tasks assigned to them
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            if (User.IsInRole(AppRoles.Worker) && task.AssignedUserId != currentUserId)
            {
                return Forbid();
            }

            return View(task);
        }

        // GET: TaskAssignment/Assign
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Assign()
        {
            await PopulateWorkersViewBagAsync();
            return View(new TaskAssignViewModel());
        }

        // POST: TaskAssignment/Assign
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Assign(TaskAssignViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateWorkersViewBagAsync();
                return View(model);
            }

            await _taskService.AssignTaskAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // GET: TaskAssignment/Edit/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            var editModel = new TaskAssignViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                AssignedWorkerId = task.AssignedWorkerId,
                DueDate = task.DueDate,
                Status = task.Status
            };

            await PopulateWorkersViewBagAsync();
            return View(editModel);
        }

        // POST: TaskAssignment/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Edit(int id, TaskAssignViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PopulateWorkersViewBagAsync();
                return View(model);
            }

            await _taskService.AssignTaskAsync(model); // Re-uses assign handler for updates
            return RedirectToAction(nameof(Index));
        }

        // GET: TaskAssignment/Delete/{id}
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: TaskAssignment/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.AdminOrOwner)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // POST: TaskAssignment/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> UpdateStatus(int taskId, string status)
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId);
            var success = await _taskService.UpdateTaskStatusAsync(taskId, currentUserId, status);
            if (!success)
            {
                TempData["ErrorMessage"] = "Failed to update task status. Make sure the task is assigned to you.";
                return RedirectToAction(nameof(Index));
            }
            TempData["SuccessMessage"] = $"Task status updated to '{status}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
