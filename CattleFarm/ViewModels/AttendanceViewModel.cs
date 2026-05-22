using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CattleFarm.ViewModels
{
    public class AttendanceRowViewModel
    {
        public int WorkerId { get; set; }
        public string WorkerName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = "Present"; // Present, Absent, Late
    }

    public class BulkAttendanceViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        public List<AttendanceRowViewModel> Rows { get; set; } = new List<AttendanceRowViewModel>();
    }
}
