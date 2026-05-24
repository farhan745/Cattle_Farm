using System;
using System.ComponentModel.DataAnnotations;

namespace CattleFarm.ViewModels
{
    /// <summary>Read-only view model for displaying payroll records.</summary>
    public class PayrollViewModel
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public int UserId { get; set; }
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string WorkerName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal OvertimePay { get; set; }
        public double OvertimeHours { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>Form model for editing/adjusting a payroll record.</summary>
    public class PayrollEditViewModel
    {
        public int Id { get; set; }
        public int WorkerId { get; set; }
        public int UserId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal BaseSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Overtime pay cannot be negative.")]
        [Display(Name = "Overtime Pay")]
        public decimal OvertimePay { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Overtime Hours")]
        public double OvertimeHours { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Bonus cannot be negative.")]
        [Display(Name = "Bonus")]
        public decimal Bonus { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deductions cannot be negative.")]
        [Display(Name = "Deductions")]
        public decimal Deductions { get; set; }

        [Display(Name = "Net Salary")]
        public decimal NetSalary { get; set; }

        [Display(Name = "Mark as Paid")]
        public bool IsPaid { get; set; }
    }

    /// <summary>Form model for generating monthly payroll.</summary>
    public class PayrollGenerateViewModel
    {
        [Required]
        [Range(2000, 2100, ErrorMessage = "Please enter a valid year.")]
        [Display(Name = "Year")]
        public int Year { get; set; } = DateTime.Today.Year;

        [Required]
        [Range(1, 12, ErrorMessage = "Please select a valid month.")]
        [Display(Name = "Month")]
        public int Month { get; set; } = DateTime.Today.Month;
    }
}
