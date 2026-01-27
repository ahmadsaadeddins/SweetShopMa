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

    /// <summary>
    /// Generates a sales report PDF with metrics, top products, and recent orders.
    /// </summary>
    /// <param name="totalSales">Total revenue from all orders</param>
    /// <param name="totalOrders">Number of completed orders</param>
    /// <param name="averageOrderValue">Average order value</param>
    /// <param name="totalItemsSold">Total quantity of items sold</param>
    /// <param name="last7DaysSales">Sales in the last 7 days</param>
    /// <param name="topProducts">List of top products by sales</param>
    /// <param name="recentOrders">List of recent orders</param>
    /// <returns>Path to the generated PDF file, or null if generation failed</returns>
    Task<string?> GenerateSalesReportPdfAsync(
        decimal totalSales,
        int totalOrders,
        decimal averageOrderValue,
        decimal totalItemsSold,
        decimal last7DaysSales,
        List<ViewModels.ProductReportItem> topProducts,
        List<Models.Order> recentOrders);

    /// <summary>
    /// Generates an inventory report PDF with product stock information.
    /// </summary>
    /// <param name="products">List of all products with stock information</param>
    /// <param name="lowStockThreshold">Threshold for low stock warning (default 10)</param>
    /// <returns>Path to the generated PDF file, or null if generation failed</returns>
    Task<string?> GenerateInventoryReportPdfAsync(
        List<Models.Product> products,
        decimal lowStockThreshold = 10);

    /// <summary>
    /// Generates an inventory report PDF for a specific branch or all branches.
    /// </summary>
    /// <param name="productsWithStock">List of products with their stock levels</param>
    /// <param name="locationName">Name of the location (or "All Branches")</param>
    /// <param name="lowStockThreshold">Threshold for low stock warning (default 10)</param>
    /// <returns>Path to the generated PDF file, or null if generation failed</returns>
    Task<string?> GenerateInventoryReportByLocationPdfAsync(
        List<(Models.Product Product, decimal Stock)> productsWithStock,
        string locationName,
        decimal lowStockThreshold = 10);
}

