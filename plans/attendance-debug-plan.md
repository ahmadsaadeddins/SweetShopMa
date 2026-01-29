# Attendance Record Creation Workflow - Debug Plan

## Issues Identified

### Issue 1: Time Range Defaulting to 12:00 AM to 12:00 AM
**Expected Behavior**: Time range should default to 8:00 AM to 4:00 PM (standard shift hours)
**Actual Behavior**: Time range defaults to 12:00 AM to 12:00 AM

### Issue 2: Record Submission Button Unresponsive
**Expected Behavior**: Clicking "Record Attendance" button should save the attendance record
**Actual Behavior**: Button is unresponsive and fails to execute any action

---

## Root Causes Analysis

### Root Cause for Issue 1: Missing Proxy Properties in AdminViewModel

The `AttendancePage.xaml.cs` uses `AdminViewModel` as its BindingContext (line 16), but the XAML bindings reference properties that exist only in `AttendanceViewModel`. The `AdminViewModel` is missing proxy properties for:

| Property | Purpose | Location |
|----------|---------|----------|
| `AttendanceCheckInTime` | Check-in time binding | AttendanceViewModel line 34 |
| `AttendanceCheckOutTime` | Check-out time binding | AttendanceViewModel line 35 |
| `AttendanceDate` | Date picker binding | AttendanceViewModel line 31 |
| `AttendanceNotes` | Notes entry binding | AttendanceViewModel line 33 |
| `AttendancePreview` | Preview label binding | AttendanceViewModel line 36 |
| `SelectedAttendanceUser` | Employee picker selection | AttendanceViewModel line 30 |
| `AttendanceStatuses` | Status picker items source | Missing from AttendanceViewModel |
| `IsAttendanceTimeEntryEnabled` | Time picker enable/disable | Missing from AttendanceViewModel |

When these bindings fail, the TimePicker controls default to 12:00 AM.

### Root Cause for Issue 2: Missing Command Proxies in AdminViewModel

The `RecordAttendanceButton` in XAML (line 92-96) is bound to `Command="{Binding AddAttendanceCommand}"`, but the `AdminViewModel` doesn't have a proxy for this command. The command only exists in `AttendanceViewModel`.

Additionally, the XAML references commands that don't exist in AttendanceViewModel:

| Command | Purpose | Status |
|---------|---------|--------|
| `AddAttendanceCommand` | Save attendance record | Exists in AttendanceViewModel, not proxied |
| `UpdateAttendanceRecordCommand` | Update existing record | Missing from AttendanceViewModel |
| `EditAttendanceRecordCommand` | Edit existing record | Missing from AttendanceViewModel |
| `DeleteAttendanceRecordCommand` | Delete existing record | Missing from AttendanceViewModel |

---

## Solution Plan

### Step 1: Add Missing Properties to AttendanceViewModel

Add the following properties to `AttendanceViewModel.cs`:

```csharp
// Status picker items source
[ObservableProperty] private ObservableCollection<string> _attendanceStatuses = new();

// Time entry enable/disable based on status
[ObservableProperty] private bool _isAttendanceTimeEntryEnabled = true;

// Editing state
[ObservableProperty] private bool _isEditingAttendance;

// Editing record
[ObservableProperty] private AttendanceRecord _editingAttendanceRecord;

// Collapsible section visibility
[ObservableProperty] private bool _isAddEditSectionExpanded = true;
[ObservableProperty] private bool _isStatisticsSectionExpanded = true;
[ObservableSystem] private bool _isRecordsSectionExpanded = true;
[ObservableProperty] private bool _isMonthlySummarySectionExpanded = true;
[ObservableProperty] private bool _isCalendarSectionExpanded = true;
[ObservableProperty] private bool _isEmployeeComparisonSectionExpanded = true;
[ObservableProperty] private bool _isFilteringSectionExpanded = true;

// Filtering properties
[ObservableProperty] private string _filterStatus = "All";
[ObservableProperty] private string _filterOvertime = "All";

// Selected records for bulk operations
[ObservableProperty] private ObservableCollection<AttendanceRecord> _selectedRecords = new();
public bool HasSelectedRecords => SelectedRecords.Count > 0;

// Statistics summary
[ObservableProperty] private AttendanceSummary _attendanceSummary = new();

// Monthly summary properties
[ObservableProperty] private ObservableCollection<MonthlyAttendanceSummary> _monthlyAttendanceSummaries = new();
[ObservableProperty] private MonthlyAttendanceTotals _monthlySummaryTotals = new();
[ObservableProperty] private MonthlyAttendanceSummary _selectedMonthlySummary;

// Calendar properties
[ObservableProperty] private ObservableCollection<DailyAttendanceEntry> _attendanceCalendarDays = new();

// Employee comparison
[ObservableProperty] private ObservableCollection<EmployeeComparisonItem> _employeeComparisonData = new();
```

### Step 2: Add Missing Commands to AttendanceViewModel

Add the following commands to `AttendanceViewModel.cs`:

```csharp
[RelayCommand]
public async Task UpdateAttendanceRecordAsync()
{
    if (EditingAttendanceRecord == null) return;
    
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
public void EditAttendanceRecord(AttendanceRecord record)
{
    if (record == null) return;
    
    EditingAttendanceRecord = record;
    IsEditingAttendance = true;
    
    // Populate form fields
    SelectedAttendanceUser = Users.FirstOrDefault(u => u.Id == record.UserId);
    AttendanceDate = record.Date;
    SelectedAttendanceStatus = _localizationService.GetString(record.Status);
    AttendanceCheckInTime = record.CheckInTime?.TimeOfDay ?? new TimeSpan(8, 0, 0);
    AttendanceCheckOutTime = record.CheckOutTime?.TimeOfDay ?? new TimeSpan(16, 0, 0);
    AttendanceNotes = record.Notes ?? "";
}

[RelayCommand]
public async Task DeleteAttendanceRecordAsync(AttendanceRecord record)
{
    if (record == null) return;
    
    bool confirm = await Application.Current.MainPage.DisplayAlert(
        _localizationService.GetString("ConfirmDelete"),
        _localizationService.GetString("ConfirmDeleteAttendance"),
        _localizationService.GetString("Yes"),
        _localizationService.GetString("No"));
    
    if (!confirm) return;
    
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
public async Task ApplyFiltersAsync()
{
    await LoadAttendanceAsync();
}

[RelayCommand]
public void ClearFiltersAsync()
{
    FilterStartDate = DateTime.Today.AddDays(-30);
    FilterEndDate = DateTime.Today;
    FilterStatus = "All";
    FilterOvertime = "All";
    SearchText = "";
}

[RelayCommand]
public async Task BulkDeleteAsync()
{
    if (SelectedRecords.Count == 0) return;
    
    bool confirm = await Application.Current.MainPage.DisplayAlert(
        _localizationService.GetString("ConfirmDelete"),
        _localizationService.GetString("ConfirmDeleteSelected", SelectedRecords.Count),
        _localizationService.GetString("Yes"),
        _localizationService.GetString("No"));
    
    if (!confirm) return;
    
    IsBusy = true;
    try
    {
        foreach (var record in SelectedRecords)
        {
            await _databaseService.DeleteAttendanceRecordAsync(record);
        }
        ShowStatus(_localizationService.GetString("RecordsDeleted"), false);
        SelectedRecords.Clear();
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
public void ClearSelectionAsync()
{
    SelectedRecords.Clear();
}

[RelayCommand]
public async Task ExportAttendanceToExcelAsync()
{
    // Implementation for Excel export
}

[RelayCommand]
public async Task ExportAttendanceToPdfAsync()
{
    await ExportAttendanceAsync();
}

[RelayCommand]
public async Task ExportSelectedEmployeePayrollPdfAsync()
{
    if (SelectedMonthlySummary == null) return;
    
    IsBusy = true;
    try
    {
        var employeeRecords = await _databaseService.GetAttendanceRecordsAsync(
            new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1),
            SummaryMonth.AddMonths(1).AddDays(-1));
        
        var userRecords = employeeRecords.Where(r => r.UserId == SelectedMonthlySummary.UserId).ToList();
        
        var filePath = await _pdfService.GeneratePayrollPdfAsync(
            new List<MonthlyAttendanceSummary> { SelectedMonthlySummary },
            SummaryMonth,
            new MonthlyAttendanceTotals
            {
                TotalPayroll = SelectedMonthlySummary.Payroll,
                TotalPresentDays = SelectedMonthlySummary.DaysPresent,
                TotalAbsentDays = SelectedMonthlySummary.DaysAbsent
            });
        
        if (!string.IsNullOrEmpty(filePath))
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = _localizationService.GetString("EmployeePayrollReport"),
                File = new ShareFile(filePath)
            });
            ShowStatus(_localizationService.GetString("PayrollExported"), false);
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
public async Task CalendarDayTappedAsync(DailyAttendanceEntry day)
{
    if (day == null || day.IsPlaceholder) return;
    
    AttendanceDate = day.Date;
    IsAddEditSectionExpanded = true;
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
```

### Step 3: Add Proxy Properties to AdminViewModel

Add the following proxy properties and commands to `AdminViewModel.cs`:

```csharp
// Attendance Proxies - Add to existing section (after line 151)
public ObservableCollection<AttendanceRecord> AttendanceRecords => Attendance.AttendanceRecords;
public ObservableCollection<DailyAttendanceEntry> AttendanceCalendarDays => Attendance.AttendanceCalendarDays;
public ObservableCollection<MonthlyAttendanceSummary> MonthlyAttendanceSummaries => Attendance.MonthlyAttendanceSummaries;
public MonthlyAttendanceTotals MonthlySummaryTotals => Attendance.MonthlySummaryTotals;
public MonthlyAttendanceSummary SelectedMonthlySummary
{
    get => Attendance.SelectedMonthlySummary;
    set => Attendance.SelectedMonthlySummary = value;
}
public AttendanceSummary AttendanceSummary => Attendance.AttendanceSummary;
public ObservableCollection<EmployeeComparisonItem> EmployeeComparisonData => Attendance.EmployeeComparisonData;

// Attendance Form Properties
public User SelectedAttendanceUser
{
    get => Attendance.SelectedAttendanceUser;
    set => Attendance.SelectedAttendanceUser = value;
}
public DateTime AttendanceDate
{
    get => Attendance.AttendanceDate;
    set => Attendance.AttendanceDate = value;
}
public string SelectedAttendanceStatus
{
    get => Attendance.SelectedAttendanceStatus;
    set => Attendance.SelectedAttendanceStatus = value;
}
public string AttendanceNotes
{
    get => Attendance.AttendanceNotes;
    set => Attendance.AttendanceNotes = value;
}
public TimeSpan AttendanceCheckInTime
{
    get => Attendance.AttendanceCheckInTime;
    set => Attendance.AttendanceCheckInTime = value;
}
public TimeSpan AttendanceCheckOutTime
{
    get => Attendance.AttendanceCheckOutTime;
    set => Attendance.AttendanceCheckOutTime = value;
}
public string AttendancePreview
{
    get => Attendance.AttendancePreview;
    set => Attendance.AttendancePreview = value;
}
public ObservableCollection<string> AttendanceStatuses
{
    get => Attendance.AttendanceStatuses;
    set => Attendance.AttendanceStatuses = value;
}
public bool IsAttendanceTimeEntryEnabled
{
    get => Attendance.IsAttendanceTimeEntryEnabled;
    set => Attendance.IsAttendanceTimeEntryEnabled = value;
}
public bool IsEditingAttendance
{
    get => Attendance.IsEditingAttendance;
    set => Attendance.IsEditingAttendance = value;
}
public AttendanceRecord EditingAttendanceRecord
{
    get => Attendance.EditingAttendanceRecord;
    set => Attendance.EditingAttendanceRecord = value;
}

// Collapsible Section Visibility
public bool IsAddEditSectionExpanded
{
    get => Attendance.IsAddEditSectionExpanded;
    set => Attendance.IsAddEditSectionExpanded = value;
}
public bool IsStatisticsSectionExpanded
{
    get => Attendance.IsStatisticsSectionExpanded;
    set => Attendance.IsStatisticsSectionExpanded = value;
}
public bool IsRecordsSectionExpanded
{
    get => Attendance.IsRecordsSectionExpanded;
    set => Attendance.IsRecordsSectionExpanded = value;
}
public bool IsMonthlySummarySectionExpanded
{
    get => Attendance.IsMonthlySummarySectionExpanded;
    set => Attendance.IsMonthlySummarySectionExpanded = value;
}
public bool IsCalendarSectionExpanded
{
    get => Attendance.IsCalendarSectionExpanded;
    set => Attendance.IsCalendarSectionExpanded = value;
}
public bool IsEmployeeComparisonSectionExpanded
{
    get => Attendance.IsEmployeeComparisonSectionExpanded;
    set => Attendance.IsEmployeeComparisonSectionExpanded = value;
}
public bool IsFilteringSectionExpanded
{
    get => Attendance.IsFilteringSectionExpanded;
    set => Attendance.IsFilteringSectionExpanded = value;
}

// Filtering Properties
public DateTime FilterStartDate
{
    get => Attendance.FilterStartDate;
    set => Attendance.FilterStartDate = value;
}
public DateTime FilterEndDate
{
    get => Attendance.FilterEndDate;
    set => Attendance.FilterEndDate = value;
}
public string FilterStatus
{
    get => Attendance.FilterStatus;
    set => Attendance.FilterStatus = value;
}
public string FilterOvertime
{
    get => Attendance.FilterOvertime;
    set => Attendance.FilterOvertime = value;
}
public string SearchText
{
    get => Attendance.SearchText;
    set => Attendance.SearchText = value;
}

// Selected Records for Bulk Operations
public ObservableCollection<AttendanceRecord> SelectedRecords
{
    get => Attendance.SelectedRecords;
    set => Attendance.SelectedRecords = value;
}
public bool HasSelectedRecords => Attendance.HasSelectedRecords;

// Monthly Summary Properties
public DateTime SummaryMonth
{
    get => Attendance.SummaryMonth;
    set => Attendance.SummaryMonth = value;
}

// Expense Properties
public DateTime ExpenseDate
{
    get => Attendance.ExpenseDate;
    set => Attendance.ExpenseDate = value;
}
public string ExpenseAmount
{
    get => Attendance.ExpenseAmount;
    set => Attendance.ExpenseAmount = value;
}
public string ExpenseCategory
{
    get => Attendance.ExpenseCategory;
    set => Attendance.ExpenseCategory = value;
}
public string ExpenseNotes
{
    get => Attendance.ExpenseNotes;
    set => Attendance.ExpenseNotes = value;
}

// Commands - Add to existing section (after line 134)
public IAsyncRelayCommand AddAttendanceCommand => Attendance.AddAttendanceCommand;
public IAsyncRelayCommand UpdateAttendanceRecordCommand => Attendance.UpdateAttendanceRecordCommand;
public IRelayCommand<AttendanceRecord> EditAttendanceRecordCommand => Attendance.EditAttendanceRecordCommand;
public IAsyncRelayCommand<AttendanceRecord> DeleteAttendanceRecordCommand => Attendance.DeleteAttendanceRecordCommand;
public IAsyncRelayCommand ApplyFiltersCommand => Attendance.ApplyFiltersCommand;
public IRelayCommand ClearFiltersCommand => Attendance.ClearFiltersCommand;
public IAsyncRelayCommand BulkDeleteCommand => Attendance.BulkDeleteCommand;
public IRelayCommand ClearSelectionCommand => Attendance.ClearSelectionCommand;
public IAsyncRelayCommand ExportAttendanceToExcelCommand => Attendance.ExportAttendanceToExcelCommand;
public IAsyncRelayCommand ExportAttendanceToPdfCommand => Attendance.ExportAttendanceToPdfCommand;
public IAsyncRelayCommand ExportSelectedEmployeePayrollPdfCommand => Attendance.ExportSelectedEmployeePayrollPdfCommand;
public IAsyncRelayCommand<DailyAttendanceEntry> CalendarDayTappedCommand => Attendance.CalendarDayTappedCommand;
public IAsyncRelayCommand AddExpenseCommand => Attendance.AddExpenseCommand;
public IAsyncRelayCommand<EmployeeExpense> DeleteExpenseCommand => Attendance.DeleteExpenseCommand;
```

### Step 4: Initialize AttendanceStatuses in AttendanceViewModel Constructor

Update the constructor in `AttendanceViewModel.cs` to initialize the attendance statuses:

```csharp
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
    
    // Initialize attendance statuses
    AttendanceStatuses.Clear();
    AttendanceStatuses.Add(_localizationService.GetString("Present"));
    AttendanceStatuses.Add(_localizationService.GetString("Reset"));
    AttendanceStatuses.Add(_localizationService.GetString("AbsentWithPermission"));
    AttendanceStatuses.Add(_localizationService.GetString("AbsentWithoutPermission"));
}
```

### Step 5: Add Missing Model Classes

Create or update the following model classes:

```csharp
// Models/AttendanceSummary.cs
namespace SweetShopMa.Models;

public class AttendanceSummary
{
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int OvertimeCount { get; set; }
    public decimal TotalRegularHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalPayroll { get; set; }
}

// Models/MonthlyAttendanceTotals.cs
namespace SweetShopMa.Models;

public class MonthlyAttendanceTotals
{
    public decimal TotalPayroll { get; set; }
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalRestPayout { get; set; }
    public decimal TotalAbsenceDeductions { get; set; }
}

// Models/EmployeeComparisonItem.cs
namespace SweetShopMa.Models;

public class EmployeeComparisonItem
{
    public string UserName { get; set; }
    public int DaysWorked { get; set; }
    public decimal HoursWorked { get; set; }
    public string HoursDisplay => $"{HoursWorked:F1}h";
    public decimal Payroll { get; set; }
    public string PayrollDisplay => $"${Payroll:F2}";
}

// Models/DailyAttendanceEntry.cs
namespace SweetShopMa.Models;

public class DailyAttendanceEntry
{
    public DateTime Date { get; set; }
    public string DayText => Date.Day.ToString();
    public string DetailText { get; set; } = "";
    public bool IsPlaceholder { get; set; }
    public bool IsPresent { get; set; }
    public bool IsAbsent { get; set; }
    public bool HasOvertime { get; set; }
}
```

### Step 6: Update DatabaseService Methods

Ensure the `DatabaseService` has the following methods:

```csharp
public Task DeleteAttendanceRecordAsync(AttendanceRecord record)
{
    return _database.DeleteAsync(record);
}
```

---

## Implementation Order

1. **Add missing model classes** (AttendanceSummary, MonthlyAttendanceTotals, EmployeeComparisonItem, DailyAttendanceEntry)
2. **Update AttendanceViewModel** with missing properties and commands
3. **Update AdminViewModel** with proxy properties and commands
4. **Update DatabaseService** if needed
5. **Test the fixes**

---

## Testing Checklist

- [ ] Time range defaults to 8:00 AM to 4:00 PM
- [ ] Record Attendance button saves data successfully
- [ ] Update Attendance button works correctly
- [ ] Edit and Delete commands work
- [ ] All bindings are working (no binding errors in output)
- [ ] Time entry is enabled/disabled based on attendance status
- [ ] Status picker shows all available statuses
- [ ] Collapsible sections work correctly
- [ ] Filtering and search work correctly
- [ ] Bulk operations work correctly
- [ ] Export functions work correctly
- [ ] Calendar interaction works correctly
- [ ] Expense management works correctly
