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

    public async Task<string?> GenerateSalesReportPdfAsync(List<Order> orders, List<ViewModels.ProductReportItem> topProducts, DateTime startDate, DateTime endDate)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var fileName = $"SalesReport_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var totalSales = orders?.Sum(o => o?.Total ?? 0m) ?? 0m;
            var totalOrders = orders?.Count ?? 0;
            var avgOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0m;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(M("Sales Report", "تقرير المبيعات")).FontSize(18).Bold().AlignCenter();
                        col.Item().Text(M($"Period: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}",
                            $"الفترة: {startDate:MMM dd, yyyy} - {endDate:MMM dd, yyyy}")).FontSize(12).AlignCenter();
                    });

                    page.Content().Column(col =>
                    {
                        // Summary Statistics
                        col.Item().PaddingTop(10).Text(M("Summary", "الملخص")).FontSize(14).Bold();
                        col.Item().Border(1).Padding(10).Column(c =>
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text(M("Total Sales:", "إجمالي المبيعات:")).Bold();
                                row.RelativeItem().Text(FormatAmount(totalSales)).AlignRight();
                            });
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text(M("Total Orders:", "إجمالي الطلبات:")).Bold();
                                row.RelativeItem().Text(totalOrders.ToString()).AlignRight();
                            });
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text(M("Average Order Value:", "متوسط قيمة الطلب:")).Bold();
                                row.RelativeItem().Text(FormatAmount(avgOrderValue)).AlignRight();
                            });
                        });

                        // Top Products
                        if (topProducts != null && topProducts.Any())
                        {
                            col.Item().PaddingTop(15).Text(M("Top Selling Products", "المنتجات الأكثر مبيعاً")).FontSize(14).Bold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Product
                                    columns.RelativeColumn(1); // Quantity
                                    columns.RelativeColumn(1); // Sales
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text(M("Product", "المنتج")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Quantity", "الكمية")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Sales", "المبيعات")).Bold();
                                });

                                foreach (var product in topProducts)
                                {
                                    table.Cell().Element(CellStyle).Text($"{product.Emoji} {product.Name}");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text($"{product.Quantity:F2} {product.UnitLabel}");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.TotalSales));
                                }
                            });
                        }

                        // Recent Orders
                        if (orders != null && orders.Any())
                        {
                            col.Item().PaddingTop(15).Text(M("Recent Orders", "الطلبات الأخيرة")).FontSize(14).Bold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1); // Order #
                                    columns.RelativeColumn(2); // Date
                                    columns.RelativeColumn(2); // Cashier
                                    columns.RelativeColumn(1); // Items
                                    columns.RelativeColumn(1); // Total
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text(M("Order #", "رقم الطلب")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Date", "التاريخ")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Cashier", "الموظف")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Items", "العناصر")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Total", "الإجمالي")).Bold();
                                });

                                foreach (var order in orders.OrderByDescending(o => o.OrderDate).Take(50))
                                {
                                    table.Cell().Element(CellStyle).Text($"#{order.Id}");
                                    table.Cell().Element(CellStyle).Text(order.OrderDate.ToString("MMM dd, HH:mm"));
                                    table.Cell().Element(CellStyle).Text(order.UserName ?? "");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(order.ItemCount.ToString());
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(order.Total));
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        if (IsArabic)
                        {
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                            x.Span(M(" Generated on ", " تم الإنشاء في "));
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

    public async Task<string?> GenerateInventoryReportPdfAsync(List<Product> products, DateTime reportDate)
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var fileName = $"InventoryReport_{reportDate:yyyy-MM-dd}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            var totalProducts = products?.Count ?? 0;
            var totalValue = products?.Sum(p => (p?.Stock ?? 0) * (p?.Price ?? 0)) ?? 0m;
            var lowStockProducts = products?.Where(p => p != null && p.Stock < 10).ToList() ?? new List<Product>();
            var outOfStockProducts = products?.Where(p => p != null && p.Stock == 0).ToList() ?? new List<Product>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(col =>
                    {
                        col.Item().Text(M("Inventory Report", "تقرير المخزون")).FontSize(18).Bold().AlignCenter();
                        col.Item().Text(M($"As of {reportDate:MMM dd, yyyy}", $"في تاريخ {reportDate:MMM dd, yyyy}")).FontSize(12).AlignCenter();
                    });

                    page.Content().Column(col =>
                    {
                        // Summary Statistics
                        col.Item().PaddingTop(10).Text(M("Summary", "الملخص")).FontSize(14).Bold();
                        col.Item().Border(1).Padding(10).Column(c =>
                        {
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text(M("Total Products:", "إجمالي المنتجات:")).Bold();
                                row.RelativeItem().Text(totalProducts.ToString()).AlignRight();
                            });
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text(M("Total Inventory Value:", "قيمة المخزون الإجمالية:")).Bold();
                                row.RelativeItem().Text(FormatAmount(totalValue)).AlignRight();
                            });
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text(M("Low Stock Items:", "منتجات منخفضة المخزون:")).Bold();
                                row.RelativeItem().Text(lowStockProducts.Count.ToString()).AlignRight().FontColor("#FF9800");
                            });
                            c.Item().Row(row =>
                            {
                                row.RelativeItem().Text(M("Out of Stock Items:", "منتجات نفذت من المخزون:")).Bold();
                                row.RelativeItem().Text(outOfStockProducts.Count.ToString()).AlignRight().FontColor("#c00000");
                            });
                        });

                        // Out of Stock Products
                        if (outOfStockProducts.Any())
                        {
                            col.Item().PaddingTop(15).Text(M("Out of Stock", "نفذ من المخزون")).FontSize(14).Bold().FontColor("#c00000");
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Product
                                    columns.RelativeColumn(1); // Category
                                    columns.RelativeColumn(1); // Price
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text(M("Product", "المنتج")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Category", "الفئة")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Price", "السعر")).Bold();
                                });

                                foreach (var product in outOfStockProducts)
                                {
                                    table.Cell().Element(CellStyle).Text($"{product.Emoji} {product.Name}");
                                    table.Cell().Element(CellStyle).Text(product.Category ?? "All");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.Price));
                                }
                            });
                        }

                        // Low Stock Products
                        if (lowStockProducts.Any())
                        {
                            col.Item().PaddingTop(15).Text(M("Low Stock (< 10)", "مخزون منخفض (< 10)")).FontSize(14).Bold().FontColor("#FF9800");
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Product
                                    columns.RelativeColumn(1); // Category
                                    columns.RelativeColumn(1); // Stock
                                    columns.RelativeColumn(1); // Price
                                    columns.RelativeColumn(1); // Value
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text(M("Product", "المنتج")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Category", "الفئة")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Stock", "المخزون")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Price", "السعر")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Value", "القيمة")).Bold();
                                });

                                foreach (var product in lowStockProducts)
                                {
                                    table.Cell().Element(CellStyle).Text($"{product.Emoji} {product.Name}");
                                    table.Cell().Element(CellStyle).Text(product.Category ?? "All");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text($"{product.Stock:F2}");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.Price));
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.Stock * product.Price));
                                }
                            });
                        }

                        // All Products
                        if (products != null && products.Any())
                        {
                            col.Item().PaddingTop(15).Text(M("All Products", "جميع المنتجات")).FontSize(14).Bold();
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Product
                                    columns.RelativeColumn(1); // Category
                                    columns.RelativeColumn(1); // Stock
                                    columns.RelativeColumn(1); // Price
                                    columns.RelativeColumn(1); // Value
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text(M("Product", "المنتج")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Category", "الفئة")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Stock", "المخزون")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Price", "السعر")).Bold();
                                    header.Cell().Element(CellStyle).Text(M("Value", "القيمة")).Bold();
                                });

                                foreach (var product in products.OrderBy(p => p.Name))
                                {
                                    if (product == null) continue;

                                    table.Cell().Element(CellStyle).Text($"{product.Emoji} {product.Name}");
                                    table.Cell().Element(CellStyle).Text(product.Category ?? "All");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text($"{product.Stock:F2}");
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.Price));
                                    AlignNumeric(table.Cell().Element(CellStyle)).Text(FormatAmount(product.Stock * product.Price));
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        if (IsArabic)
                        {
                            x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).Bold();
                            x.Span(M(" Generated on ", " تم الإنشاء في "));
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
}
