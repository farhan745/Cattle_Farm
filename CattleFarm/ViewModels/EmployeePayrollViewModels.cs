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
}