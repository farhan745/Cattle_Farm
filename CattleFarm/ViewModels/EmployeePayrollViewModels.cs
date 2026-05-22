using System;
using System.ComponentModel.DataAnnotations;
using CattleFarm.Models;

namespace CattleFarm.ViewModels
{
    // ── Employee ViewModels ───────────────────────────────────────────────────────
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public decimal BaseSalary { get; set; }
        public bool IsActive { get; set; }
        public DateTime HiredAt { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class EmployeeCreateViewModel
    {
        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [Required, Range(0, 10000000)]
        public decimal BaseSalary { get; set; }

        [Required, StringLength(50)]
        public string Role { get; set; } = "Worker";

        /// <summary>Which farm this employee will be assigned to.</summary>
        public int? FarmId { get; set; }
    }

    public class EmployeeEditViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [Required, Range(0, 10000000)]
        public decimal BaseSalary { get; set; }

        public bool IsActive { get; set; }

        [Required, StringLength(50)]
        public string Role { get; set; } = "Worker";
    }

    // ── Payroll ViewModels ────────────────────────────────────────────────────────
    public class PayrollViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WorkerId { get; set; }
        public int FarmId { get; set; }      // which farm this worker belongs to
        public string WorkerName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public double OvertimeHours { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Deductions { get; set; }
        public decimal Bonus { get; set; }
        public decimal NetSalary { get; set; }
        public bool IsPaid { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class PayrollGenerateViewModel
    {
        [Required, Range(2000, 2100)]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Required, Range(1, 12)]
        public int Month { get; set; } = DateTime.UtcNow.Month;
    }

    public class PayrollEditViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }

        [Range(0, 10000000)]
        public decimal BaseSalary { get; set; }

        [Range(0, 500)]
        public double OvertimeHours { get; set; }

        [Range(0, 10000000)]
        public decimal OvertimePay { get; set; }

        [Range(0, 10000000)]
        public decimal Deductions { get; set; }

        [Range(0, 10000000)]
        public decimal Bonus { get; set; }

        [Range(0, 10000000)]
        public decimal NetSalary { get; set; }
        public bool IsPaid { get; set; }
    }

    // ── Attendance ViewModels ─────────────────────────────────────────────────────
    public class AttendanceViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public AttendanceStatus Status { get; set; }
        public double? HoursWorked { get; set; }
        public string? Notes { get; set; }
    }

    public class AttendanceMarkViewModel
    {
        [Required]
        public int WorkerId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Today;

        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class AttendanceEditViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WorkerId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }
    }

    // ── Task ViewModels ───────────────────────────────────────────────────────────
    public class TaskViewModel
    {
        public int Id { get; set; }
        public int AssignedUserId { get; set; }
        public int AssignedWorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending"; // e.g. "Pending", "InProgress", "Completed"
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class TaskAssignViewModel
    {
        public int Id { get; set; }

        [Required]
        public int AssignedWorkerId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(1);

        public string Status { get; set; } = "Pending";
    }
}
