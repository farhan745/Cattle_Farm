using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;

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
            _context     = context;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int CurrentUserId()
        {
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id);
            return id;
        }

        private bool IsStaffManager =>
            User.IsInRole(AppRoles.Admin)   ||
            User.IsInRole(AppRoles.Owner)   ||
            User.IsInRole(AppRoles.Manager);

        private async Task PopulateFarmsViewBagAsync()
        {
            int userId   = CurrentUserId();
            string role  = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var query = _context.Farms.Where(f => f.IsActive && !f.IsDeleted);

            if (role != AppRoles.Admin)
                query = query.Where(f => f.OwnerId == userId);

            ViewBag.Farms = await query.OrderBy(f => f.Name).ToListAsync();
        }

        // ── GET: /TaskAssignment ──────────────────────────────────────────────
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner},{AppRoles.Manager},{AppRoles.Worker}")]
        public async Task<IActionResult> Index()
        {
            int userId = CurrentUserId();

            if (User.IsInRole(AppRoles.Worker))
            {
                var workerTasks = await _taskService.GetTasksByUserIdAsync(userId);
                return View(workerTasks);
            }

            var allTasks = await _taskService.GetAllTasksAsync();
            if (!User.IsInRole(AppRoles.Admin))
            {
                allTasks = allTasks.Where(t => t.CreatedBy == userId);
            }
            return View(allTasks);
        }

        // ── GET: /TaskAssignment/Details/{id} ─────────────────────────────────
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner},{AppRoles.Manager},{AppRoles.Worker}")]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            int userId = CurrentUserId();
            if (User.IsInRole(AppRoles.Worker) && task.AssignedUserId != userId)
                return Forbid();

            if (!User.IsInRole(AppRoles.Admin) &&
                !User.IsInRole(AppRoles.Worker) &&
                task.CreatedBy != userId)
                return Forbid();

            return View(task);
        }

        // ── GET: /TaskAssignment/OpenBoard ────────────────────────────────────
        /// <summary>Worker board showing open tasks they can claim.</summary>
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> OpenBoard()
        {
            int userId = CurrentUserId();

            var worker = await _context.Workers
                .FirstOrDefaultAsync(w => w.UserId == userId && w.IsActive && !w.IsDeleted);

            if (worker == null)
            {
                TempData["ErrorMessage"] = "Worker profile not found. Please contact your farm owner.";
                return RedirectToAction(nameof(Index));
            }

            if (!worker.FarmId.HasValue)
            {
                TempData["ErrorMessage"] = "You are not assigned to a farm yet. Apply to a farm first.";
                return RedirectToAction("Browse", "FarmJoin");
            }

            var vm = new OpenTaskBoardViewModel
            {
                OpenTasks        = await _taskService.GetOpenTasksAsync(worker.FarmId.Value),
                MyActiveTasks    = (await _taskService.GetTasksByUserIdAsync(userId))
                                        .Where(t => t.Status != Models.TaskStatus.Completed &&
                                                    t.Status != Models.TaskStatus.Expired),
                MyCompletedTasks = (await _taskService.GetTasksByUserIdAsync(userId))
                                        .Where(t => t.Status == Models.TaskStatus.Completed)
                                        .Take(10)
            };

            return View(vm);
        }

        // ── POST: /TaskAssignment/AcceptTask ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> AcceptTask(int taskId)
        {
            int userId  = CurrentUserId();
            bool success = await _taskService.AcceptTaskAsync(taskId, userId);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Task accepted! Get to work 💪"
                : "Sorry, this task was already claimed or has expired.";

            return RedirectToAction(nameof(OpenBoard));
        }

        // ── GET: /TaskAssignment/SubmitProof/{taskId} ─────────────────────────
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> SubmitProof(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            int userId = CurrentUserId();
            if (task.AssignedUserId != userId) return Forbid();

            if (task.Status == Models.TaskStatus.Completed ||
                task.Status == Models.TaskStatus.ProofSubmitted)
            {
                TempData["InfoMessage"] = "Proof already submitted for this task.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Task = task;
            return View(new SubmitProofViewModel { TaskId = id });
        }

        // ── POST: /TaskAssignment/SubmitProof ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> SubmitProof(SubmitProofViewModel model)
        {
            int userId = CurrentUserId();

            if (!ModelState.IsValid)
            {
                ViewBag.Task = await _taskService.GetTaskByIdAsync(model.TaskId);
                return View(model);
            }

            bool success = await _taskService.SubmitProofAsync(model, userId);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Proof submitted! Waiting for owner review."
                : "Failed to submit proof. Please try again.";

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /TaskAssignment/ReviewProofs ─────────────────────────────────
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner}")]
        public async Task<IActionResult> ReviewProofs()
        {
            var allTasks = await _taskService.GetAllTasksAsync();
            var pending  = allTasks.Where(t => t.Status == Models.TaskStatus.ProofSubmitted).ToList();
            return View(pending);
        }

        // ── POST: /TaskAssignment/ReviewProof ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner}")]
        public async Task<IActionResult> ReviewProof(ReviewProofViewModel model)
        {
            int userId   = CurrentUserId();
            bool success = await _taskService.ReviewProofAsync(model, userId);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? (model.Approve ? "Task approved! Bonus granted ✅" : "Task rejected. Worker notified.")
                : "Review failed. You may not own this farm's task.";

            return RedirectToAction(nameof(ReviewProofs));
        }

        // ── POST: /TaskAssignment/UpdateStatus ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Worker)]
        public async Task<IActionResult> UpdateStatus(int taskId, string status)
        {
            // Prevent workers from marking directly as Completed (must use SubmitProof)
            if (status == Models.TaskStatus.Completed)
            {
                TempData["InfoMessage"] = "To complete a task, please submit proof first.";
                return RedirectToAction(nameof(Index));
            }

            int userId   = CurrentUserId();
            bool success = await _taskService.UpdateTaskStatusAsync(taskId, userId, status);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? $"Status updated to '{status}'."
                : "Failed to update status. Make sure the task is assigned to you.";

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /TaskAssignment/Assign ───────────────────────────────────────
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner},{AppRoles.Manager}")]
        public async Task<IActionResult> Assign()
        {
            await PopulateFarmsViewBagAsync();
            var farms = ViewBag.Farms as List<Farm> ?? new List<Farm>();
            return View(new TaskAssignViewModel
            {
                FarmId = farms.FirstOrDefault()?.Id ?? 0,
                DueDate = DateTime.Today.AddDays(3),
                ExpiresAt = DateTime.Now.AddHours(8)
            });
        }

        // ── POST: /TaskAssignment/Assign ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner},{AppRoles.Manager}")]
        public async Task<IActionResult> Assign(TaskAssignViewModel model)
        {
            model.TaskType = TaskTypes.Open;
            model.AssignedWorkerId = 0;

            var ownsFarm = User.IsInRole(AppRoles.Admin) ||
                await _context.Farms.AnyAsync(f => f.Id == model.FarmId &&
                                                   f.OwnerId == CurrentUserId() &&
                                                   !f.IsDeleted);
            if (!ownsFarm)
                ModelState.AddModelError("FarmId", "Select one of your farms.");

            if (!ModelState.IsValid)
            {
                await PopulateFarmsViewBagAsync();
                return View(model);
            }

            int userId = CurrentUserId();
            await _taskService.CreateTaskAsync(model, userId);
            TempData["SuccessMessage"] = "Task created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ── GET: /TaskAssignment/Edit/{id} ────────────────────────────────────
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner}")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var editModel = new TaskAssignViewModel
            {
                Id               = task.Id,
                Title            = task.Title,
                Description      = task.Description,
                AssignedWorkerId = task.AssignedWorkerId,
                FarmId           = task.FarmId,
                Priority         = task.Priority,
                TaskType         = TaskTypes.Open,
                DueDate          = task.DueDate,
                ExpiresAt        = task.ExpiresAt,
                BonusAmount      = task.BonusAmount,
                Status           = task.Status
            };

            await PopulateFarmsViewBagAsync();
            return View(editModel);
        }

        // ── POST: /TaskAssignment/Edit/{id} ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner}")]
        public async Task<IActionResult> Edit(int id, TaskAssignViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateFarmsViewBagAsync();
                return View(model);
            }

            await _taskService.UpdateTaskAsync(model);
            TempData["SuccessMessage"] = "Task updated.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET: /TaskAssignment/Delete/{id} ──────────────────────────────────
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return View(task);
        }

        // ── POST: /TaskAssignment/Delete/{id} ─────────────────────────────────
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Owner}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            TempData["SuccessMessage"] = "Task deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
