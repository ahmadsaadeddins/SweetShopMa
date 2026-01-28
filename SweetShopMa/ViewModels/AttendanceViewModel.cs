using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;
using SweetShopMa.Utils;

namespace SweetShopMa.ViewModels;

/// <summary>
/// ViewModel for managing employee attendance, payroll, and expenses.
/// Extracted from AdminViewModel to follow SRP.
/// </summary>
public partial class AttendanceViewModel : BaseViewModel
{
    private readonly AttendanceRulesService _attendanceRulesService;
    private readonly Services.IPdfService _pdfService;

    private const int HoursPerWorkDay = 8;
    private const int VirtualDaysForShortMonth = 2;
    private const int ShortMonthDays = 28;
    private const decimal DefaultOvertimeMultiplier = 1.5m;

    [ObservableProperty] private ObservableCollection<AttendanceRecord> _attendanceRecords = new();
    [ObservableProperty] private ObservableCollection<DailyAttendanceEntry> _attendanceCalendarDays = new();
    [ObservableProperty] private ObservableCollection<MonthlyAttendanceSummary> _monthlyAttendanceSummaries = new();
    [ObservableProperty] private ObservableCollection<EmployeeExpense> _employeeExpenses = new();

    [ObservableProperty] private User _selectedAttendanceUser;
    [ObservableProperty] private DateTime _attendanceDate = DateTime.Today;
    [ObservableProperty] private string _selectedAttendanceStatus = "Present";
    [ObservableProperty] private string _attendanceNotes = "";
    [ObservableProperty] private TimeSpan _attendanceCheckInTime = new(8, 0, 0);
    [ObservableProperty] private TimeSpan _attendanceCheckOutTime = new(16, 0, 0);
    [ObservableProperty] private string _attendancePreview = "";

    [ObservableProperty] private AttendanceRecord _editingAttendanceRecord;
    [ObservableProperty] private bool _isEditingAttendance;

    [ObservableProperty] private DateTime _summaryMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    [ObservableProperty] private MonthlyAttendanceSummary _selectedMonthlySummary;
    [ObservableProperty] private MonthlyAttendanceTotals _monthlySummaryTotals = new();

    // Filtering
    [ObservableProperty] private DateTime _filterStartDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _filterEndDate = DateTime.Today;
    [ObservableProperty] private string _searchText = "";

    public bool CanUseAttendanceTracker => _authService.CanUseAttendanceTracker;

    public AttendanceViewModel(
        DatabaseService databaseService, 
        AuthService authService, 
        Services.LocalizationService localizationService,
        Services.LoggingService loggingService,
        AttendanceRulesService attendanceRulesService,
        Services.IPdfService pdfService) 
        : base(databaseService, authService, localizationService, loggingService)
    {
        _attendanceRulesService = attendanceRulesService;
        _pdfService = pdfService;
        
        SelectedAttendanceStatus = _localizationService.GetString("Present");
    }

    [RelayCommand]
    public async Task LoadAttendanceAsync()
    {
        IsBusy = true;
        try
        {
            var records = await _databaseService.GetAttendanceRecordsAsync(FilterStartDate, FilterEndDate);
            AttendanceRecords.Clear();
            if (records != null)
            {
                foreach (var record in records.OrderByDescending(r => r.Date))
                {
                    AttendanceRecords.Add(record);
                }
            }
            await LoadMonthlySummaryAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadAttendanceAsync", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LoadMonthlySummaryAsync()
    {
        try
        {
            var monthStart = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var allRecords = await _databaseService.GetAttendanceRecordsAsync(monthStart, monthEnd) ?? new List<AttendanceRecord>();
            var users = await _databaseService.GetUsersAsync();
            
            MonthlyAttendanceSummaries.Clear();
            var summaries = new List<MonthlyAttendanceSummary>();

            foreach (var user in users.Where(u => !u.IsDeveloper))
            {
                var userRecords = allRecords.Where(r => r.UserId == user.Id).ToList();
                var stats = _attendanceRulesService.ComputeMonthly(user, SummaryMonth, userRecords);
                
                // Simplified payroll logic for extraction
                decimal expensesTotal = (await _databaseService.GetEmployeeExpensesAsync(user.Id, monthStart, monthEnd))?.Sum(e => e.Amount) ?? 0m;
                
                var summary = new MonthlyAttendanceSummary
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    DaysPresent = userRecords.Count(r => r.IsPresent),
                    DaysAbsent = userRecords.Count(r => !r.IsPresent),
                    WorkedDays = stats.workedDays,
                    Payroll = Math.Max(0, user.MonthlySalary + stats.restPayout - stats.absenceDeductions - expensesTotal),
                    ExpensesTotal = expensesTotal
                };
                summaries.Add(summary);
                MonthlyAttendanceSummaries.Add(summary);
            }

            MonthlySummaryTotals = new MonthlyAttendanceTotals
            {
                TotalPayroll = summaries.Sum(s => s.Payroll),
                TotalPresentDays = summaries.Sum(s => s.DaysPresent)
            };
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadMonthlySummaryAsync", ex);
        }
    }

    [RelayCommand]
    public async Task AddAttendanceAsync()
    {
        if (SelectedAttendanceUser == null)
        {
            ShowStatus(_localizationService.GetString("PleaseSelectUser"), true);
            return;
        }

        IsBusy = true;
        try
        {
            var record = new AttendanceRecord
            {
                UserId = SelectedAttendanceUser.Id,
                UserName = SelectedAttendanceUser.Name,
                Date = AttendanceDate,
                Status = SelectedAttendanceStatus,
                IsPresent = SelectedAttendanceStatus == _localizationService.GetString("Present"),
                CheckInTime = AttendanceDate.Date.Add(AttendanceCheckInTime),
                CheckOutTime = AttendanceDate.Date.Add(AttendanceCheckOutTime),
                Notes = AttendanceNotes
            };

            await _databaseService.SaveAttendanceRecordAsync(record);
            ShowStatus(_localizationService.GetString("AttendanceSaved"), false);
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("AddAttendanceAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorSavingAttendance"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [ObservableProperty] private DateTime _expenseDate = DateTime.Today;
    [ObservableProperty] private string _expenseAmount = "";
    [ObservableProperty] private string _expenseCategory = "";
    [ObservableProperty] private string _expenseNotes = "";

    public string SelectedAttendanceStatusKey 
    {
        get 
        {
            // Simple mapping for brevity in extraction, assuming localized strings match keys or vice versa
            // In a real app, this would use a more robust mapping
            if (SelectedAttendanceStatus == _localizationService.GetString("Present")) return "Present";
            if (SelectedAttendanceStatus == _localizationService.GetString("Reset")) return "Reset";
            if (SelectedAttendanceStatus == _localizationService.GetString("AbsentWithPermission")) return "AbsentWithPermission";
            if (SelectedAttendanceStatus == _localizationService.GetString("AbsentWithoutPermission")) return "AbsentWithoutPermission";
            return "Present";
        }
    }

    [RelayCommand]
    public async Task AddExpenseAsync()
    {
        if (SelectedAttendanceUser == null)
        {
            ShowStatus(_localizationService.GetString("PleaseSelectUser"), true);
            return;
        }

        if (!decimal.TryParse(ExpenseAmount, out var amount) || amount <= 0)
        {
            ShowStatus(_localizationService.GetString("EnterValidAmount"), true);
            return;
        }

        IsBusy = true;
        try
        {
            var expense = new EmployeeExpense
            {
                UserId = SelectedAttendanceUser.Id,
                UserName = SelectedAttendanceUser.Name,
                ExpenseDate = ExpenseDate,
                Amount = amount,
                Category = string.IsNullOrWhiteSpace(ExpenseCategory) ? "General" : ExpenseCategory.Trim(),
                Notes = ExpenseNotes?.Trim() ?? ""
            };

            await _databaseService.CreateEmployeeExpenseAsync(expense);
            ShowStatus(_localizationService.GetString("ExpenseAdded"), false);
            
            ExpenseAmount = "";
            ExpenseCategory = "";
            ExpenseNotes = "";
            
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("AddExpenseAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorSavingExpense"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DeleteExpenseAsync(EmployeeExpense expense)
    {
        if (expense == null) return;
        
        IsBusy = true;
        try
        {
            await _databaseService.DeleteEmployeeExpenseAsync(expense);
            ShowStatus(_localizationService.GetString("ExpenseDeleted"), false);
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("DeleteExpenseAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorDeletingExpense"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task OpenAttendancePageAsync()
    {
        await Shell.Current.GoToAsync("AttendancePage");
    }
}
