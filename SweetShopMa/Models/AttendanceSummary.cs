namespace SweetShopMa.Models;

/// <summary>
/// Summary statistics for attendance records within a date range.
/// </summary>
public class AttendanceSummary
{
    /// <summary>
    /// Total number of present days.
    /// </summary>
    public int PresentCount { get; set; }

    /// <summary>
    /// Total number of absent days.
    /// </summary>
    public int AbsentCount { get; set; }

    /// <summary>
    /// Total number of overtime entries.
    /// </summary>
    public int OvertimeCount { get; set; }

    /// <summary>
    /// Total regular hours worked.
    /// </summary>
    public decimal TotalRegularHours { get; set; }

    /// <summary>
    /// Total overtime hours worked.
    /// </summary>
    public decimal TotalOvertimeHours { get; set; }

    /// <summary>
    /// Total payroll amount for the period.
    /// </summary>
    public decimal TotalPayroll { get; set; }
}
