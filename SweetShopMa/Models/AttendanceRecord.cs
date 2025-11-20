using SQLite;

namespace SweetShopMa.Models;

/// <summary>
/// Represents an employee attendance record for a specific date.
/// 
/// WHAT IS AN ATTENDANCE RECORD?
/// An AttendanceRecord tracks whether an employee was present or absent on a given date,
/// along with their working hours, pay, and check-in/check-out times.
/// 
/// USE CASE:
/// This is used for payroll and attendance tracking. Managers can see who was present,
/// calculate hours worked, and determine daily pay based on regular and overtime hours.
/// </summary>
public class AttendanceRecord
{
    // ============================================
    // DATABASE PROPERTIES
    // ============================================
    
    /// <summary>
    /// Unique identifier for the attendance record.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    
    /// <summary>
    /// ID of the employee (User) this record belongs to.
    /// </summary>
    public int UserId { get; set; }
    
    /// <summary>
    /// Employee name (for display purposes, denormalized from User table).
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Date of the attendance record (defaults to today).
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Attendance status: "Present" or "Absent" (as string for display).
    /// </summary>
    public string Status { get; set; } = "Present";
    
    /// <summary>
    /// Boolean attendance status (true = Present, false = Absent).
    /// This is the programmatic way to check attendance.
    /// </summary>
    public bool IsPresent { get; set; } = true;
    
    /// <summary>
    /// Regular working hours for this day (e.g., 8.0 hours).
    /// </summary>
    public decimal RegularHours { get; set; } = 0;
    
    /// <summary>
    /// Overtime hours for this day (e.g., 2.0 hours).
    /// </summary>
    public decimal OvertimeHours { get; set; } = 0;
    
    /// <summary>
    /// Calculated daily pay for this day (based on hours and salary).
    /// </summary>
    public decimal DailyPay { get; set; } = 0;
    
    /// <summary>
    /// Check-in time (when employee started work).
    /// DateTime? means it's nullable - can be null if not recorded.
    /// </summary>
    public DateTime? CheckInTime { get; set; }
    
    /// <summary>
    /// Check-out time (when employee finished work).
    /// DateTime? means it's nullable - can be null if not recorded.
    /// </summary>
    public DateTime? CheckOutTime { get; set; }
    
    /// <summary>
    /// Optional notes about the attendance (e.g., "Late arrival", "Left early").
    /// </summary>
    public string Notes { get; set; } = "";
    
    /// <summary>
    /// Absence permission type: "None" (for present days), "WithPermission", "WithoutPermission", or "Reset".
    /// Used to determine payroll deductions (1 day for with permission, 2 days for without).
    /// "Reset" means the day is a reset day and doesn't count in the work cycle.
    /// </summary>
    public string AbsencePermissionType { get; set; } = "None";
    
    /// <summary>
    /// Timestamp when this record was created (for audit purposes).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ============================================
    // COMPUTED PROPERTIES (Not stored in database)
    // ============================================
    
    /// <summary>
    /// Total hours worked: RegularHours + OvertimeHours.
    /// </summary>
    [Ignore]
    public decimal TotalHours => RegularHours + OvertimeHours;

    /// <summary>
    /// Formatted check-in time for display (e.g., "08:00" or "--" if not set).
    /// Uses null-conditional operator (?.) and null-coalescing operator (??).
    /// </summary>
    [Ignore]
    public string CheckInDisplay => CheckInTime?.ToString("HH:mm") ?? "--";

    /// <summary>
    /// Formatted check-out time for display (e.g., "17:00" or "--" if not set).
    /// </summary>
    [Ignore]
    public string CheckOutDisplay => CheckOutTime?.ToString("HH:mm") ?? "--";
}

