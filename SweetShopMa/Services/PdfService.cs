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
    #region Constants
    private const int UsersPerPage = 10;
    private const int HoursPerWorkDay = 8;
    #endregion

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
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            
            var monthName = IsArabic ? month.ToString("MMMM yyyy", new CultureInfo("ar")) : month.ToString("MMMM yyyy");
            var fileName = $"Payroll_{month:yyyy-MM}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).Fallback(d => d.FontFamily("Segoe UI Emoji")));

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
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(summary.ExpensesTotal));
                                                AlignText(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(summary.AbsenceDeductions));
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
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(summary.AbsenceDeductions));
                                                AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(summary.ExpensesTotal));
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
                                        totalsColumn.Item().AlignRight().Text(M($"Rest Payout: +{FormatAmount(totals.TotalRestPayout)}", $"مكافأة الراحة: +{FormatAmount(totals.TotalRestPayout)}"));
                                        totalsColumn.Item().AlignRight().Text(M($"Absence Deductions: {FormatAmount(totals.TotalAbsenceDeductions)}", $"خصومات الغياب: {FormatAmount(totals.TotalAbsenceDeductions)}"));
                                        totalsColumn.Item().AlignRight().Text(M($"Total Payroll: {FormatAmount(totals.TotalPayroll)}", $"إجمالي الرواتب: {FormatAmount(totals.TotalPayroll)}")).Bold();
                                    }
                                    else
                                    {
                                        totalsColumn.Item().Text(M("Totals", "الإجماليات")).Bold().FontSize(11);
                                        totalsColumn.Item().Text(M($"Total Present Days: {totals.TotalPresentDays}", $"إجمالي أيام الحضور: {totals.TotalPresentDays}"));
                                        totalsColumn.Item().Text(M($"Total Absent Days: {totals.TotalAbsentDays}", $"إجمالي أيام الغياب: {totals.TotalAbsentDays}"));
                                        totalsColumn.Item().Text(M($"Total OT Hours: {totals.TotalOvertimeHours:F1}", $"إجمالي الساعات الإضافية: {totals.TotalOvertimeHours:F1}"));
                                        totalsColumn.Item().Text(M($"Rest Payout: +{FormatAmount(totals.TotalRestPayout)}", $"مكافأة الراحة: +{FormatAmount(totals.TotalRestPayout)}"));
                                        totalsColumn.Item().Text(M($"Absence Deductions: {FormatAmount(totals.TotalAbsenceDeductions)}", $"خصومات الغياب: {FormatAmount(totals.TotalAbsenceDeductions)}"));
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
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            var monthName = IsArabic ? month.ToString("MMMM yyyy", new CultureInfo("ar")) : month.ToString("MMMM yyyy");
            var fileName = $"Payroll_{summary.UserName}_{month:yyyy-MM}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Ensure expenses list is not null and calculate total safely
            var safeExpenses = expenses ?? new List<EmployeeExpense>();
            var expensesTotal = safeExpenses.Sum(e => e?.Amount ?? 0m);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).Fallback(d => d.FontFamily("Segoe UI Emoji")));

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
                                c.Item().AlignRight().Text(M($"Expenses (deducted): {FormatAmount(expensesTotal)}", $"المصاريف (مخصومة): {FormatAmount(expensesTotal)}"));
                                c.Item().AlignRight().Text(M($"Absence Deductions: {FormatAmount(summary.AbsenceDeductions)}", $"خصومات الغياب: {FormatAmount(summary.AbsenceDeductions)}"));
                                c.Item().AlignRight().Text(M($"Rest Payout: {FormatAmount(summary.RestDayPayout)}", $"مكافأة الراحة: {FormatAmount(summary.RestDayPayout)}"));
                                c.Item().AlignRight().Text(M($"Rest Days: {summary.EarnedRestDays}", $"أيام الراحة: {summary.EarnedRestDays}"));
                                c.Item().AlignRight().Text(M($"Worked Days: {summary.WorkedDays}", $"أيام العمل: {summary.WorkedDays}"));
                            }
                            else
                            {
                                c.Item().Text(M($"Worked Days: {summary.WorkedDays}", $"أيام العمل: {summary.WorkedDays}"));
                                c.Item().Text(M($"Rest Days: {summary.EarnedRestDays}", $"أيام الراحة: {summary.EarnedRestDays}"));
                                c.Item().Text(M($"Rest Payout: {FormatAmount(summary.RestDayPayout)}", $"مكافأة الراحة: {FormatAmount(summary.RestDayPayout)}"));
                                c.Item().Text(M($"Absence Deductions: {FormatAmount(summary.AbsenceDeductions)}", $"خصومات الغياب: {FormatAmount(summary.AbsenceDeductions)}"));
                                c.Item().Text(M($"Expenses (deducted): {FormatAmount(expensesTotal)}", $"المصاريف (مخصومة): {FormatAmount(expensesTotal)}"));
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

                            foreach (var e in safeExpenses)
                            {
                                if (e == null) continue;

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
                                    table.Cell().Element(CellStyle).Text(e.Category ?? "");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(e.Amount));
                                    table.Cell().Element(CellStyle).Text(e.Notes ?? "");
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

    public async Task<string?> GenerateAttendancePdfAsync(List<AttendanceRecord> records, DateTime month)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var monthName = IsArabic ? month.ToString("MMMM yyyy", new CultureInfo("ar")) : month.ToString("MMMM yyyy");
            var fileName = $"Attendance_{month:yyyy-MM}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .Text(M($"Attendance Report - {monthName}", $"تقرير الحضور - {monthName}"))
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    page.Content().Column(column =>
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.2f);
                            });

                            table.Header(header =>
                            {
                                if (IsArabic)
                                {
                                    header.Cell().Element(CellStyleRTL).Text(M("User", "الموظف")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("Date", "التاريخ")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("Status", "الحالة")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("Regular", "العادي")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("OT", "الإضافي")).Bold();
                                    header.Cell().Element(CellStyleRTL).Text(M("Daily Pay", "أجر اليوم")).Bold();
                                }
                                else
                                {
                                    header.Cell().Element(CellStyle).Text(M("User", "الموظف")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Date", "التاريخ")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Status", "الحالة")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Regular", "العادي")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("OT", "الإضافي")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Daily Pay", "أجر اليوم")).Bold();
                                }
                            });

                            foreach (var r in records ?? new List<AttendanceRecord>())
                            {
                                if (IsArabic)
                                {
                                    AlignText(table.Cell().Element(CellStyleRTL)).Text(r.UserName);
                                    AlignText(table.Cell().Element(CellStyleRTL)).Text(r.Date.ToString("yyyy-MM-dd"));
                                    AlignText(table.Cell().Element(CellStyleRTL)).Text(r.Status);
                                    AlignNumeric(table.Cell().Element(CellStyleRTL)).Text($"{r.RegularHours:F1}");
                                    AlignNumeric(table.Cell().Element(CellStyleRTL)).Text($"{r.OvertimeHours:F1}");
                                    AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(r.DailyPay));
                                }
                                else
                                {
                                    table.Cell().Element(CellStyle).Text(r.UserName);
                                    table.Cell().Element(CellStyle).Text(r.Date.ToString("yyyy-MM-dd"));
                                    table.Cell().Element(CellStyle).Text(r.Status);
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text($"{r.RegularHours:F1}");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text($"{r.OvertimeHours:F1}");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(r.DailyPay));
                                }
                            }
                        });
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
            System.Diagnostics.Debug.WriteLine($"Error generating attendance PDF: {ex.Message}");
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

    public async Task<string?> GenerateSalesReportPdfAsync(
        decimal totalSales,
        int totalOrders,
        decimal averageOrderValue,
        decimal totalItemsSold,
        decimal last7DaysSales,
        List<ViewModels.ProductReportItem> topProducts,
        List<Models.Order> recentOrders)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            var today = DateTime.Today;
            var fileName = $"SalesReport_{today:yyyy-MM-dd}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).Fallback(d => d.FontFamily("Segoe UI Emoji")));

                    page.Header()
                        .Text(M($"Sales Report - {today:MMMM yyyy}", $"تقرير المبيعات - {today:MMMM yyyy}"))
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Column(column =>
                        {
                            // Summary Metrics Section
                            column.Item()
                                .Border(1)
                                .Padding(8)
                                .Column(metrics =>
                                {
                                    if (IsArabic)
                                    {
                                        metrics.Item().AlignRight().Text(M("Sales Summary", "ملخص المبيعات")).Bold().FontSize(11);
                                        metrics.Item().PaddingTop(3).AlignRight().Text(M($"Total Sales: {FormatAmount(totalSales)}", $"إجمالي المبيعات: {FormatAmount(totalSales)}"));
                                        metrics.Item().AlignRight().Text(M($"Total Orders: {totalOrders}", $"إجمالي الطلبات: {totalOrders}"));
                                        metrics.Item().AlignRight().Text(M($"Average Order Value: {FormatAmount(averageOrderValue)}", $"متوسط قيمة الطلب: {FormatAmount(averageOrderValue)}"));
                                        metrics.Item().AlignRight().Text(M($"Total Items Sold: {totalItemsSold:F0}", $"إجمالي العناصر المباعة: {totalItemsSold:F0}"));
                                        metrics.Item().AlignRight().Text(M($"Last 7 Days Sales: {FormatAmount(last7DaysSales)}", $"مبيعات آخر 7 أيام: {FormatAmount(last7DaysSales)}"));
                                    }
                                    else
                                    {
                                        metrics.Item().Text(M("Sales Summary", "ملخص المبيعات")).Bold().FontSize(11);
                                        metrics.Item().PaddingTop(3).Text(M($"Total Sales: {FormatAmount(totalSales)}", $"إجمالي المبيعات: {FormatAmount(totalSales)}"));
                                        metrics.Item().Text(M($"Total Orders: {totalOrders}", $"إجمالي الطلبات: {totalOrders}"));
                                        metrics.Item().Text(M($"Average Order Value: {FormatAmount(averageOrderValue)}", $"متوسط قيمة الطلب: {FormatAmount(averageOrderValue)}"));
                                        metrics.Item().Text(M($"Total Items Sold: {totalItemsSold:F0}", $"إجمالي العناصر المباعة: {totalItemsSold:F0}"));
                                        metrics.Item().Text(M($"Last 7 Days Sales: {FormatAmount(last7DaysSales)}", $"مبيعات آخر 7 أيام: {FormatAmount(last7DaysSales)}"));
                                    }
                                });

                            // Top Products Section
                            if (topProducts != null && topProducts.Any())
                            {
                                column.Item().PaddingTop(10);
                                if (IsArabic)
                                {
                                    column.Item().AlignRight().Text(M("Top Products", "أكثر المنتجات مبيعاً")).Bold().FontSize(11);
                                }
                                else
                                {
                                    column.Item().Text(M("Top Products", "أكثر المنتجات مبيعاً")).Bold().FontSize(11);
                                }

                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        if (IsArabic)
                                        {
                                            columns.RelativeColumn(1.5f); // Total Sales (rightmost in RTL)
                                            columns.RelativeColumn(1); // Quantity
                                            columns.RelativeColumn(0.8f); // Unit
                                            columns.RelativeColumn(2); // Name (leftmost in RTL)
                                        }
                                        else
                                        {
                                            columns.RelativeColumn(2); // Name
                                            columns.RelativeColumn(0.8f); // Unit
                                            columns.RelativeColumn(1); // Quantity
                                            columns.RelativeColumn(1.5f); // Total Sales
                                        }
                                    });

                                    table.Header(header =>
                                    {
                                        if (IsArabic)
                                        {
                                            header.Cell().Element(CellStyleRTL).Text(M("Total Sales", "إجمالي المبيعات")).Bold();
                                            header.Cell().Element(CellStyleRTL).Text(M("Qty", "الكمية")).Bold();
                                            header.Cell().Element(CellStyleRTL).Text(M("Unit", "الوحدة")).Bold();
                                            header.Cell().Element(CellStyleRTL).Text(M("Name", "الاسم")).Bold();
                                        }
                                        else
                                        {
                                            header.Cell().Element(CellStyle).Text(M("Name", "الاسم")).Bold();
                                            header.Cell().Element(CellStyle).Text(M("Unit", "الوحدة")).Bold();
                                            header.Cell().Element(CellStyle).Text(M("Qty", "الكمية")).Bold();
                                            header.Cell().Element(CellStyle).Text(M("Total Sales", "إجمالي المبيعات")).Bold();
                                        }
                                    });

                                    foreach (var product in topProducts.Take(10))
                                    {
                                        if (IsArabic)
                                        {
                                            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(product.TotalSales));
                                            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(product.QuantityDisplay);
                                            table.Cell().Element(CellStyleRTL).Text(product.UnitLabel);
                                            table.Cell().Element(CellStyleRTL).Text($"{product.Emoji} {product.Name}");
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).Text($"{product.Emoji} {product.Name}");
                                            table.Cell().Element(CellStyle).Text(product.UnitLabel);
                                            AlignNumeric(table.Cell().Element(CellStyle)).Text(product.QuantityDisplay);
                                            AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.TotalSales));
                                        }
                                    }
                                });
                            }

                            // Recent Orders Section
                            if (recentOrders != null && recentOrders.Any())
                            {
                                column.Item().PaddingTop(10);
                                if (IsArabic)
                                {
                                    column.Item().AlignRight().Text(M("Recent Orders", "الطلبات الأخيرة")).Bold().FontSize(11);
                                }
                                else
                                {
                                    column.Item().Text(M("Recent Orders", "الطلبات الأخيرة")).Bold().FontSize(11);
                                }

                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        if (IsArabic)
                                        {
                                            columns.RelativeColumn(1.2f); // Total (rightmost in RTL)
                                            columns.RelativeColumn(0.8f); // Items
                                            columns.RelativeColumn(1.5f); // Cashier
                                            columns.RelativeColumn(2); // Date (leftmost in RTL)
                                        }
                                        else
                                        {
                                            columns.RelativeColumn(2); // Date
                                            columns.RelativeColumn(1.5f); // Cashier
                                            columns.RelativeColumn(0.8f); // Items
                                            columns.RelativeColumn(1.2f); // Total
                                        }
                                    });

                                    table.Header(header =>
                                    {
                                        if (IsArabic)
                                        {
                                            header.Cell().Element(CellStyleRTL).Text(M("Total", "الإجمالي")).Bold();
                                            header.Cell().Element(CellStyleRTL).Text(M("Items", "العناصر")).Bold();
                                            header.Cell().Element(CellStyleRTL).Text(M("Cashier", "الكاشير")).Bold();
                                            header.Cell().Element(CellStyleRTL).Text(M("Date", "التاريخ")).Bold();
                                        }
                                        else
                                        {
                                            header.Cell().Element(CellStyle).Text(M("Date", "التاريخ")).Bold();
                                            header.Cell().Element(CellStyle).Text(M("Cashier", "الكاشير")).Bold();
                                            header.Cell().Element(CellStyle).Text(M("Items", "العناصر")).Bold();
                                            header.Cell().Element(CellStyle).Text(M("Total", "الإجمالي")).Bold();
                                        }
                                    });

                                    foreach (var order in recentOrders.Take(20))
                                    {
                                        if (IsArabic)
                                        {
                                            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(order.Total));
                                            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(order.ItemCount.ToString());
                                            table.Cell().Element(CellStyleRTL).Text(order.UserName ?? "");
                                            table.Cell().Element(CellStyleRTL).Text(order.OrderDate.ToString("yyyy-MM-dd HH:mm"));
                                        }
                                        else
                                        {
                                            table.Cell().Element(CellStyle).Text(order.OrderDate.ToString("yyyy-MM-dd HH:mm"));
                                            table.Cell().Element(CellStyle).Text(order.UserName ?? "");
                                            AlignNumeric(table.Cell().Element(CellStyle)).Text(order.ItemCount.ToString());
                                            AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(order.Total));
                                        }
                                    }
                                });
                            }
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

            await Task.Run(() => document.GeneratePdf(filePath));
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating sales report PDF: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GenerateInventoryReportPdfAsync(
        List<Models.Product> products,
        decimal lowStockThreshold = 10)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            var today = DateTime.Today;
            var fileName = $"InventoryReport_{today:yyyy-MM-dd}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Separate products into categories
            var outOfStock = products.Where(p => p.Stock <= 0).OrderBy(p => p.Name).ToList();
            var lowStock = products.Where(p => p.Stock > 0 && p.Stock <= lowStockThreshold).OrderBy(p => p.Stock).ToList();
            var inStock = products.Where(p => p.Stock > lowStockThreshold).OrderByDescending(p => p.Stock).ToList();

            // Calculate totals
            var totalProducts = products.Count;
            var totalValue = products.Sum(p => p.Stock * p.Price);
            var totalStockUnits = products.Where(p => !p.IsSoldByWeight).Sum(p => p.Stock);
            var totalStockWeight = products.Where(p => p.IsSoldByWeight).Sum(p => p.Stock);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).Fallback(d => d.FontFamily("Segoe UI Emoji")));

                    page.Header()
                        .Text(M($"Inventory Report - {today:MMMM yyyy}", $"تقرير المخزون - {today:MMMM yyyy}"))
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Column(column =>
                        {
                            // Summary Metrics Section
                            column.Item()
                                .Border(1)
                                .Padding(8)
                                .Column(metrics =>
                                {
                                    if (IsArabic)
                                    {
                                        metrics.Item().AlignRight().Text(M("Inventory Summary", "ملخص المخزون")).Bold().FontSize(11);
                                        metrics.Item().PaddingTop(3).AlignRight().Text(M($"Total Products: {totalProducts}", $"إجمالي المنتجات: {totalProducts}"));
                                        metrics.Item().AlignRight().Text(M($"Total Stock Value: {FormatAmount(totalValue)}", $"إجمالي قيمة المخزون: {FormatAmount(totalValue)}"));
                                        metrics.Item().AlignRight().Text(M($"Total Units: {totalStockUnits:F0} PCS", $"إجمالي القطع: {totalStockUnits:F0} قطعة"));
                                        metrics.Item().AlignRight().Text(M($"Total Weight: {totalStockWeight:F2} KGS", $"إجمالي الوزن: {totalStockWeight:F2} كجم"));
                                        metrics.Item().AlignRight().Text(M($"Out of Stock: {outOfStock.Count} items", $"نفد المخزون: {outOfStock.Count} منتج")).FontColor(outOfStock.Count > 0 ? "#c00000" : "#1f7a4d");
                                        metrics.Item().AlignRight().Text(M($"Low Stock: {lowStock.Count} items", $"مخزون منخفض: {lowStock.Count} منتج")).FontColor(lowStock.Count > 0 ? "#ff9800" : "#1f7a4d");
                                    }
                                    else
                                    {
                                        metrics.Item().Text(M("Inventory Summary", "ملخص المخزون")).Bold().FontSize(11);
                                        metrics.Item().PaddingTop(3).Text(M($"Total Products: {totalProducts}", $"إجمالي المنتجات: {totalProducts}"));
                                        metrics.Item().Text(M($"Total Stock Value: {FormatAmount(totalValue)}", $"إجمالي قيمة المخزون: {FormatAmount(totalValue)}"));
                                        metrics.Item().Text(M($"Total Units: {totalStockUnits:F0} PCS", $"إجمالي القطع: {totalStockUnits:F0} قطعة"));
                                        metrics.Item().Text(M($"Total Weight: {totalStockWeight:F2} KGS", $"إجمالي الوزن: {totalStockWeight:F2} كجم"));
                                        metrics.Item().Text(M($"Out of Stock: {outOfStock.Count} items", $"نفد المخزون: {outOfStock.Count} منتج")).FontColor(outOfStock.Count > 0 ? "#c00000" : "#1f7a4d");
                                        metrics.Item().Text(M($"Low Stock: {lowStock.Count} items", $"مخزون منخفض: {lowStock.Count} منتج")).FontColor(lowStock.Count > 0 ? "#ff9800" : "#1f7a4d");
                                    }
                                });

                            // Out of Stock Section
                            if (outOfStock.Any())
                            {
                                column.Item().PaddingTop(10);
                                if (IsArabic)
                                {
                                    column.Item().AlignRight().Text(M("⚠️ Out of Stock Items", "⚠️ المنتجات التي نفد مخزونها")).Bold().FontSize(11).FontColor("#c00000");
                                }
                                else
                                {
                                    column.Item().Text(M("⚠️ Out of Stock Items", "⚠️ المنتجات التي نفد مخزونها")).Bold().FontSize(11).FontColor("#c00000");
                                }

                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    DefineProductTableColumns(table);
                                    AddProductTableHeader(table);
                                    foreach (var product in outOfStock.Take(15))
                                    {
                                        AddProductTableRow(table, product, "#c00000");
                                    }
                                });
                            }

                            // Low Stock Section
                            if (lowStock.Any())
                            {
                                column.Item().PaddingTop(10);
                                if (IsArabic)
                                {
                                    column.Item().AlignRight().Text(M("⚡ Low Stock Items", "⚡ المنتجات ذات المخزون المنخفض")).Bold().FontSize(11).FontColor("#ff9800");
                                }
                                else
                                {
                                    column.Item().Text(M("⚡ Low Stock Items", "⚡ المنتجات ذات المخزون المنخفض")).Bold().FontSize(11).FontColor("#ff9800");
                                }

                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    DefineProductTableColumns(table);
                                    AddProductTableHeader(table);
                                    foreach (var product in lowStock.Take(15))
                                    {
                                        AddProductTableRow(table, product, "#ff9800");
                                    }
                                });
                            }

                            // All Products Section
                            column.Item().PaddingTop(10);
                            if (IsArabic)
                            {
                                column.Item().AlignRight().Text(M("📦 All Products", "📦 جميع المنتجات")).Bold().FontSize(11);
                            }
                            else
                            {
                                column.Item().Text(M("📦 All Products", "📦 جميع المنتجات")).Bold().FontSize(11);
                            }

                            column.Item().PaddingTop(5).Table(table =>
                            {
                                DefineProductTableColumns(table);
                                AddProductTableHeader(table);
                                foreach (var product in products.OrderBy(p => p.Category).ThenBy(p => p.Name).Take(50))
                                {
                                    string fontColor = product.Stock <= 0 ? "#c00000" : 
                                                      product.Stock <= lowStockThreshold ? "#ff9800" : "#000000";
                                    AddProductTableRow(table, product, fontColor);
                                }
                            });

                            if (products.Count > 50)
                            {
                                column.Item().PaddingTop(5).AlignCenter().Text(M($"...and {products.Count - 50} more products", $"...و {products.Count - 50} منتج آخر")).FontSize(10).FontColor("#666");
                            }
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

            await Task.Run(() => document.GeneratePdf(filePath));
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating inventory report PDF: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GenerateInventoryReportByLocationPdfAsync(
        List<(Models.Product Product, decimal Stock)> productsWithStock,
        string locationName,
        decimal lowStockThreshold = 10)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
            var today = DateTime.Today;
            var safeLocationName = string.Join("_", locationName.Split(Path.GetInvalidFileNameChars()));
            var fileName = $"InventoryReport_{safeLocationName}_{today:yyyy-MM-dd}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            // Convert to list with stock values for filtering
            var products = productsWithStock.Select(ps => 
            {
                return new { Product = ps.Product, Stock = ps.Stock };
            }).ToList();

            // Separate products into categories based on location stock
            var outOfStock = products.Where(p => p.Stock <= 0).OrderBy(p => p.Product.Name).ToList();
            var lowStock = products.Where(p => p.Stock > 0 && p.Stock <= lowStockThreshold).OrderBy(p => p.Stock).ToList();
            var inStock = products.Where(p => p.Stock > lowStockThreshold).OrderByDescending(p => p.Stock).ToList();

            // Calculate totals
            var totalProducts = products.Count;
            var totalValue = products.Sum(p => p.Stock * p.Product.Price);
            var totalStockUnits = products.Where(p => !p.Product.IsSoldByWeight).Sum(p => p.Stock);
            var totalStockWeight = products.Where(p => p.Product.IsSoldByWeight).Sum(p => p.Stock);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9).Fallback(d => d.FontFamily("Segoe UI Emoji")));

                    page.Header()
                        .Text(M($"Inventory Report - {locationName} - {today:MMMM yyyy}", 
                               $"تقرير المخزون - {locationName} - {today:MMMM yyyy}"))
                        .FontSize(16)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Column(column =>
                        {
                            // Summary Metrics Section
                            column.Item()
                                .Border(1)
                                .Padding(8)
                                .Column(metrics =>
                                {
                                    if (IsArabic)
                                    {
                                        metrics.Item().AlignRight().Text(M("Inventory Summary", "ملخص المخزون")).Bold().FontSize(11);
                                        metrics.Item().PaddingTop(3).AlignRight().Text(M($"Branch: {locationName}", $"الفرع: {locationName}"));
                                        metrics.Item().AlignRight().Text(M($"Total Products: {totalProducts}", $"إجمالي المنتجات: {totalProducts}"));
                                        metrics.Item().AlignRight().Text(M($"Total Stock Value: {FormatAmount(totalValue)}", $"إجمالي قيمة المخزون: {FormatAmount(totalValue)}"));
                                        metrics.Item().AlignRight().Text(M($"Total Units: {totalStockUnits:F0} PCS", $"إجمالي القطع: {totalStockUnits:F0} قطعة"));
                                        metrics.Item().AlignRight().Text(M($"Total Weight: {totalStockWeight:F2} KGS", $"إجمالي الوزن: {totalStockWeight:F2} كجم"));
                                        metrics.Item().AlignRight().Text(M($"Out of Stock: {outOfStock.Count} items", $"نفد المخزون: {outOfStock.Count} منتج")).FontColor(outOfStock.Count > 0 ? "#c00000" : "#1f7a4d");
                                        metrics.Item().AlignRight().Text(M($"Low Stock: {lowStock.Count} items", $"مخزون منخفض: {lowStock.Count} منتج")).FontColor(lowStock.Count > 0 ? "#ff9800" : "#1f7a4d");
                                    }
                                    else
                                    {
                                        metrics.Item().Text(M("Inventory Summary", "ملخص المخزون")).Bold().FontSize(11);
                                        metrics.Item().PaddingTop(3).Text(M($"Branch: {locationName}", $"الفرع: {locationName}"));
                                        metrics.Item().Text(M($"Total Products: {totalProducts}", $"إجمالي المنتجات: {totalProducts}"));
                                        metrics.Item().Text(M($"Total Stock Value: {FormatAmount(totalValue)}", $"إجمالي قيمة المخزون: {FormatAmount(totalValue)}"));
                                        metrics.Item().Text(M($"Total Units: {totalStockUnits:F0} PCS", $"إجمالي القطع: {totalStockUnits:F0} قطعة"));
                                        metrics.Item().Text(M($"Total Weight: {totalStockWeight:F2} KGS", $"إجمالي الوزن: {totalStockWeight:F2} كجم"));
                                        metrics.Item().Text(M($"Out of Stock: {outOfStock.Count} items", $"نفد المخزون: {outOfStock.Count} منتج")).FontColor(outOfStock.Count > 0 ? "#c00000" : "#1f7a4d");
                                        metrics.Item().Text(M($"Low Stock: {lowStock.Count} items", $"مخزون منخفض: {lowStock.Count} منتج")).FontColor(lowStock.Count > 0 ? "#ff9800" : "#1f7a4d");
                                    }
                                });

                            // Out of Stock Section
                            if (outOfStock.Any())
                            {
                                column.Item().PaddingTop(10);
                                if (IsArabic)
                                {
                                    column.Item().AlignRight().Text(M("⚠️ Out of Stock Items", "⚠️ المنتجات التي نفد مخزونها")).Bold().FontSize(11).FontColor("#c00000");
                                }
                                else
                                {
                                    column.Item().Text(M("⚠️ Out of Stock Items", "⚠️ المنتجات التي نفد مخزونها")).Bold().FontSize(11).FontColor("#c00000");
                                }

                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    DefineProductTableColumns(table);
                                    AddProductTableHeader(table);
                                    foreach (var item in outOfStock.Take(15))
                                    {
                                        AddProductTableRowWithStock(table, item.Product, item.Stock, "#c00000");
                                    }
                                });
                            }

                            // Low Stock Section
                            if (lowStock.Any())
                            {
                                column.Item().PaddingTop(10);
                                if (IsArabic)
                                {
                                    column.Item().AlignRight().Text(M("⚡ Low Stock Items", "⚡ المنتجات ذات المخزون المنخفض")).Bold().FontSize(11).FontColor("#ff9800");
                                }
                                else
                                {
                                    column.Item().Text(M("⚡ Low Stock Items", "⚡ المنتجات ذات المخزون المنخفض")).Bold().FontSize(11).FontColor("#ff9800");
                                }

                                column.Item().PaddingTop(5).Table(table =>
                                {
                                    DefineProductTableColumns(table);
                                    AddProductTableHeader(table);
                                    foreach (var item in lowStock.Take(15))
                                    {
                                        AddProductTableRowWithStock(table, item.Product, item.Stock, "#ff9800");
                                    }
                                });
                            }

                            // All Products Section
                            column.Item().PaddingTop(10);
                            if (IsArabic)
                            {
                                column.Item().AlignRight().Text(M("📦 All Products", "📦 جميع المنتجات")).Bold().FontSize(11);
                            }
                            else
                            {
                                column.Item().Text(M("📦 All Products", "📦 جميع المنتجات")).Bold().FontSize(11);
                            }

                            column.Item().PaddingTop(5).Table(table =>
                            {
                                DefineProductTableColumns(table);
                                AddProductTableHeader(table);
                                foreach (var item in products.OrderBy(p => p.Product.Category).ThenBy(p => p.Product.Name).Take(50))
                                {
                                    string fontColor = item.Stock <= 0 ? "#c00000" : 
                                                      item.Stock <= lowStockThreshold ? "#ff9800" : "#000000";
                                    AddProductTableRowWithStock(table, item.Product, item.Stock, fontColor);
                                }
                            });

                            if (products.Count > 50)
                            {
                                column.Item().PaddingTop(5).AlignCenter().Text(M($"...and {products.Count - 50} more products", $"...و {products.Count - 50} منتج آخر")).FontSize(10).FontColor("#666");
                            }
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

            await Task.Run(() => document.GeneratePdf(filePath));
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error generating inventory report PDF: {ex.Message}");
            return null;
        }
    }

    private void AddProductTableRowWithStock(QuestPDF.Fluent.TableDescriptor table, Models.Product product, decimal stock, string fontColor)
    {
        var stockDisplay = product.IsSoldByWeight ? $"{stock:F2}" : $"{stock:F0}";
        var valueDisplay = FormatAmount(stock * product.Price);

        if (IsArabic)
        {
            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(valueDisplay).FontColor(fontColor);
            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(stockDisplay).FontColor(fontColor);
            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(product.Price));
            table.Cell().Element(CellStyleRTL).Text(product.UnitLabel);
            table.Cell().Element(CellStyleRTL).Text(product.Category ?? "");
            table.Cell().Element(CellStyleRTL).Text($"{product.Emoji} {product.Name}");
        }
        else
        {
            table.Cell().Element(CellStyle).Text($"{product.Emoji} {product.Name}");
            table.Cell().Element(CellStyle).Text(product.Category ?? "");
            table.Cell().Element(CellStyle).Text(product.UnitLabel);
            AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.Price));
            AlignNumeric(table.Cell().Element(CellStyle)).Text(stockDisplay).FontColor(fontColor);
            AlignNumeric(table.Cell().Element(CellStyle)).Text(valueDisplay).FontColor(fontColor);
        }
    }

    private void DefineProductTableColumns(QuestPDF.Fluent.TableDescriptor table)
    {
        table.ColumnsDefinition(columns =>
        {
            if (IsArabic)
            {
                columns.RelativeColumn(1.2f); // Value (rightmost in RTL)
                columns.RelativeColumn(1); // Stock
                columns.RelativeColumn(1); // Price
                columns.RelativeColumn(0.8f); // Unit
                columns.RelativeColumn(1); // Category
                columns.RelativeColumn(2); // Name (leftmost in RTL)
            }
            else
            {
                columns.RelativeColumn(2); // Name
                columns.RelativeColumn(1); // Category
                columns.RelativeColumn(0.8f); // Unit
                columns.RelativeColumn(1); // Price
                columns.RelativeColumn(1); // Stock
                columns.RelativeColumn(1.2f); // Value
            }
        });
    }

    private void AddProductTableHeader(QuestPDF.Fluent.TableDescriptor table)
    {
        table.Header(header =>
        {
            if (IsArabic)
            {
                header.Cell().Element(CellStyleRTL).Text(M("Value", "القيمة")).Bold();
                header.Cell().Element(CellStyleRTL).Text(M("Stock", "المخزون")).Bold();
                header.Cell().Element(CellStyleRTL).Text(M("Price", "السعر")).Bold();
                header.Cell().Element(CellStyleRTL).Text(M("Unit", "الوحدة")).Bold();
                header.Cell().Element(CellStyleRTL).Text(M("Category", "الفئة")).Bold();
                header.Cell().Element(CellStyleRTL).Text(M("Name", "الاسم")).Bold();
            }
            else
            {
                header.Cell().Element(CellStyle).Text(M("Name", "الاسم")).Bold();
                header.Cell().Element(CellStyle).Text(M("Category", "الفئة")).Bold();
                header.Cell().Element(CellStyle).Text(M("Unit", "الوحدة")).Bold();
                header.Cell().Element(CellStyle).Text(M("Price", "السعر")).Bold();
                header.Cell().Element(CellStyle).Text(M("Stock", "المخزون")).Bold();
                header.Cell().Element(CellStyle).Text(M("Value", "القيمة")).Bold();
            }
        });
    }

    private void AddProductTableRow(QuestPDF.Fluent.TableDescriptor table, Models.Product product, string fontColor)
    {
        var stockDisplay = product.IsSoldByWeight ? $"{product.Stock:F2}" : $"{product.Stock:F0}";
        var valueDisplay = FormatAmount(product.Stock * product.Price);

        if (IsArabic)
        {
            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(valueDisplay).FontColor(fontColor);
            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(stockDisplay).FontColor(fontColor);
            AlignNumeric(table.Cell().Element(CellStyleRTL)).Text(FormatAmount(product.Price));
            table.Cell().Element(CellStyleRTL).Text(product.UnitLabel);
            table.Cell().Element(CellStyleRTL).Text(product.Category ?? "");
            table.Cell().Element(CellStyleRTL).Text($"{product.Emoji} {product.Name}");
        }
        else
        {
            table.Cell().Element(CellStyle).Text($"{product.Emoji} {product.Name}");
            table.Cell().Element(CellStyle).Text(product.Category ?? "");
            table.Cell().Element(CellStyle).Text(product.UnitLabel);
            AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.Price));
            AlignNumeric(table.Cell().Element(CellStyle)).Text(stockDisplay).FontColor(fontColor);
            AlignNumeric(table.Cell().Element(CellStyle)).Text(valueDisplay).FontColor(fontColor);
        }
    }
}
