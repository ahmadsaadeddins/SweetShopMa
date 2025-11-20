using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Services;

/// <summary>
/// PDF generation service using QuestPDF library.
/// </summary>
public class PdfService : IPdfService
{
    private const int UsersPerPage = 10;

    public async Task<string?> GeneratePayrollPdfAsync(List<MonthlyAttendanceSummary> summaries, DateTime month, MonthlyAttendanceTotals totals)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            var monthName = month.ToString("MMMM yyyy");
            var fileName = $"Payroll_{month:yyyy-MM}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .Text($"Payroll Report - {monthName}")
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Column(column =>
                        {
                            // Group summaries into pages (10 per page)
                            var pages = summaries
                                .Select((summary, index) => new { summary, index })
                                .GroupBy(x => x.index / UsersPerPage)
                                .ToList();

                            foreach (var pageGroup in pages)
                            {
                                var pageSummaries = pageGroup.Select(x => x.summary).ToList();
                                
                                // Table header
                                column.Item()
                                    .PaddingBottom(5)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2); // Name
                                            columns.RelativeColumn(1); // Days Worked
                                            columns.RelativeColumn(1); // Absent Days
                                            columns.RelativeColumn(1); // OT Hours
                                            columns.RelativeColumn(1.5f); // Total Pay
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell().Element(CellStyle).Text("Name").Bold();
                                            header.Cell().Element(CellStyle).Text("Days Worked").Bold();
                                            header.Cell().Element(CellStyle).Text("Absent Days").Bold();
                                            header.Cell().Element(CellStyle).Text("OT Hours").Bold();
                                            header.Cell().Element(CellStyle).Text("Total Pay").Bold();
                                        });

                                        foreach (var summary in pageSummaries)
                                        {
                                            table.Cell().Element(CellStyle).Text(summary.UserName);
                                            table.Cell().Element(CellStyle).Text(summary.DaysPresent.ToString());
                                            table.Cell().Element(CellStyle).Text(summary.DaysAbsent.ToString());
                                            table.Cell().Element(CellStyle).Text($"{summary.OvertimeHours:F1}");
                                            table.Cell().Element(CellStyle).Text($"${summary.Payroll:F2}");
                                        }
                                    });

                                // Add spacing between pages (except for last page)
                                if (pageGroup.Key < pages.Count - 1)
                                {
                                    column.Item().PageBreak();
                                }
                            }

                            // Totals section at the end
                            column.Item().PaddingTop(10);
                            column.Item()
                                .Border(1)
                                .Padding(5)
                                .Column(totalsColumn =>
                                {
                                    totalsColumn.Item().Text("Totals").Bold().FontSize(11);
                                    totalsColumn.Item().Text($"Total Present Days: {totals.TotalPresentDays}");
                                    totalsColumn.Item().Text($"Total Absent Days: {totals.TotalAbsentDays}");
                                    totalsColumn.Item().Text($"Total OT Hours: {totals.TotalOvertimeHours:F1}");
                                    totalsColumn.Item().Text($"Total Payroll: ${totals.TotalPayroll:F2}").Bold();
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on ");
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                        });
                });
            });

            // GeneratePdf is synchronous, but we're in an async method
            await Task.Run(() => document.GeneratePdf(filePath));
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating PDF: {ex.Message}");
            return null;
        }
    }

    private static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
    {
        return container
            .Border(1)
            .Padding(3)
            .AlignLeft()
            .AlignMiddle();
    }
}

