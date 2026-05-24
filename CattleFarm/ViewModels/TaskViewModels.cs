// This file is commented out to avoid CS0101 compilation errors (duplicate class definitions).
// All definitions (TaskViewModel and TaskAssignViewModel) have been consolidated and expanded 
// in TaskAssignmentViewModels.cs as part of the Phase 2 Smart Task System implementation.

/*
using System;
using System.ComponentModel.DataAnnotations;

namespace CattleFarm.ViewModels
{
    /// <summary>
    /// Read-only view model for displaying a task.
    /// </summary>
    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int AssignedWorkerId { get; set; }
        public int AssignedUserId { get; set; }
        public string? WorkerName { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime DueDate { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Form model for creating or editing a task assignment.
    /// </summary>
    public class TaskAssignViewModel
    {
        public int Id { get; set; } // 0 = new task

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        [Display(Name = "Task Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a worker.")]
        [Display(Name = "Assign To")]
        public int AssignedWorkerId { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);

        [Display(Name = "Status")]
        public string? Status { get; set; } = "Pending";
    }
}
*/
