using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using CattleFarm.Models;
using CattleFarm.Services.Interfaces;
using System.Collections.Generic;
using System;

namespace CattleFarm.Services.Implementations
{
    public class PdfService : IPdfService
    {
        public PdfService()
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;
            }
            catch { }
        }

        public byte[] GenerateReportPdf(string farmName, DateTime from, DateTime to,
            decimal revenue, decimal expenses, decimal profit, double milkTotal,
            int totalCattle, int activeCattle, int sickCattle,
            IEnumerable<(string Category, decimal Total)> expenseBreakdown,
            IEnumerable<(string Source, decimal Total)> revenueBreakdown)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SMART CATTLE FARM MANAGEMENT SYSTEM").Bold().FontSize(14).FontColor(Colors.Green.Darken3);
                            col.Item().Text($"Farm: {farmName}").FontSize(12).SemiBold();
                            col.Item().Text($"Period: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}").FontSize(9).Italic();
                        });
                        row.ConstantItem(150).AlignRight().Text("FINANCIAL REPORT").Bold().FontSize(12).FontColor(Colors.Grey.Darken2);
                    });

                    // Content
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(15);

                        // Summary section
                        col.Item().Text("Summary Metrics").Bold().FontSize(12).Underline();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Revenue").Bold().FontSize(8);
                                c.Item().Text($"BDT {revenue:N2}").FontSize(12).Bold().FontColor(Colors.Green.Darken2);
                            });
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Column(c =>
                            {
                                c.Item().Text("Total Expenses").Bold().FontSize(8);
                                c.Item().Text($"BDT {expenses:N2}").FontSize(12).Bold().FontColor(Colors.Red.Darken2);
                            });
                            table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Column(c =>
                            {
                                c.Item().Text("Net Profit").Bold().FontSize(8);
                                c.Item().Text($"BDT {profit:N2}").FontSize(12).Bold().FontColor(profit >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                            });
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            table.Cell().Padding(5).Text($"Milk Production: {milkTotal:N1} L").SemiBold();
                            table.Cell().Padding(5).Text($"Total Cattle: {totalCattle}").SemiBold();
                            table.Cell().Padding(5).Text($"Active Cattle: {activeCattle}").SemiBold();
                            table.Cell().Padding(5).Text($"Sick Cattle: {sickCattle}").SemiBold();
                        });

                        // Breakdown tables
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Spacing(5);
                                c.Item().Text("Revenue Breakdown").Bold().FontSize(10);
                                c.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn();
                                        cols.ConstantColumn(100);
                                    });
                                    t.Header(h =>
                                    {
                                        h.Cell().Background(Colors.Green.Darken2).Padding(4).Text("Source").Bold().FontColor(Colors.White);
                                        h.Cell().Background(Colors.Green.Darken2).Padding(4).Text("Amount").Bold().FontColor(Colors.White).AlignRight();
                                    });
                                    foreach (var r in revenueBreakdown)
                                    {
                                        t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(r.Source);
                                        t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"BDT {r.Total:N2}").AlignRight();
                                    }
                                });
                            });

                            row.ConstantItem(20); // Spacer

                            row.RelativeItem().Column(c =>
                            {
                                c.Spacing(5);
                                c.Item().Text("Expense Breakdown").Bold().FontSize(10);
                                c.Item().Table(t =>
                                {
                                    t.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn();
                                        cols.ConstantColumn(100);
                                    });
                                    t.Header(h =>
                                    {
                                        h.Cell().Background(Colors.Red.Darken2).Padding(4).Text("Category").Bold().FontColor(Colors.White);
                                        h.Cell().Background(Colors.Red.Darken2).Padding(4).Text("Amount").Bold().FontColor(Colors.White).AlignRight();
                                    });
                                    foreach (var ex in expenseBreakdown)
                                    {
                                        t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(ex.Category);
                                        t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"BDT {ex.Total:N2}").AlignRight();
                                    }
                                });
                            });
                        });
                    });

                    // Footer
                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GeneratePayrollSlipPdf(Payroll payroll)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(payroll.Farm?.Name ?? "SMART CATTLE FARM").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                        col.Item().Text("PAYROLL SLIP").Bold().FontSize(14);
                        col.Item().Text($"Period: {System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(payroll.Month)} {payroll.Year}").FontSize(9).Italic();
                        col.Item().LineHorizontal(1f).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(x => { x.Span("Worker: ").Bold(); x.Span(payroll.Worker?.FullName ?? "N/A"); });
                                c.Item().Text(x => { x.Span("Role: ").Bold(); x.Span(payroll.Worker?.Role ?? "N/A"); });
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text(x => { x.Span("Slip ID: ").Bold(); x.Span(payroll.Id.ToString()); });
                                c.Item().Text(x => { x.Span("Status: ").Bold(); x.Span(payroll.IsPaid ? "PAID" : "UNPAID").FontColor(payroll.IsPaid ? Colors.Green.Darken3 : Colors.Red.Darken3).Bold(); });
                                if (payroll.PaidAt.HasValue)
                                    c.Item().Text(x => { x.Span("Paid Date: ").Bold(); x.Span(payroll.PaidAt.Value.ToString("yyyy-MM-dd")); });
                            });
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Earnings / Deductions").Bold();
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Amount").Bold().AlignRight();
                            });

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Base Salary");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"BDT {payroll.BaseSalary:N2}").AlignRight();

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"Overtime ({payroll.OvertimeHours} hours)");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"BDT {payroll.OvertimePay:N2}").AlignRight();

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Bonus");
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"BDT {payroll.Bonus:N2}").AlignRight();

                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text("Deductions").FontColor(Colors.Red.Darken2);
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"- BDT {payroll.Deductions:N2}").AlignRight().FontColor(Colors.Red.Darken2);

                            table.Cell().Padding(4).Text("Net Salary").Bold();
                            table.Cell().Padding(4).Text($"BDT {payroll.NetSalary:N2}").Bold().AlignRight();
                        });
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Employer Signature: __________________").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text("Employee Signature: __________________").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateCattleProfilePdf(Cattle cattle)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(cattle.Farm?.Name ?? "SMART CATTLE FARM").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                            col.Item().Text($"CATTLE PROFILE: {cattle.Name}").Bold().FontSize(16);
                            col.Item().Text($"Tag ID: {cattle.TagId}").FontSize(11).SemiBold();
                        });
                        row.ConstantItem(100).AlignRight().Text(cattle.Status.ToString().ToUpper())
                            .Bold().FontSize(12).FontColor(cattle.Status == CattleStatus.Active ? Colors.Green.Darken3 : Colors.Grey.Darken2);
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Text("Key Information").Bold().FontSize(12).Underline();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Padding(4).Text("Category:").Bold();
                            table.Cell().Padding(4).Text(cattle.Category.ToString());

                            table.Cell().Padding(4).Text("Breed:").Bold();
                            table.Cell().Padding(4).Text(cattle.Breed);

                            table.Cell().Padding(4).Text("Gender:").Bold();
                            table.Cell().Padding(4).Text(cattle.Gender.ToString());

                            table.Cell().Padding(4).Text("DOB:").Bold();
                            table.Cell().Padding(4).Text(cattle.DateOfBirth.ToString("yyyy-MM-dd"));

                            table.Cell().Padding(4).Text("Weight:").Bold();
                            table.Cell().Padding(4).Text($"{cattle.Weight} kg");

                            table.Cell().Padding(4).Text("Health Status:").Bold();
                            table.Cell().Padding(4).Text(cattle.HealthStatus.ToString());

                            table.Cell().Padding(4).Text("Origin:").Bold();
                            table.Cell().Padding(4).Text(cattle.Origin ?? "N/A");

                            table.Cell().Padding(4).Text("Purchase Price:").Bold();
                            table.Cell().Padding(4).Text($"BDT {cattle.PurchasePrice:N2}");
                            
                            table.Cell().Padding(4).Text("Purchase Date:").Bold();
                            table.Cell().Padding(4).Text(cattle.PurchaseDate?.ToString("yyyy-MM-dd") ?? "N/A");

                            table.Cell().Padding(4).Text("Sale Price:").Bold();
                            table.Cell().Padding(4).Text(cattle.SalePrice.HasValue ? $"BDT {cattle.SalePrice.Value:N2}" : "N/A");

                            if (cattle.Status == CattleStatus.Transferred)
                            {
                                table.Cell().Padding(4).Text("Transferred To:").Bold();
                                table.Cell().Padding(4).Text(cattle.TransferredTo ?? "N/A");

                                table.Cell().Padding(4).Text("Transfer Date:").Bold();
                                table.Cell().Padding(4).Text(cattle.TransferDate?.ToString("yyyy-MM-dd") ?? "N/A");
                            }
                            else
                            {
                                table.Cell().Padding(4);
                                table.Cell().Padding(4);
                                table.Cell().Padding(4);
                                table.Cell().Padding(4);
                            }
                        });

                        if (!string.IsNullOrWhiteSpace(cattle.Description))
                        {
                            col.Item().Column(c =>
                            {
                                c.Item().Text("Description").Bold().FontSize(10);
                                c.Item().Text(cattle.Description).Italic().FontColor(Colors.Grey.Darken2);
                            });
                        }

                        if (cattle.Vaccinations != null && cattle.Vaccinations.Count > 0)
                        {
                            col.Item().Text("Vaccination History").Bold().FontSize(12).Underline();
                            col.Item().Table(t =>
                            {
                                t.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn();
                                    cols.RelativeColumn();
                                    cols.RelativeColumn();
                                });
                                t.Header(h =>
                                {
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Vaccine").Bold();
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Date Given").Bold();
                                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Next Due").Bold();
                                });
                                foreach (var v in cattle.Vaccinations)
                                {
                                    t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(v.VaccineName);
                                    t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(v.VaccinationDate.ToString("yyyy-MM-dd"));
                                    t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(v.NextDueDate?.ToString("yyyy-MM-dd") ?? "");
                                }
                            });
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Report Generated: {DateTime.Now:yyyy-MM-dd}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateOrderInvoicePdf(Order order)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(order.Farm?.Name ?? "SMART CATTLE FARM").Bold().FontSize(14).FontColor(Colors.Green.Darken3);
                            col.Item().Text(order.Farm?.Location ?? "").FontSize(9).FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"INVOICE FOR ORDER #{order.Id}").Bold().FontSize(16);
                        });
                        row.ConstantItem(150).AlignRight().Column(col =>
                        {
                            col.Item().Text("Payment Status").Bold().FontSize(9);
                            col.Item().Text(order.PaymentStatus.ToString().ToUpper())
                                .Bold().FontSize(14).FontColor(order.PaymentStatus == PaymentStatus.Completed ? Colors.Green.Darken3 : Colors.Red.Darken3);
                            col.Item().Text($"Date: {order.OrderDate:yyyy-MM-dd HH:mm}").FontSize(8);
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Customer Details").Bold().FontSize(10).Underline();
                                c.Item().Text(order.Customer?.FullName ?? "N/A").SemiBold();
                                c.Item().Text(order.Customer?.Email ?? "N/A");
                                c.Item().Text(order.Customer?.PhoneNumber ?? "N/A");
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Delivery Address").Bold().FontSize(10).Underline();
                                c.Item().Text(order.DeliveryAddress ?? "Local Pickup").Italic();
                            });
                        });

                        col.Item().Text("Order Items").Bold().FontSize(12).Underline();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Product").Bold();
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Qty").Bold().AlignRight();
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Unit Price").Bold().AlignRight();
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Total").Bold().AlignRight();
                            });

                            foreach (var item in order.OrderItems)
                            {
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(item.Product?.Name ?? "Unknown Product");
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"{item.Quantity:N1}").AlignRight();
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"BDT {item.UnitPrice:N2}").AlignRight();
                                table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text($"BDT {item.TotalPrice:N2}").AlignRight();
                            }

                            table.Cell().Padding(4).Text("Grand Total").Bold();
                            table.Cell().Padding(4);
                            table.Cell().Padding(4);
                            table.Cell().Padding(4).Text($"BDT {order.TotalAmount:N2}").Bold().AlignRight();
                        });

                        if (!string.IsNullOrWhiteSpace(order.Notes))
                        {
                            col.Item().Column(c =>
                            {
                                c.Item().Text("Order Notes").Bold().FontSize(9);
                                c.Item().Text(order.Notes).Italic().FontSize(9).FontColor(Colors.Grey.Darken2);
                            });
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text("Thank you for your business!").Italic().FontSize(9).FontColor(Colors.Green.Darken3);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateAppointmentPdf(Appointment appointment)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(appointment.Farm?.Name ?? "SMART CATTLE FARM").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                            col.Item().Text("VETERINARY APPOINTMENT REPORT").Bold().FontSize(14);
                            col.Item().Text($"Scheduled Date: {appointment.ScheduledAt:yyyy-MM-dd HH:mm}").FontSize(10);
                        });
                        row.ConstantItem(120).AlignRight().Column(col =>
                        {
                            col.Item().Text("Status").Bold().FontSize(9);
                            col.Item().Text(appointment.Status.ToString().ToUpper())
                                .Bold().FontSize(12).FontColor(appointment.Status == AppointmentStatus.Completed ? Colors.Green.Darken3 : Colors.Orange.Darken3);
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Spacing(15);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Cell().Padding(4).Column(c =>
                            {
                                c.Item().Text("Veterinarian Details").Bold().FontSize(10).Underline();
                                c.Item().Text(appointment.Doctor?.FullName ?? "N/A").SemiBold();
                                c.Item().Text($"Specialization: {appointment.Doctor?.Specialization ?? "N/A"}");
                                c.Item().Text($"Phone: {appointment.Doctor?.Phone ?? "N/A"}");
                            });

                            table.Cell().Padding(4).Column(c =>
                            {
                                c.Item().Text("Cattle Details").Bold().FontSize(10).Underline();
                                c.Item().Text($"Name: {appointment.Cattle?.Name ?? "N/A"}").SemiBold();
                                c.Item().Text($"Tag ID: {appointment.Cattle?.TagId ?? "N/A"}");
                                c.Item().Text($"Breed: {appointment.Cattle?.Breed ?? "N/A"}");
                            });
                        });

                        col.Item().Column(c =>
                        {
                            c.Spacing(3);
                            c.Item().Text("Reason for Visit").Bold().FontSize(10);
                            c.Item().Text(appointment.Reason);
                        });

                        if (!string.IsNullOrWhiteSpace(appointment.Notes))
                        {
                            col.Item().Column(c =>
                            {
                                c.Spacing(3);
                                c.Item().Text("Appointment Notes").Bold().FontSize(10);
                                c.Item().Text(appointment.Notes).Italic().FontColor(Colors.Grey.Darken2);
                            });
                        }

                        if (appointment.Status == AppointmentStatus.Completed)
                        {
                            col.Item().LineHorizontal(1f).LineColor(Colors.Grey.Lighten1);
                            col.Item().Text("Treatment & Completion Summary").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                            
                            if (appointment.CompletedAt.HasValue)
                                col.Item().Text($"Completed On: {appointment.CompletedAt.Value:yyyy-MM-dd HH:mm}");
                            
                            if (!string.IsNullOrWhiteSpace(appointment.CompletionNotes))
                            {
                                col.Item().Column(c =>
                                {
                                    c.Spacing(3);
                                    c.Item().Text("Completion Notes:").Bold().FontSize(10);
                                    c.Item().Text(appointment.CompletionNotes);
                                });
                            }
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Generated: {DateTime.Now:yyyy-MM-dd}").FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
