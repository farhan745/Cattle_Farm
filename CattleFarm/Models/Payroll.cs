using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    /// <summary>
    /// Monthly payroll record for a single worker.
    /// Scoped to the worker's farm — owners only see their own farm's payroll.
    /// </summary>
    public class Payroll
    {
        public int Id { get; set; }

        // Worker reference
        public int WorkerId { get; set; }
        [ForeignKey(nameof(WorkerId))]
        public virtual Worker? Worker { get; set; }

        // User reference (for quick lookup)
        public int UserId { get; set; }

        // Farm reference (for per-farm filtering)
        public int FarmId { get; set; }
        [ForeignKey(nameof(FarmId))]
        public virtual Farm? Farm { get; set; }

        // Pay period
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        // Salary breakdown
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OvertimePay { get; set; } = 0;

        public double OvertimeHours { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        // Status
        public bool IsPaid { get; set; } = false;
        public DateTime? PaidAt { get; set; }

        // Audit
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
