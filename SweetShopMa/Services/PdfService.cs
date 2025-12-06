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
using SweetShopMa.Models;
using System.Globalization;

namespace SweetShopMa.Services;

/// <summary>
/// PDF generation service using QuestPDF library.
/// </summary>
public class PdfService : IPdfService
{
    private const int UsersPerPage = 10;
    private readonly LocalizationService _localizationService;

    public PdfService(LocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    private bool IsArabic => _localizationService?.CurrentLanguage == "ar";
    private string M(string en, string ar) => IsArabic ? ar : en;
    private string FormatAmount(decimal value) => IsArabic ? $"{value:F2} ج.م." : $"${value:F2}";
    private QuestPDF.Infrastructure.IContainer AlignNumeric(QuestPDF.Infrastructure.IContainer c) => IsArabic ? c.AlignRight() : c.AlignLeft();
    private QuestPDF.Infrastructure.IContainer AlignText(QuestPDF.Infrastructure.IContainer c) => IsArabic ? c.AlignRight() : c.AlignLeft();

    public async Task<string?> GeneratePayrollPdfAsync(List<MonthlyAttendanceSummary> summaries, DateTime month, MonthlyAttendanceTotals totals)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            var monthName = IsArabic ? month.ToString("MMMM yyyy", new CultureInfo("ar")) : month.ToString("MMMM yyyy");
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
                        .Text(M($"Payroll Report - {monthName}", $"تقرير الرواتب - {monthName}"))
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
                                            columns.RelativeColumn(1); // Worked Days
                                            columns.RelativeColumn(1); // Rest Days
                                            columns.RelativeColumn(1.2f); // Rest Payout
                                            columns.RelativeColumn(1.4f); // Absence Deductions
                                            columns.RelativeColumn(1.2f); // Expenses
                                            columns.RelativeColumn(1); // OT Hours
                                            columns.RelativeColumn(1.5f); // Total Pay
                                        });

                                        table.Header(header =>
                                        {
                                            if (IsArabic)
                                            {
                                                // RTL: Reverse column order
                                                header.Cell().Element(CellStyleRTL).Text(M("Total Pay", "إجمالي الراتب")).Bold();
                                                header.Cell().Element(CellStyleRTL).Text(M("OT Hours", "ساعات إضافية")).Bold();
                                                header.Cell().Element(CellStyleRTL).Text(M("Expenses", "المصاريف")).Bold();
                                                header.Cell().Element(CellStyleRTL).Text(M("Absence Deductions", "خصومات الغياب")).Bold();
                                                header.Cell().Element(CellStyleRTL).Text(M("Rest Payout", "مكافأة أيام الراحة")).Bold();
                                                header.Cell().Element(CellStyleRTL).Text(M("Rest", "أيام الراحة")).Bold();
                                                header.Cell().Element(CellStyleRTL).Text(M("Worked", "أيام العمل")).Bold();
                                                header.Cell().Element(CellStyleRTL).Text(M("Name", "الاسم")).Bold();
                                            }
                                            else
                                            {
                                                header.Cell().Element(CellStyle).Text(M("Name", "الاسم")).Bold();
                                                header.Cell().Element(CellStyle).Text(M("Worked", "أيام العمل")).Bold();
                                                header.Cell().Element(CellStyle).Text(M("Rest", "أيام الراحة")).Bold();
                                                header.Cell().Element(CellStyle).Text(M("Rest Payout", "مكافأة أيام الراحة")).Bold();
                                                header.Cell().Element(CellStyle).Text(M("Absence Deductions", "خصومات الغياب")).Bold();
                                                header.Cell().Element(CellStyle).Text(M("Expenses", "المصاريف")).Bold();
                                                header.Cell().Element(CellStyle).Text(M("OT Hours", "ساعات إضافية")).Bold();
                                                header.Cell().Element(CellStyle).Text(M("Total Pay", "إجمالي الراتب")).Bold();
                                            }
                                        });

                                        foreach (var summary in pageSummaries)
                                        {
                                            if (IsArabic)
                                            {
                                                // RTL: Reverse column order
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(summary.Payroll));
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text($"{summary.OvertimeHours:F1}");
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text($"-{FormatAmount(summary.ExpensesTotal)}");
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text($"-{FormatAmount(summary.AbsenceDeductions)}");
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(summary.RestDayPayout));
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text(summary.EarnedRestDays.ToString());
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text(summary.WorkedDays.ToString());
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text(summary.UserName);
                                            }
                                            else
                                            {
                                                table.Cell().Element(CellStyle).Text(summary.UserName);
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text(summary.WorkedDays.ToString());
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text(summary.EarnedRestDays.ToString());
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(summary.RestDayPayout));
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text($"-{FormatAmount(summary.AbsenceDeductions)}");
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text($"-{FormatAmount(summary.ExpensesTotal)}");
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text($"{summary.OvertimeHours:F1}");
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(summary.Payroll));
                                            }
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
                                    if (IsArabic)
                                    {
                                        totalsColumn.Item().AlignRight().Text(M("Totals", "الإجماليات")).Bold().FontSize(11);
                                        totalsColumn.Item().AlignRight().Text(M($"Total Present Days: {totals.TotalPresentDays}", $"إجمالي أيام الحضور: {totals.TotalPresentDays}"));
                                        totalsColumn.Item().AlignRight().Text(M($"Total Absent Days: {totals.TotalAbsentDays}", $"إجمالي أيام الغياب: {totals.TotalAbsentDays}"));
                                        totalsColumn.Item().AlignRight().Text(M($"Total OT Hours: {totals.TotalOvertimeHours:F1}", $"إجمالي الساعات الإضافية: {totals.TotalOvertimeHours:F1}"));
                                        totalsColumn.Item().AlignRight().Text(M($"Rest Payout: {FormatAmount(totals.TotalRestPayout)}", $"مكافأة الراحة: {FormatAmount(totals.TotalRestPayout)}"));
                                        totalsColumn.Item().AlignRight().Text(M($"Absence Deductions: -{FormatAmount(totals.TotalAbsenceDeductions)}", $"خصومات الغياب: -{FormatAmount(totals.TotalAbsenceDeductions)}"));
                                        totalsColumn.Item().AlignRight().Text(M($"Total Payroll: {FormatAmount(totals.TotalPayroll)}", $"إجمالي الرواتب: {FormatAmount(totals.TotalPayroll)}")).Bold();
                                    }
                                    else
                                    {
                                        totalsColumn.Item().Text(M("Totals", "الإجماليات")).Bold().FontSize(11);
                                        totalsColumn.Item().Text(M($"Total Present Days: {totals.TotalPresentDays}", $"إجمالي أيام الحضور: {totals.TotalPresentDays}"));
                                        totalsColumn.Item().Text(M($"Total Absent Days: {totals.TotalAbsentDays}", $"إجمالي أيام الغياب: {totals.TotalAbsentDays}"));
                                        totalsColumn.Item().Text(M($"Total OT Hours: {totals.TotalOvertimeHours:F1}", $"إجمالي الساعات الإضافية: {totals.TotalOvertimeHours:F1}"));
                                        totalsColumn.Item().Text(M($"Rest Payout: {FormatAmount(totals.TotalRestPayout)}", $"مكافأة الراحة: {FormatAmount(totals.TotalRestPayout)}"));
                                        totalsColumn.Item().Text(M($"Absence Deductions: -{FormatAmount(totals.TotalAbsenceDeductions)}", $"خصومات الغياب: -{FormatAmount(totals.TotalAbsenceDeductions)}"));
                                        totalsColumn.Item().Text(M($"Total Payroll: {FormatAmount(totals.TotalPayroll)}", $"إجمالي الرواتب: {FormatAmount(totals.TotalPayroll)}")).Bold();
                                    }
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            if (IsArabic)
                            {
                                x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                                x.Span(M("Generated on ", " تم الإنشاء في "));
                            }
                            else
                            {
                                x.Span(M("Generated on ", "تم الإنشاء في "));
                                x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                            }
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

    public async Task<string?> GenerateEmployeePayrollPdfAsync(MonthlyAttendanceSummary summary, DateTime month, List<EmployeeExpense> expenses)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var monthName = IsArabic ? month.ToString("MMMM yyyy", new CultureInfo("ar")) : month.ToString("MMMM yyyy");
            var fileName = $"Payroll_{summary.UserName}_{month:yyyy-MM}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var expensesTotal = expenses?.Sum(e => e.Amount) ?? 0m;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text(M($"Employee Payroll - {summary.UserName} - {monthName}", $"مسير رواتب الموظف - {summary.UserName} - {monthName}")).FontSize(16).Bold().AlignCenter();

                    page.Content().Column(col =>
                    {
                        col.Item().Border(1).Padding(8).Column(c =>
                        {
                            if (IsArabic)
                            {
                                var net = summary.Payroll;
                                c.Item().AlignRight().Text(M($"Net Pay: {FormatAmount(net)}", $"الصافي: {FormatAmount(net)}")).Bold();
                                c.Item().AlignRight().Text(M($"Overtime Hours: {summary.OvertimeHours:F1}", $"ساعات إضافية: {summary.OvertimeHours:F1}"));
                                c.Item().AlignRight().Text(M($"Expenses: -{FormatAmount(expensesTotal)}", $"المصاريف: -{FormatAmount(expensesTotal)}"));
                                c.Item().AlignRight().Text(M($"Absence Deductions: -{FormatAmount(summary.AbsenceDeductions)}", $"خصومات الغياب: -{FormatAmount(summary.AbsenceDeductions)}"));
                                c.Item().AlignRight().Text(M($"Rest Payout: {FormatAmount(summary.RestDayPayout)}", $"مكافأة الراحة: {FormatAmount(summary.RestDayPayout)}"));
                                c.Item().AlignRight().Text(M($"Rest Days: {summary.EarnedRestDays}", $"أيام الراحة: {summary.EarnedRestDays}"));
                                c.Item().AlignRight().Text(M($"Worked Days: {summary.WorkedDays}", $"أيام العمل: {summary.WorkedDays}"));
                            }
                            else
                            {
                                c.Item().Text(M($"Worked Days: {summary.WorkedDays}", $"أيام العمل: {summary.WorkedDays}"));
                                c.Item().Text(M($"Rest Days: {summary.EarnedRestDays}", $"أيام الراحة: {summary.EarnedRestDays}"));
                                c.Item().Text(M($"Rest Payout: {FormatAmount(summary.RestDayPayout)}", $"مكافأة الراحة: {FormatAmount(summary.RestDayPayout)}"));
                                c.Item().Text(M($"Absence Deductions: -{FormatAmount(summary.AbsenceDeductions)}", $"خصومات الغياب: -{FormatAmount(summary.AbsenceDeductions)}"));
                                c.Item().Text(M($"Expenses: -{FormatAmount(expensesTotal)}", $"المصاريف: -{FormatAmount(expensesTotal)}"));
                                c.Item().Text(M($"Overtime Hours: {summary.OvertimeHours:F1}", $"ساعات إضافية: {summary.OvertimeHours:F1}"));
                                var net = summary.Payroll;
                                c.Item().Text(M($"Net Pay: {FormatAmount(net)}", $"الصافي: {FormatAmount(net)}")).Bold();
                            }
                        });

                        if (IsArabic)
                        {
                            col.Item().PaddingTop(10).AlignRight().Text(M("Expenses", "المصاريف")).Bold();
                        }
                        else
                        {
                            col.Item().PaddingTop(10).AlignLeft().Text(M("Expenses", "المصاريف")).Bold();
                        }
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                if (IsArabic)
                                {
                                    columns.RelativeColumn(2); // Notes (rightmost in RTL)
                                    columns.RelativeColumn(1); // Amount
                                    columns.RelativeColumn(1); // Category
                                    columns.RelativeColumn(1); // Date (leftmost in RTL)
                                }
                                else
                                {
                                    columns.RelativeColumn(1); // Date
                                    columns.RelativeColumn(1); // Category
                                    columns.RelativeColumn(1); // Amount
                                    columns.RelativeColumn(2); // Notes
                                }
                            });

                            table.Header(header =>
                            {
                                if (IsArabic)
                                {
                                    header.Cell().Element(CellStyleRTL).Text(M("Notes", "الملاحظات")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("Amount", "المبلغ")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("Category", "الفئة")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("Date", "التاريخ")).Bold();
                                }
                                else
                                {
                                    header.Cell().Element(CellStyle).Text(M("Date", "التاريخ")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Category", "الفئة")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Amount", "المبلغ")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Notes", "الملاحظات")).Bold();
                                }
                            });

                            foreach (var e in expenses ?? new List<EmployeeExpense>())
                            {
                                if (IsArabic)
                                {
                                    AlignText(table.Cell().Element(CellStyleRTL)).Text(e.Notes ?? "");
                                    AlignText(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(e.Amount));
                                    AlignText(table.Cell().Element(CellStyleRTL)).Text(e.Category ?? "");
                                    AlignText(table.Cell().Element(CellStyleRTL)).Text(e.ExpenseDate.ToString("yyyy-MM-dd"));
                                }
                                else
                                {
                                    table.Cell().Element(CellStyle).Text(e.ExpenseDate.ToString("yyyy-MM-dd"));
                                    table.Cell().Element(CellStyle).Text(e.Category);
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(e.Amount));
                                    table.Cell().Element(CellStyle).Text(e.Notes);
                                }
                            }
                        });

                        if (IsArabic)
                        {
                            col.Item().PaddingTop(8).AlignRight().Text(M($"Expenses Total: {FormatAmount(expensesTotal)}", $"إجمالي المصاريف: {FormatAmount(expensesTotal)}")).Bold();
                        }
                        else
                        {
                            col.Item().PaddingTop(8).AlignLeft().Text(M($"Expenses Total: {FormatAmount(expensesTotal)}", $"إجمالي المصاريف: {FormatAmount(expensesTotal)}")).Bold();
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        if (IsArabic)
                        {
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                            x.Span(M("Generated on ", " تم الإنشاء في "));
                        }
                        else
                        {
                            x.Span(M("Generated on ", "تم الإنشاء في "));
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                        }
                    });
                });
            });

            await Task.Run(() => document.GeneratePdf(filePath));
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating employee payroll PDF: {ex.Message}");
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
    
    private QuestPDF.Infrastructure.IContainer CellStyleRTL(QuestPDF.Infrastructure.IContainer container)
    {
        return container
            .Border(1)
            .Padding(3)
            .AlignRight()
            .AlignMiddle();
    }
}
