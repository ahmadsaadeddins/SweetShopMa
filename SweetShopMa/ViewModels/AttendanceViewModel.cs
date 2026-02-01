using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;
using SweetShopMa.Utils;
using Microsoft.Maui.ApplicationModel;

namespace SweetShopMa.ViewModels;

/// <summary>
/// ViewModel for managing employee attendance, payroll, and expenses.
/// Extracted from AdminViewModel to follow SRP.
/// </summary>
public partial class AttendanceViewModel : BaseViewModel
{
    private readonly AttendanceRulesService _attendanceRulesService;
    private readonly Services.IPdfService _pdfService;
    private readonly ExportService _exportService;
    private readonly IShopSettingsService _settingsService;

    private const int HoursPerWorkDay = 8;
    private const int VirtualDaysForShortMonth = 2;
    private const int ShortMonthDays = 28;
    private const decimal DefaultOvertimeMultiplier = 1.5m;

    [ObservableProperty] private ObservableCollection<AttendanceRecord> _attendanceRecords = new();
    [ObservableProperty] private ObservableCollection<DailyAttendanceEntry> _attendanceCalendarDays = new();
    [ObservableProperty] private ObservableCollection<MonthlyAttendanceSummary> _monthlyAttendanceSummaries = new();
    [ObservableProperty] private ObservableCollection<EmployeeExpense> _employeeExpenses = new();
    [ObservableProperty] private ObservableCollection<User> _users = new();

    [ObservableProperty] private User _selectedAttendanceUser;

    partial void OnSelectedAttendanceUserChanged(User value)
    {
        _ = LoadAttendanceCalendarAsync();
    }

    [ObservableProperty] private DateTime _attendanceDate = DateTime.Today;
    [ObservableProperty] private string _selectedAttendanceStatus = "Present";
    [ObservableProperty] private string _attendanceNotes = "";
    [ObservableProperty] private TimeSpan _attendanceCheckInTime = new(8, 0, 0);
    [ObservableProperty] private TimeSpan _attendanceCheckOutTime = new(16, 0, 0);
    [ObservableProperty] private string _attendancePreview = "";

    [ObservableProperty] private AttendanceRecord _editingAttendanceRecord;
    [ObservableProperty] private bool _isEditingAttendance;

    [ObservableProperty] private DateTime _summaryMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    partial void OnSummaryMonthChanged(DateTime value)
    {
        _ = LoadAttendanceCalendarAsync();
    }

    [ObservableProperty] private MonthlyAttendanceSummary _selectedMonthlySummary;
    [ObservableProperty] private MonthlyAttendanceTotals _monthlySummaryTotals = new();

    // Filtering
    [ObservableProperty] private DateTime _filterStartDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _filterEndDate = DateTime.Today;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _filterStatus = "All";
    [ObservableProperty] private string _filterOvertime = "All";

    // Selected Records for Bulk Operations
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedRecords))]
    private ObservableCollection<AttendanceRecord> _selectedRecords = new();
    public bool HasSelectedRecords => SelectedRecords.Count > 0;

    // Collapsible Section Visibility
    [ObservableProperty] private bool _isAddEditSectionExpanded = true;
    [ObservableProperty] private bool _isStatisticsSectionExpanded = true;
    [ObservableProperty] private bool _isRecordsSectionExpanded = true;
    [ObservableProperty] private bool _isMonthlySummarySectionExpanded = true;
    [ObservableProperty] private bool _isCalendarSectionExpanded = true;
    [ObservableProperty] private bool _isEmployeeComparisonSectionExpanded = true;
    [ObservableProperty] private bool _isFilteringSectionExpanded = true;

    // Additional properties for UI binding
    [ObservableProperty] private AttendanceSummary _attendanceSummary = new();
    [ObservableProperty] private ObservableCollection<EmployeeComparisonItem> _employeeComparisonData = new();
    private ObservableCollection<string> _attendanceStatuses = new();
    public ObservableCollection<string> AttendanceStatuses
    {
        get => _attendanceStatuses;
        set => SetProperty(ref _attendanceStatuses, value);
    }
    private bool _isAttendanceTimeEntryEnabled = true;
    public bool IsAttendanceTimeEntryEnabled
    {
        get => _isAttendanceTimeEntryEnabled;
        set => SetProperty(ref _isAttendanceTimeEntryEnabled, value);
    }

    public bool CanUseAttendanceTracker => _authService.CanUseAttendanceTracker;

    public AttendanceViewModel(
        DatabaseService databaseService,
        AuthService authService,
        Services.LocalizationService localizationService,
        Services.LoggingService loggingService,
        AttendanceRulesService attendanceRulesService,
        Services.IPdfService pdfService,
        ExportService exportService,
        IShopSettingsService settingsService)
        : base(databaseService, authService, localizationService, loggingService)
    {
        _attendanceRulesService = attendanceRulesService;
        _pdfService = pdfService;
        _exportService = exportService;
        _settingsService = settingsService;
        
        SelectedAttendanceStatus = _localizationService.GetString("Present");
        
        // Initialize AttendanceStatuses
        AttendanceStatuses = new ObservableCollection<string>
        {
            _localizationService.GetString("Present"),
            _localizationService.GetString("Reset"),
            _localizationService.GetString("AbsentWithPermission"),
            _localizationService.GetString("AbsentWithoutPermission")
        };

        // Initialize status key map with localized strings
        _statusKeyMap[_localizationService.GetString("Present")] = "Present";
        _statusKeyMap[_localizationService.GetString("Reset")] = "Reset";
        _statusKeyMap[_localizationService.GetString("AbsentWithPermission")] = "AbsentWithPermission";
        _statusKeyMap[_localizationService.GetString("AbsentWithoutPermission")] = "AbsentWithoutPermission";

        // Load users asynchronously
        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var users = await _databaseService.GetUsersAsync();
            Users.Clear();
            if (users != null)
            {
                foreach (var user in users.Where(u => !u.IsDeveloper))
                {
                    Users.Add(user);
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadUsersAsync", ex);
        }
    }

    public void RefreshLocalizedProperties()
    {
        // Update localized strings
        SelectedAttendanceStatus = _localizationService.GetString("Present");
        
        // Refresh AttendanceStatuses collection
        AttendanceStatuses.Clear();
        AttendanceStatuses.Add(_localizationService.GetString("Present"));
        AttendanceStatuses.Add(_localizationService.GetString("Reset"));
        AttendanceStatuses.Add(_localizationService.GetString("AbsentWithPermission"));
        AttendanceStatuses.Add(_localizationService.GetString("AbsentWithoutPermission"));

        // Update status key map with localized strings
        _statusKeyMap.Clear();
        _statusKeyMap[_localizationService.GetString("Present")] = "Present";
        _statusKeyMap[_localizationService.GetString("Reset")] = "Reset";
        _statusKeyMap[_localizationService.GetString("AbsentWithPermission")] = "AbsentWithPermission";
        _statusKeyMap[_localizationService.GetString("AbsentWithoutPermission")] = "AbsentWithoutPermission";
    }

    [RelayCommand]
    public async Task LoadAttendanceAsync()
    {
        IsBusy = true;
        try
        {
            // Ensure users are loaded first
            await LoadUsersAsync();
            
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
            await LoadAttendanceCalendarAsync();
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
    public async Task LoadAttendanceCalendarAsync()
    {
        try
        {
            AttendanceCalendarDays.Clear();
            
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            
            // Get all attendance records for the current month
            var records = await _databaseService.GetAttendanceRecordsAsync(firstDayOfMonth, lastDayOfMonth);
            
            // Get records for selected user only, or all records if no user selected
            var userRecords = SelectedAttendanceUser != null 
                ? records?.Where(r => r.UserId == SelectedAttendanceUser.Id).ToList() 
                : records?.ToList();
            
            // Add placeholder days for alignment (days before first of month)
            var firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            for (int i = 0; i < firstDayOfWeek; i++)
            {
                AttendanceCalendarDays.Add(new DailyAttendanceEntry
                {
                    Date = firstDayOfMonth.AddDays(-(firstDayOfWeek - i)),
                    DayText = "",
                    IsPlaceholder = true
                });
            }
            
            // Add days of the month
            for (var date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
            {
                var record = userRecords?.FirstOrDefault(r => r.Date.Date == date.Date);
                var entry = new DailyAttendanceEntry
                {
                    Date = date,
                    DayText = date.Day.ToString(),
                    Status = record?.Status ?? "",
                    IsPresent = record?.IsPresent ?? false,
                    IsAbsent = record != null && !record.IsPresent,
                    HasOvertime = (record?.OvertimeHours ?? 0) > 0,
                    DetailText = record != null ? (record.IsPresent ? "P" : "A") : ""
                };
                AttendanceCalendarDays.Add(entry);
            }
            
            OnPropertyChanged(nameof(AttendanceCalendarDays));
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadAttendanceCalendarAsync", ex);
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

            // Load employee comparison data with dynamic currency
            await LoadEmployeeComparisonDataAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadMonthlySummaryAsync", ex);
        }
    }

    [RelayCommand]
    public async Task LoadEmployeeComparisonDataAsync()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            var currency = settings?.Currency ?? "EGP";
            var isArabic = _localizationService.IsArabic;

            EmployeeComparisonData.Clear();

            foreach (var summary in MonthlyAttendanceSummaries)
            {
                var comparisonItem = new EmployeeComparisonItem
                {
                    UserName = summary.UserName,
                    DaysWorked = summary.DaysPresent,
                    HoursWorked = summary.WorkedDays * EmployeeComparisonItem.HoursPerWorkDay,
                    Payroll = summary.Payroll,
                    Currency = currency,
                    IsArabic = isArabic
                };
                EmployeeComparisonData.Add(comparisonItem);
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadEmployeeComparisonDataAsync", ex);
        }
    }

    [RelayCommand]
    public async Task AddAttendanceAsync()
    {
        if (SelectedAttendanceUser == null)
        {
            ShowStatus(_localizationService.GetString("PleaseSelectEmployee"), true);
            return;
        }

        IsBusy = true;
        try
        {
            // DIAGNOSTIC: Check if a record already exists for this user and date
            var existingRecord = await _databaseService.GetAttendanceRecordAsync(SelectedAttendanceUser.Id, AttendanceDate);
            if (existingRecord != null)
            {
                _loggingService?.LogDebug("AddAttendanceAsync", $"Duplicate attendance record found for UserId={SelectedAttendanceUser.Id}, Date={AttendanceDate:yyyy-MM-dd}, ExistingRecordId={existingRecord.Id}");
                ShowStatus(_localizationService.GetString("AttendanceRecordAlreadyExists"), true);
                return;
            }

            _loggingService?.LogDebug("AddAttendanceAsync", $"No existing record found for UserId={SelectedAttendanceUser.Id}, Date={AttendanceDate:yyyy-MM-dd}, proceeding with insert");

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
        catch (SQLite.SQLiteException sqliteEx)
        {
            // DIAGNOSTIC: Log SQLite-specific exceptions using the additionalInfo parameter
            _loggingService?.LogError("AddAttendanceAsync", sqliteEx, $"SQLiteException ResultCode: {sqliteEx.Result}");

            // Check if it's a UNIQUE constraint violation using ResultCode instead of fragile string parsing
            if (sqliteEx.Result == SQLite.SQLite3.Result.Constraint)
            {
                _loggingService?.LogDebug("AddAttendanceAsync", "Detected UNIQUE constraint violation - record already exists");
                ShowStatus(_localizationService.GetString("AttendanceRecordAlreadyExists"), true);
            }
            else
            {
                ShowStatus(_localizationService.GetString("ErrorSavingAttendance"), true);
            }
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
            // Use a dictionary-based mapping for robust status key lookup
            // This avoids fragile string comparisons and is more maintainable
            return _statusKeyMap.TryGetValue(SelectedAttendanceStatus, out var key) ? key : "Present";
        }
    }

    private static readonly Dictionary<string, string> _statusKeyMap = new()
    {
        { "Present", "Present" },
        { "Reset", "Reset" },
        { "AbsentWithPermission", "AbsentWithPermission" },
        { "AbsentWithoutPermission", "AbsentWithoutPermission" }
    };

    [RelayCommand]
    public async Task AddExpenseAsync()
    {
        if (SelectedAttendanceUser == null)
        {
            ShowStatus(_localizationService.GetString("PleaseSelectEmployee"), true);
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
        await Shell.Current.GoToAsync("attendance");
    }

    [RelayCommand]
    public async Task ExportPayrollAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingPayrollReport"), false);
            
            var month = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
            var filePath = await _pdfService.GeneratePayrollPdfAsync(
                MonthlyAttendanceSummaries.ToList(), 
                month, 
                MonthlySummaryTotals);

            if (!string.IsNullOrEmpty(filePath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = _localizationService.GetString("PayrollReport"),
                    File = new ShareFile(filePath)
                });
                ShowStatus(_localizationService.GetString("PayrollExported"), false);
            }
            else
            {
                ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ExportPayrollAsync", ex);
            ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ExportAttendanceAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingAttendanceReport"), false);
            
            var month = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
            var filePath = await _pdfService.GenerateAttendancePdfAsync(
                AttendanceRecords.ToList(), 
                month);

            if (!string.IsNullOrEmpty(filePath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = _localizationService.GetString("AttendanceReport"),
                    File = new ShareFile(filePath)
                });
                ShowStatus(_localizationService.GetString("AttendanceExported"), false);
            }
            else
            {
                ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ExportAttendanceAsync", ex);
            ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ApplyFiltersAsync()
    {
        IsBusy = true;
        try
        {
            var records = await _databaseService.GetAttendanceRecordsAsync(FilterStartDate, FilterEndDate) ?? new List<AttendanceRecord>();
            
            var filteredRecords = records.AsEnumerable();
            
            // Filter by status
            var allText = _localizationService.GetString("All");
            var presentText = _localizationService.GetString("Present");
            var absentText = _localizationService.GetString("Absent");
            
            if (FilterStatus != allText)
            {
                var isPresent = FilterStatus == presentText;
                filteredRecords = filteredRecords.Where(r => r.IsPresent == isPresent);
            }
            
            // Filter by overtime
            var withOTText = _localizationService.GetString("WithOT");
            var withoutOTText = _localizationService.GetString("WithoutOT");
            
            if (FilterOvertime == withOTText)
            {
                filteredRecords = filteredRecords.Where(r => r.OvertimeHours > 0);
            }
            else if (FilterOvertime == withoutOTText)
            {
                filteredRecords = filteredRecords.Where(r => r.OvertimeHours == 0);
            }
            
            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filteredRecords = filteredRecords.Where(r => 
                    (r.UserName?.ToLower().Contains(searchLower) ?? false) ||
                    (r.Notes?.ToLower().Contains(searchLower) ?? false));
            }
            
            AttendanceRecords.Clear();
            foreach (var record in filteredRecords.OrderByDescending(r => r.Date))
            {
                AttendanceRecords.Add(record);
            }
            
            ShowStatus(_localizationService.GetString("FiltersApplied"), false);
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ApplyFiltersAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorApplyingFilters"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public void ClearFilters()
    {
        FilterStartDate = DateTime.Today.AddDays(-30);
        FilterEndDate = DateTime.Today;
        FilterStatus = _localizationService.GetString("All");
        FilterOvertime = _localizationService.GetString("All");
        SearchText = "";
        ShowStatus(_localizationService.GetString("FiltersCleared"), false);
    }

    [RelayCommand]
    public void ClearSelection()
    {
        SelectedRecords.Clear();
        ShowStatus(_localizationService.GetString("SelectionCleared"), false);
    }

    [RelayCommand]
    public async Task EditAttendanceRecord(AttendanceRecord record)
    {
        if (record == null) return;

        EditingAttendanceRecord = record;
        IsEditingAttendance = true;

        // Find the user from the loaded Users collection
        SelectedAttendanceUser = Users.FirstOrDefault(u => u.Id == record.UserId);
        
        // If user not found in collection, try to load from database
        if (SelectedAttendanceUser == null)
        {
            await LoadUsersAsync();
            SelectedAttendanceUser = Users.FirstOrDefault(u => u.Id == record.UserId);
        }

        AttendanceDate = record.Date;
        SelectedAttendanceStatus = record.Status;
        AttendanceCheckInTime = record.CheckInTime?.TimeOfDay ?? new TimeSpan(8, 0, 0);
        AttendanceCheckOutTime = record.CheckOutTime?.TimeOfDay ?? new TimeSpan(16, 0, 0);
        AttendanceNotes = record.Notes ?? "";

        IsAddEditSectionExpanded = true;
    }

    [RelayCommand]
    public async Task UpdateAttendanceRecordAsync()
    {
        if (EditingAttendanceRecord == null || SelectedAttendanceUser == null)
        {
            ShowStatus(_localizationService.GetString("ErrorUpdatingAttendance"), true);
            return;
        }

        IsBusy = true;
        try
        {
            EditingAttendanceRecord.UserId = SelectedAttendanceUser.Id;
            EditingAttendanceRecord.UserName = SelectedAttendanceUser.Name;
            EditingAttendanceRecord.Date = AttendanceDate;
            EditingAttendanceRecord.Status = SelectedAttendanceStatus;
            EditingAttendanceRecord.IsPresent = SelectedAttendanceStatus == _localizationService.GetString("Present");
            EditingAttendanceRecord.CheckInTime = AttendanceDate.Date.Add(AttendanceCheckInTime);
            EditingAttendanceRecord.CheckOutTime = AttendanceDate.Date.Add(AttendanceCheckOutTime);
            EditingAttendanceRecord.Notes = AttendanceNotes;

            await _databaseService.SaveAttendanceRecordAsync(EditingAttendanceRecord);
            ShowStatus(_localizationService.GetString("AttendanceUpdated"), false);
            
            IsEditingAttendance = false;
            EditingAttendanceRecord = null;
            
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("UpdateAttendanceRecordAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorUpdatingAttendance"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task DeleteAttendanceRecordAsync(AttendanceRecord record)
    {
        if (record == null) return;
        
        IsBusy = true;
        try
        {
            await _databaseService.DeleteAttendanceRecordAsync(record);
            ShowStatus(_localizationService.GetString("AttendanceDeleted"), false);
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("DeleteAttendanceRecordAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorDeletingAttendance"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task BulkDeleteAsync()
    {
        if (SelectedRecords.Count == 0) return;
        
        IsBusy = true;
        try
        {
            foreach (var record in SelectedRecords.ToList())
            {
                await _databaseService.DeleteAttendanceRecordAsync(record);
            }
            SelectedRecords.Clear();
            ShowStatus(_localizationService.GetString("RecordsDeleted"), false);
            await LoadAttendanceAsync();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("BulkDeleteAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorDeletingRecords"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ExportAttendanceToExcelAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingExcelReport"), false);
            
            var filePath = Path.Combine(FileSystem.CacheDirectory, $"attendance_export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            await _exportService.ExportAttendanceToExcelAsync(AttendanceRecords.ToList(), filePath);
            
            if (File.Exists(filePath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = _localizationService.GetString("AttendanceReport"),
                    File = new ShareFile(filePath)
                });
                ShowStatus(_localizationService.GetString("ExcelExported"), false);
            }
            else
            {
                ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ExportAttendanceToExcelAsync", ex);
            ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ExportAttendanceToPdfAsync()
    {
        await ExportAttendanceAsync();
    }

    [RelayCommand]
    public async Task ExportSelectedEmployeePayrollPdfAsync()
    {
        if (SelectedMonthlySummary == null)
        {
            ShowStatus(_localizationService.GetString("PleaseSelectEmployee"), true);
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingPayrollReport"), false);
            
            var month = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
            var filePath = await _pdfService.GeneratePayrollPdfAsync(
                new List<MonthlyAttendanceSummary> { SelectedMonthlySummary },
                month,
                MonthlySummaryTotals);

            if (!string.IsNullOrEmpty(filePath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = _localizationService.GetString("PayrollReport"),
                    File = new ShareFile(filePath)
                });
                ShowStatus(_localizationService.GetString("PayrollExported"), false);
            }
            else
            {
                ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
            }
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ExportSelectedEmployeePayrollPdfAsync", ex);
            ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task CalendarDayTappedAsync(DailyAttendanceEntry entry)
    {
        if (entry == null || entry.IsPlaceholder) return;
        
        // Ensure users are loaded
        if (Users.Count == 0)
        {
            await LoadUsersAsync();
        }

        var records = await _databaseService.GetAttendanceRecordsAsync(entry.Date, entry.Date);
        if (records != null && records.Any())
        {
            var record = records.FirstOrDefault(r => SelectedAttendanceUser != null && r.UserId == SelectedAttendanceUser.Id) ?? records.First();
            await EditAttendanceRecord(record);
        }
        else
        {
            AttendanceDate = entry.Date;
            IsAddEditSectionExpanded = true;
        }
    }
}
