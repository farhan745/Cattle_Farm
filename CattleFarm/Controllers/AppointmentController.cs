using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using CattleFarm.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;
using ClosedXML.Excel;

namespace CattleFarm.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ICattleService      _cattleService;
        private readonly IDoctorService      _doctorService;
        private readonly IFarmService        _farmService;
        private readonly IPdfService         _pdfService;

        public AppointmentController(IAppointmentService appointment, ICattleService cattle, IDoctorService doctor, IFarmService farm, IPdfService pdfService)
        {
            _appointmentService = appointment;
            _cattleService = cattle;
            _doctorService = doctor;
            _farmService = farm;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index(int page = 1, int? farmId = null, AppointmentStatus? status = null)
        {
            int? doctorId = null;
            IReadOnlyCollection<int>? ownerFarmIds = null;

            if (User.IsInRole(AppRoles.Doctor))
            {
                var doc = await _doctorService.GetByUserIdAsync(GetUserId());
                if (doc is null)
                {
                    ViewData["CurrentPage"] = 1;
                    ViewData["TotalPages"] = 0;
                    return View(Enumerable.Empty<Appointment>());
                }
                doctorId = doc.Id;
            }
            else if (User.IsInRole(AppRoles.Owner))
            {
                var farms = await _farmService.GetByOwnerAsync(GetUserId());
                ownerFarmIds = farms.Select(f => f.Id).ToList();
                if (ownerFarmIds.Count == 0)
                {
                    ViewData["CurrentPage"] = 1;
                    ViewData["TotalPages"] = 0;
                    ViewBag.Farms = farms;
                    return View(Enumerable.Empty<Appointment>());
                }
            }

            var (items, total) = await _appointmentService.GetPagedAsync(page, 10, farmId, status, doctorId, ownerFarmIds);
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"]  = (int)Math.Ceiling(total / (double)10);
            ViewBag.Farms = User.IsInRole(AppRoles.Owner)
                ? await _farmService.GetByOwnerAsync(GetUserId())
                : await _farmService.GetAllAsync();
            ViewBag.IsDoctor = User.IsInRole(AppRoles.Doctor);
            ViewBag.IsOwner  = User.IsInRole(AppRoles.Owner);
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var appt = await _appointmentService.GetByIdAsync(id);
            if (appt is null) return NotFound();
            if (!await _appointmentService.CanViewAsync(id, GetUserId(), GetUserRole()))
                return Forbid();

            ViewBag.IsAssignedDoctor = await IsAssignedDoctorAsync(appt);
            ViewBag.IsOwner = User.IsInRole(AppRoles.Owner);
            return View(appt);
        }

        [Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(int? cattleId, int? farmId, int? doctorId)
        {
            if (cattleId.HasValue && cattleId.Value > 0 && (!farmId.HasValue || farmId.Value == 0))
            {
                var cattle = await _cattleService.GetByIdAsync(cattleId.Value);
                if (cattle != null) farmId = cattle.FarmId;
            }

            await LoadDropdowns(farmId, GetUserId());
            return View(new AppointmentViewModel
            {
                CattleId = cattleId ?? 0,
                FarmId = farmId ?? 0,
                DoctorId = doctorId ?? 0,
                ScheduledAt = DateTime.Now.AddDays(1)
            });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Create(AppointmentViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(vm.FarmId, GetUserId());
                return View(vm);
            }

            try
            {
                var appt = await _appointmentService.CreateAsync(vm, GetUserId(), GetUserRole());
                TempData["SuccessMessage"] = "Booking request sent. The veterinarian will be notified to accept.";
                return RedirectToAction(nameof(Details), new { id = appt.Id });
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            await LoadDropdowns(vm.FarmId, GetUserId());
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> Approve(int id)
        {
            if (!await _appointmentService.ApproveAsync(id, GetUserId()))
            {
                TempData["ErrorMessage"] = "Could not accept this appointment.";
                return RedirectToAction(nameof(Details), new { id });
            }
            TempData["SuccessMessage"] = "Appointment accepted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> Reject(int id, string? reason)
        {
            if (!await _appointmentService.RejectAsync(id, GetUserId(), reason))
            {
                TempData["ErrorMessage"] = "Could not decline this appointment.";
                return RedirectToAction(nameof(Details), new { id });
            }
            TempData["SuccessMessage"] = "Appointment declined.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> Complete(int id)
        {
            var appt = await _appointmentService.GetByIdAsync(id);
            if (appt is null) return NotFound();
            if (!await IsAssignedDoctorAsync(appt) || appt.Status != AppointmentStatus.Accepted)
                return Forbid();
            return View(new CompleteAppointmentViewModel { Id = id });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.Doctor)]
        public async Task<IActionResult> Complete(CompleteAppointmentViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (!await _appointmentService.CompleteAsync(vm, GetUserId()))
            {
                ModelState.AddModelError(string.Empty, "Upload valid evidence (image) and prescription (image or PDF), each under 5 MB.");
                return View(vm);
            }

            TempData["SuccessMessage"] = "Visit completed. The farm owner has been notified.";
            return RedirectToAction(nameof(Details), new { id = vm.Id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!await _appointmentService.CancelAsync(id, GetUserId(), GetUserRole()))
            {
                TempData["ErrorMessage"] = "Could not cancel this appointment.";
                return RedirectToAction(nameof(Details), new { id });
            }
            TempData["SuccessMessage"] = "Appointment cancelled.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = AppRoles.AdminManagerOrOwner)]
        public async Task<IActionResult> Delete(int id)
        {
            await _appointmentService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Appointment deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int id)
        {
            var appt = await _appointmentService.GetByIdAsync(id);
            if (appt is null) return NotFound();
            
            if (!await _appointmentService.CanViewAsync(id, GetUserId(), GetUserRole()))
                return Forbid();

            var pdfBytes = _pdfService.GenerateAppointmentPdf(appt);
            var fileName = $"appointment-summary-{appt.Id}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPrescriptionExcel(int id)
        {
            var appt = await _appointmentService.GetByIdAsync(id);
            if (appt is null) return NotFound();

            if (!await _appointmentService.CanViewAsync(id, GetUserId(), GetUserRole()))
                return Forbid();

            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Appointment Details");
            sheet.Cell(1, 1).Value = $"Appointment #{appt.Id} Summary";
            sheet.Cell(1, 1).Style.Font.Bold = true;
            sheet.Cell(1, 1).Style.Font.FontSize = 14;

            sheet.Cell(3, 1).Value = "Field";
            sheet.Cell(3, 2).Value = "Details";
            sheet.Range(3, 1, 3, 2).Style.Font.Bold = true;

            sheet.Cell(4, 1).Value = "Scheduled At";
            sheet.Cell(4, 2).Value = appt.ScheduledAt.ToString("yyyy-MM-dd HH:mm");

            sheet.Cell(5, 1).Value = "Status";
            sheet.Cell(5, 2).Value = appt.Status.ToString();

            sheet.Cell(6, 1).Value = "Reason";
            sheet.Cell(6, 2).Value = appt.Reason;

            sheet.Cell(7, 1).Value = "Notes";
            sheet.Cell(7, 2).Value = appt.Notes ?? "";

            sheet.Cell(8, 1).Value = "Farm";
            sheet.Cell(8, 2).Value = appt.Farm?.Name ?? "";

            sheet.Cell(9, 1).Value = "Cattle Name";
            sheet.Cell(9, 2).Value = appt.Cattle?.Name ?? "";

            sheet.Cell(10, 1).Value = "Cattle Tag ID";
            sheet.Cell(10, 2).Value = appt.Cattle?.TagId ?? "";

            sheet.Cell(11, 1).Value = "Veterinarian";
            sheet.Cell(11, 2).Value = appt.Doctor?.FullName ?? "";

            sheet.Cell(12, 1).Value = "Vet License Number";
            sheet.Cell(12, 2).Value = appt.Doctor?.LicenseNumber ?? "";

            if (appt.Status == AppointmentStatus.Completed)
            {
                sheet.Cell(14, 1).Value = "Completion Summary";
                sheet.Cell(14, 1).Style.Font.Bold = true;

                sheet.Cell(15, 1).Value = "Completed At";
                sheet.Cell(15, 2).Value = appt.CompletedAt?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";

                sheet.Cell(16, 1).Value = "Completion Notes";
                sheet.Cell(16, 2).Value = appt.CompletionNotes ?? "";

                sheet.Cell(17, 1).Value = "Prescription Path";
                sheet.Cell(17, 2).Value = appt.PrescriptionPath ?? "";
            }

            sheet.Columns().AdjustToContents();

            using var workbookStream = new MemoryStream();
            workbook.SaveAs(workbookStream);
            var fileName = $"appointment-summary-{appt.Id}.xlsx";
            return File(
                workbookStream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private async Task LoadDropdowns(int? farmId, int userId)
        {
            var farms = User.IsInRole(AppRoles.Owner)
                ? await _farmService.GetByOwnerAsync(userId)
                : await _farmService.GetAllAsync();
            ViewBag.Farms = farms;

            if (farmId.HasValue && farmId.Value > 0)
                ViewBag.Cattles = await _cattleService.GetByFarmIdAsync(farmId.Value);
            else if (farms.Any())
                ViewBag.Cattles = await _cattleService.GetByFarmIdAsync(farms.First().Id);
            else
                ViewBag.Cattles = Enumerable.Empty<Cattle>();

            var doctors = await _doctorService.GetPagedAsync(1, 500);
            ViewBag.Doctors = doctors.Items.Where(d => d.ApprovalStatus == ApprovalStatus.Approved && d.IsActive);
        }

        private async Task<bool> IsAssignedDoctorAsync(Appointment appt)
        {
            if (!User.IsInRole(AppRoles.Doctor)) return false;
            var doc = await _doctorService.GetByUserIdAsync(GetUserId());
            return doc != null && appt.DoctorId == doc.Id;
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(id, out var p) ? p : 0;
        }

        private string? GetUserRole()
        {
            if (User.IsInRole(AppRoles.Admin)) return AppRoles.Admin;
            if (User.IsInRole(AppRoles.Manager)) return AppRoles.Manager;
            if (User.IsInRole(AppRoles.Owner)) return AppRoles.Owner;
            if (User.IsInRole(AppRoles.Doctor)) return AppRoles.Doctor;
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }
    }
}
