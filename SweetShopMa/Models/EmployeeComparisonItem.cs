using SweetShopMa.Utils;

namespace SweetShopMa.Models;

/// <summary>
/// Comparison data for employee attendance and payroll.
/// </summary>
public class EmployeeComparisonItem
{
    /// <summary>
    /// Standard hours per workday.
    /// </summary>
    public const int HoursPerWorkDay = 8;

    /// <summary>
    /// Employee name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Number of days worked.
    /// </summary>
    public int DaysWorked { get; set; }

    /// <summary>
    /// Total hours worked.
    /// </summary>
    public decimal HoursWorked { get; set; }

    /// <summary>
    /// Display string for hours worked.
    /// </summary>
    public string HoursDisplay => $"{HoursWorked:F1}h";

    /// <summary>
    /// Total payroll amount.
    /// </summary>
    public decimal Payroll { get; set; }

    /// <summary>
    /// Currency code for this item (set when created).
    /// </summary>
    public string Currency { get; set; } = "EGP";

    /// <summary>
    /// Whether to display in Arabic format.
    /// </summary>
    public bool IsArabic { get; set; } = false;

    /// <summary>
    /// Display string for payroll amount using stored currency and language settings.
    /// </summary>
    public string PayrollDisplay => CurrencyHelper.FormatCurrency(Payroll, Currency, IsArabic);

    /// <summary>
    /// Display string for payroll amount with custom parameters.
    /// </summary>
    public string GetPayrollDisplay(bool isArabic = false, string currency = null)
    {
        return CurrencyHelper.FormatCurrency(Payroll, currency ?? Currency, isArabic);
    }
}
