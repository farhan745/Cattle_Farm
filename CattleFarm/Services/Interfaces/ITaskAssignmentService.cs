using System.Collections.Generic;
using System.Threading.Tasks;
using CattleFarm.ViewModels;

namespace CattleFarm.Services.Interfaces
{
    public interface ITaskAssignmentService
    {
        Task<IEnumerable<TaskViewModel>> GetAllTasksAsync();
        Task<IEnumerable<TaskViewModel>> GetTasksByUserIdAsync(int userId);
        Task<TaskViewModel?> GetTaskByIdAsync(int id);
        Task AssignTaskAsync(TaskAssignViewModel model);
        Task DeleteTaskAsync(int id);
        Task<bool> UpdateTaskStatusAsync(int taskId, int userId, string status);
    }
}
