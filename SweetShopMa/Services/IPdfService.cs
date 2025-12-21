using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SweetShopMa.Models;

namespace SweetShopMa.Services;

/// <summary>
/// Interface for PDF generation service.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a payroll PDF report for the specified month.
    /// </summary>
    /// <param name="summaries">List of monthly attendance summaries</param>
    /// <param name="month">The month and year for the report</param>
    /// <param name="totals">Monthly totals</param>
    /// <returns>Path to the generated PDF file, or null if generation failed</returns>
    Task<string?> GeneratePayrollPdfAsync(List<ViewModels.MonthlyAttendanceSummary> summaries, DateTime month, ViewModels.MonthlyAttendanceTotals totals);

    Task<string?> GenerateEmployeePayrollPdfAsync(ViewModels.MonthlyAttendanceSummary summary, DateTime month, List<EmployeeExpense> expenses);

    Task<string?> GenerateAttendancePdfAsync(List<AttendanceRecord> records, DateTime month);

    Task<string?> GenerateSalesReportPdfAsync(List<Order> orders, List<ViewModels.ProductReportItem> topProducts, DateTime startDate, DateTime endDate);

    Task<string?> GenerateInventoryReportPdfAsync(List<Product> products, DateTime reportDate);
}

