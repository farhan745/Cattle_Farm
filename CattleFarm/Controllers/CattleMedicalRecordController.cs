using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class CattleMedicalRecordController : Controller
    {
        private readonly ICattleMedicalRecordService _medicalService;
        private readonly ICattleService _cattleService;
        private readonly IAuditService _auditService;

        public CattleMedicalRecordController(
            ICattleMedicalRecordService medicalService,
            ICattleService cattleService,
            IAuditService auditService)
        {
            _medicalService = medicalService;
            _cattleService = cattleService;
            _auditService = auditService;
        }

        [Authorize(Roles = AppRoles.AdminManagerOwnerDoctor)]
        public async Task<IActionResult> MedicalHistory(int cattleId)
        {
            var cattle = await _cattleService.GetByIdAsync(cattleId);
            if (cattle is null) return NotFound();

            var records = await _medicalService.GetByCattleIdAsync(cattleId);
            ViewBag.Cattle = cattle;
            return View(records);
        }

        [Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> AddMedicalRecord(int cattleId)
        {
            var cattle = await _cattleService.GetByIdAsync(cattleId);
            if (cattle is null) return NotFound();

            return View(new CattleMedicalRecordViewModel
            {
                CattleId = cattleId,
                CattleName = cattle.Name,
                CattleTagId = cattle.TagId,
                ExaminationDate = DateTime.Today
            });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> AddMedicalRecord(CattleMedicalRecordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var cattle = await _cattleService.GetByIdAsync(vm.CattleId);
                if (cattle != null)
                {
                    vm.CattleName = cattle.Name;
                    vm.CattleTagId = cattle.TagId;
                }
                return View(vm);
            }

            var record = await _medicalService.AddRecordAsync(vm, GetUserId());
            await _auditService.LogActivityAsync(GetUserId(), $"Logged medical examination for cattle #{vm.CattleId}", "CattleMedicalRecord", record.Id);
            TempData["SuccessMessage"] = "Medical record saved successfully.";
            return RedirectToAction(nameof(MedicalHistory), new { cattleId = vm.CattleId });
        }

        [Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> EditMedicalRecord(int id)
        {
            var record = await _medicalService.GetByIdAsync(id);
            if (record is null) return NotFound();
            if (record.DoctorId != GetUserId()) return Forbid();

            return View(MapToViewModel(record));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> EditMedicalRecord(int id, CattleMedicalRecordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var updated = await _medicalService.UpdateAsync(id, vm, GetUserId());
            if (!updated) return Forbid();

            TempData["SuccessMessage"] = "Medical record updated.";
            return RedirectToAction(nameof(MedicalHistory), new { cattleId = vm.CattleId });
        }

        private static CattleMedicalRecordViewModel MapToViewModel(CattleMedicalRecord record) => new()
        {
            Id = record.Id,
            CattleId = record.CattleId,
            CattleName = record.Cattle?.Name,
            CattleTagId = record.Cattle?.TagId,
            ExaminationDate = record.ExaminationDate,
            ChiefComplaint = record.ChiefComplaint,
            Diagnosis = record.Diagnosis,
            Prescription = record.Prescription,
            MedicineName = record.MedicineName,
            MedicineDose = record.MedicineDose,
            DoseFrequency = record.DoseFrequency,
            DoseDurationDays = record.DoseDurationDays,
            NextVisitDate = record.NextVisitDate,
            Notes = record.Notes
        };

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var p) ? p : 0;
        }
    }
}
