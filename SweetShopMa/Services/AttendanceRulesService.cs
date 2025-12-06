using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SweetShopMa.Models;

namespace SweetShopMa.Services;

public class AttendanceRulesService
{
    private readonly DatabaseService _databaseService;

    public AttendanceRulesService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<ViewModels.AttendanceCalculationResult> CalculateAsync(User user, DateTime date, string status, DateTime? checkIn, DateTime? checkOut)
    {
        var result = new ViewModels.AttendanceCalculationResult
        {
            IsValid = true,
            ValidationMessage = "",
            CheckIn = checkIn,
            CheckOut = checkOut
        };

        bool isAbsentWithPermission = status.Contains("Absent (With Permission)", StringComparison.OrdinalIgnoreCase);
        bool isAbsentWithoutPermission = status.Contains("Absent (Without Permission)", StringComparison.OrdinalIgnoreCase);
        bool isAbsent = isAbsentWithPermission || isAbsentWithoutPermission;
        bool isReset = string.Equals(status, "Reset", StringComparison.OrdinalIgnoreCase);
        bool requiresTimes = string.Equals(status, "Present", StringComparison.OrdinalIgnoreCase) || isReset;

        result.IsPresent = !isAbsent && !isReset;
        if (isAbsentWithPermission) result.AbsencePermissionType = "WithPermission";
        else if (isAbsentWithoutPermission) result.AbsencePermissionType = "WithoutPermission";
        else if (isReset) result.AbsencePermissionType = "Reset";
        else result.AbsencePermissionType = "None";

        if (requiresTimes)
        {
            if (!checkIn.HasValue || !checkOut.HasValue)
                return ViewModels.AttendanceCalculationResult.Invalid("Specify check-in and out times.");

            var actualIn = checkIn.Value;
            var actualOut = checkOut.Value;
            if (actualOut <= actualIn)
                return ViewModels.AttendanceCalculationResult.Invalid("Checkout must be after check-in.");

            var scheduleStart = date.Date.AddHours(8);
            var scheduleEnd = scheduleStart.AddHours(8);

            var overlapStart = actualIn > scheduleStart ? actualIn : scheduleStart;
            var overlapEnd = actualOut < scheduleEnd ? actualOut : scheduleEnd;
            decimal regularHours = overlapEnd > overlapStart ? (decimal)(overlapEnd - overlapStart).TotalHours : 0m;
            regularHours = Math.Max(0m, Math.Min(regularHours, 8m));
            decimal overtimeHours = actualOut > scheduleEnd ? (decimal)(actualOut - scheduleEnd).TotalHours : 0m;

        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            result.RegularHours = Math.Round(regularHours, 2);
            result.OvertimeHours = Math.Round(overtimeHours, 2);
        }
        else
        {
            result.RegularHours = 0m;
            result.OvertimeHours = 0m;
        }

        if (!result.IsPresent && !isReset)
        {
            result.RegularHours = 0m;
            result.OvertimeHours = 0m;
        }

        var salary = user?.MonthlySalary ?? 0m;
        var hourlyRate = (salary / 30m) / 8m;
        var otMultiplier = user?.OvertimeMultiplier ?? 1.5m;
        var overtimeRate = hourlyRate * otMultiplier;
        var pay = (result.RegularHours * hourlyRate) + (result.OvertimeHours * overtimeRate);

        result.DailyPay = Math.Round(pay, 2);
        result.NeedsSalaryInput = result.IsPresent && salary <= 0;
        if (result.NeedsSalaryInput)
        {
            result.DailyPay = 0m;
            result.ValidationMessage = "Set monthly salary to calculate pay.";
        }

        return result;
    }

    public (int workedDays, int restDays, int withPermissionAbsences, int withoutPermissionAbsences, decimal dailyRate, decimal restPayout, decimal absenceDeductions) ComputeMonthly(User user, DateTime month, List<AttendanceRecord> records)
    {
        try
        {
            if (records == null)
                records = new List<AttendanceRecord>();
            
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthRecords = records
                .Where(r => r != null && r.Date >= monthStart && r.Date <= monthEnd)
                .ToList();
            
            var workedDays = monthRecords.Count(r => r != null && r.IsPresent);
            var restDays = workedDays / 6;
            var withPerm = monthRecords.Count(r => r != null && r.AbsencePermissionType == "WithPermission");
            var withoutPerm = monthRecords.Count(r => r != null && r.AbsencePermissionType == "WithoutPermission");
            var dailyRate = (user?.MonthlySalary ?? 0m) / 30m;
            var restPayout = restDays * dailyRate;
            var absenceDeductions = (withPerm * dailyRate) + (withoutPerm * 2m * dailyRate);
            return (workedDays, restDays, withPerm, withoutPerm, dailyRate, restPayout, absenceDeductions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ComputeMonthly: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            // Return safe defaults
            var dailyRate = (user?.MonthlySalary ?? 0m) / 30m;
            return (0, 0, 0, 0, dailyRate, 0m, 0m);
        }
    }
}
