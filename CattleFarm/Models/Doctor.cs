using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CattleFarm.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(200)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? LicenseNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsultationFee { get; set; }

        public string? ImagePath { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsActive    { get; set; } = true;
        public bool IsDeleted   { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // ── Extended Professional Profile (set during invitation self-registration) ──
        public DateTime? DateOfBirth       { get; set; }
        public DoctorGender? Gender        { get; set; }

        [StringLength(500)]
        public string? Address             { get; set; }

        [StringLength(200)]
        public string? Qualification       { get; set; }

        [StringLength(200)]
        public string? University          { get; set; }

        [Range(0, 60)]
        public int YearsOfExperience       { get; set; }

        /// <summary>Comma-separated day names e.g. "Monday,Wednesday,Friday".</summary>
        [StringLength(200)]
        public string? AvailableDays       { get; set; }

        [StringLength(10)]
        public string? AvailableTimeFrom   { get; set; }   // e.g. "09:00"

        [StringLength(10)]
        public string? AvailableTimeTo     { get; set; }   // e.g. "17:00"

        /// <summary>Human-readable availability, e.g. "Mon-Fri 9am-5pm".</summary>
        [StringLength(500)]
        public string? AvailableTimeSlot   { get; set; }

        public bool EmergencyAvailable     { get; set; } = false;

        /// <summary>Path to uploaded license PDF/image in wwwroot/uploads/licenses/.</summary>
        public string? LicenseDocumentPath { get; set; }

        /// <summary>True once Admin verifies the license document.</summary>
        public bool IsVerified             { get; set; } = false;

        /// <summary>Admin must approve before the doctor appears in the public veterinarian list.</summary>
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        // ── FK — Invitation Module ────────────────────────────────────────────
        public int? InvitationId { get; set; }
        [ForeignKey(nameof(InvitationId))]
        public virtual DoctorInvitation? Invitation { get; set; }

        // ── FK — User (global veterinarian, not tied to a farm) ─────────────
        public int? UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }

        // ── Navigation ────────────────────────────────────────────────────────
        public virtual ICollection<Appointment>    Appointments    { get; set; } = new List<Appointment>();
        public virtual ICollection<HealthRecord>   HealthRecords   { get; set; } = new List<HealthRecord>();
        public virtual ICollection<MedicineRecord> MedicineRecords { get; set; } = new List<MedicineRecord>();
    }
}

