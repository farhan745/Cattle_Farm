using CattleFarm.Models;

namespace CattleFarm.Services.Interfaces
{
    /// <summary>
    /// Generates downloadable PDF documents for various entities.
    /// All methods return raw PDF bytes safe for File() responses.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>Generates a farm financial report PDF.</summary>
        byte[] GenerateReportPdf(string farmName, DateTime from, DateTime to,
            decimal revenue, decimal expenses, decimal profit, double milkTotal,
            int totalCattle, int activeCattle, int sickCattle,
            IEnumerable<(string Category, decimal Total)> expenseBreakdown,
            IEnumerable<(string Source, decimal Total)> revenueBreakdown);

        /// <summary>Generates a payroll slip PDF for a single payroll record.</summary>
        byte[] GeneratePayrollSlipPdf(Payroll payroll);

        /// <summary>Generates a cattle profile PDF.</summary>
        byte[] GenerateCattleProfilePdf(Cattle cattle);

        /// <summary>Generates an order invoice PDF.</summary>
        byte[] GenerateOrderInvoicePdf(Order order);

        /// <summary>Generates an appointment summary PDF.</summary>
        byte[] GenerateAppointmentPdf(Appointment appointment);
    }
}
