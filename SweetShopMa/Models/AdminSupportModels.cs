using System;

namespace SweetShopMa.Models;

public class ProductReportItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Emoji { get; set; }
    public string Category { get; set; }
    public decimal Quantity { get; set; }
    public decimal TotalSales { get; set; }
    public bool IsSoldByWeight { get; set; }
    public string UnitLabel => IsSoldByWeight ? "kg" : "pcs";
    public string QuantityDisplay => IsSoldByWeight ? $"{Quantity:F2}" : $"{Quantity:F0}";
}

public class MonthlyAttendanceSummary
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int WorkedDays { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal Payroll { get; set; }
    public decimal ExpensesTotal { get; set; }
    public decimal NetPay => Payroll - ExpensesTotal;
    
    public decimal AbsenceDeductions { get; set; }
    public decimal RestDayPayout { get; set; }
    public int EarnedRestDays { get; set; }
}

public class MonthlyAttendanceTotals
{
    public decimal TotalPayroll { get; set; }
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalRestPayout { get; set; }
    public decimal TotalAbsenceDeductions { get; set; }
}

public class DailyAttendanceEntry
{
    public DateTime Date { get; set; }
    public string Status { get; set; }
    public string StatusKey { get; set; }
    public bool IsPresent { get; set; }
    public bool IsToday => Date.Date == DateTime.Today;
    public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Friday;
}

public class AttendanceCalculationResult
{
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; }
    public bool IsPresent { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal DailyPay { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string AbsencePermissionType { get; set; }
    public bool NeedsSalaryInput { get; set; }

    public static AttendanceCalculationResult Invalid(string message) => new() { IsValid = false, ValidationMessage = message };
}
