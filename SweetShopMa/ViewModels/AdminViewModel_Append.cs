// Enhanced statistics classes
public class MonthlyComparisonData
{
    public int CurrentMonthDays { get; set; }
    public int PreviousMonthDays { get; set; }
    public decimal CurrentMonthHours { get; set; }
    public decimal PreviousMonthHours { get; set; }
    public decimal CurrentMonthPayroll { get; set; }
    public decimal PreviousMonthPayroll { get; set; }

    public string DaysComparison => $"{CurrentMonthDays} vs {PreviousMonthDays}";
    public string HoursComparison => $"{CurrentMonthHours:F1}h vs {PreviousMonthHours:F1}h";
    public string PayrollComparison => $"${CurrentMonthPayroll:F2} vs ${PreviousMonthPayroll:F2}";
    public decimal DaysChange => CurrentMonthDays - PreviousMonthDays;
    public decimal HoursChange => CurrentMonthHours - PreviousMonthHours;
    public decimal PayrollChange => CurrentMonthPayroll - PreviousMonthPayroll;
}

public class EmployeeComparisonItem
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int DaysWorked { get; set; }
    public decimal TotalHours { get; set; }
    public decimal AverageHoursPerDay { get; set; }
    public decimal AttendanceRate { get; set; }
    public decimal TotalPayroll { get; set; }

    public string HoursDisplay => $"{TotalHours:F1}h";
    public string AvgHoursDisplay => $"{AverageHoursPerDay:F1}h/day";
    public string AttendanceRateDisplay => $"{AttendanceRate:F1}%";
    public string PayrollDisplay => $"${TotalPayroll:F2}";
}

