using System.Collections.Generic;
using System.Threading.Tasks;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface ITaskAssignmentService
    {
        // ── Read ──────────────────────────────────────────────────────────────
        Task<IEnumerable<TaskViewModel>> GetAllTasksAsync();
        Task<IEnumerable<TaskViewModel>> GetTasksByUserIdAsync(int userId);
        Task<TaskViewModel?> GetTaskByIdAsync(int id);

        /// <summary>Open tasks visible on the worker board (any worker can accept).</summary>
        Task<IEnumerable<TaskViewModel>> GetOpenTasksAsync(int farmId);

        // ── Write ─────────────────────────────────────────────────────────────
        Task<int> CreateTaskAsync(TaskAssignViewModel model, int createdByUserId);
        Task UpdateTaskAsync(TaskAssignViewModel model);
        Task DeleteTaskAsync(int id);

        // ── Worker actions ────────────────────────────────────────────────────

        /// <summary>Worker accepts an Open task (first-come-first-served).</summary>
        Task<bool> AcceptTaskAsync(int taskId, int workerUserId);

        /// <summary>Worker updates their own task status (e.g. InProgress).</summary>
        Task<bool> UpdateTaskStatusAsync(int taskId, int workerUserId, string status);

        /// <summary>Worker submits proof (photo + note) — status → ProofSubmitted.</summary>
        Task<bool> SubmitProofAsync(SubmitProofViewModel model, int workerUserId);

        // ── Owner actions ─────────────────────────────────────────────────────

        /// <summary>
        /// Owner reviews submitted proof.
        /// Approve → status = Completed, BonusApproved = true (if bonus exists).
        /// Reject  → status = Rejected, RejectionReason set.
        /// </summary>
        Task<bool> ReviewProofAsync(ReviewProofViewModel model, int ownerUserId);
    }
}
