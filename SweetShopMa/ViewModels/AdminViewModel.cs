using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SweetShopMa.Models;
using SweetShopMa.Services;
using SweetShopMa.Utils;
using SweetShopMa.Views;

namespace SweetShopMa.ViewModels;

/// <summary>
/// ViewModel for the Admin Panel (AdminPage).
/// 
/// WHAT IS ADMINVIEWMODEL?
/// AdminViewModel manages all admin functionality including:
/// - User management (create, enable/disable users)
/// - Product management (add products)
/// - Reports and insights (sales, orders, top products)
/// - Attendance tracking (record and view attendance)
/// 
/// KEY RESPONSIBILITIES:
/// - Load and display users (excluding Developer users)
/// - Load and display products
/// - Calculate and display sales reports
/// - Manage attendance records
/// - Calculate monthly attendance summaries
/// 
/// PERMISSIONS:
/// This ViewModel checks user permissions to show/hide features:
/// - CanManageUsers: Show user management section
/// - CanManageStock: Show product management section
/// - CanUseAttendanceTracker: Show attendance tracker button
/// - IsDeveloper: Show Developer Setup button
/// 
/// DATA BINDING:
/// Properties are bound to UI elements in AdminPage.xaml.
/// ObservableCollections automatically update the UI when items are added/removed.
/// </summary>
public class AdminViewModel : INotifyPropertyChanged
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Services.LocalizationService _localizationService;
    private readonly AttendanceRulesService _attendanceRulesService;
    private readonly Services.IPdfService _pdfService;

    private bool _isBusy;
    private string _statusMessage = "";
    private bool _isErrorStatus;

    // User form fields
    private string _newUserName = "";
    private string _newUserUsername = "";
    private string _newUserPassword = "";
    private bool _newUserIsAdmin = true;

    // Product form fields
    private string _newProductName = "";
    private string _newProductEmoji = "🍬";
    private string _newProductBarcode = "";
    private string _newProductPrice = "";
    private string _newProductStock = "";
    private bool _newProductIsWeight;
    private string _newProductCategory = "All";

    // Edit product fields
    private Product _selectedProduct;
    private string _editProductName = "";
    private string _editProductEmoji = "";
    private string _editProductCategory = "";
    private string _editProductPrice = "";

    // Product search
    private string _productSearchText = "";

    // Report metrics
    private decimal _totalSales;
    private int _totalOrders;
    private decimal _averageOrderValue;
    private decimal _totalItemsSold;
    private decimal _last7DaysSales;
    private string _topProductName = "No sales yet";
    private string _topProductDetails = "Add items to see insights";

    public ObservableCollection<User> Users { get; } = new();
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<Product> FilteredProducts { get; } = new();
    public ObservableCollection<Order> RecentOrders { get; } = new();
    public ObservableCollection<ProductReportItem> TopProducts { get; } = new();
    public ObservableCollection<AttendanceRecord> AttendanceRecords { get; } = new();
    public ObservableCollection<DailyAttendanceEntry> AttendanceCalendarDays { get; } = new();
    public ObservableCollection<MonthlyAttendanceSummary> MonthlyAttendanceSummaries { get; } = new();
    public ObservableCollection<EmployeeExpense> EmployeeExpenses { get; } = new();

    public ICommand AddUserCommand { get; }
    public ICommand AddProductCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ToggleUserStatusCommand { get; }
    public ICommand OpenOrderDetailsCommand { get; }
    public ICommand AddAttendanceCommand { get; }
    public ICommand DeleteAttendanceRecordCommand { get; }
    public ICommand EditAttendanceRecordCommand { get; }
    public ICommand UpdateAttendanceRecordCommand { get; }
    public ICommand OpenAttendancePageCommand { get; }
    public ICommand CalendarDayTappedCommand { get; }
    public ICommand EditProductCommand { get; }
    public ICommand UpdateProductCommand { get; }
    public ICommand CancelEditProductCommand { get; }
    public ICommand ExportPayrollPdfCommand { get; }
    public ICommand AddExpenseCommand { get; }
    public ICommand DeleteExpenseCommand { get; }
    public ICommand ExportSelectedEmployeePayrollPdfCommand { get; }
    
    // Export commands
    public ICommand ExportAttendanceToExcelCommand { get; }
    public ICommand ExportAttendanceToPdfCommand { get; }
    public ICommand ExportSalesReportCommand { get; }
    public ICommand ExportInventoryReportCommand { get; }
    
    // Bulk operation commands
    public ICommand BulkDeleteCommand { get; }
    public ICommand BulkEditCommand { get; }
    public ICommand SelectAllRecordsCommand { get; }
    public ICommand ClearSelectionCommand { get; }
    
    // Filter commands
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }

    // Attendance form fields
    private User _selectedAttendanceUser;
    private DateTime _attendanceDate = DateTime.Today;
    private string _selectedAttendanceStatus = "Present";
    private string _attendanceNotes = "";
    private TimeSpan _attendanceCheckInTime = new(8, 0, 0);
    private TimeSpan _attendanceCheckOutTime = new(16, 0, 0);
    private string _attendancePreview = "Regular: 0h • OT: 0h • Pay $0.00";
    private AttendanceRecord _editingAttendanceRecord;
    private bool _isEditingAttendance;

    // Employee expenses fields
    private User _selectedExpenseUser;
    private string _expenseAmount = "";
    private string _expenseCategory = "";
    private string _expenseNotes = "";
    private DateTime _expenseDate = DateTime.Today;

    // Filtering properties for attendance
    private DateTime _filterStartDate = DateTime.Today.AddDays(-30);
    private DateTime _filterEndDate = DateTime.Today;
    private List<User> _selectedFilterEmployees = new();
    private string _filterStatus = "All";
    private string _filterOvertime = "All"; // "All", "With OT", "Without OT"
    private string _searchText = "";

    // Enhanced statistics properties
    private decimal _averageHoursPerDay = 0m;
    private int _consecutiveWorkingDays = 0;
    private MonthlyComparisonData _monthlyComparison = new();
    private List<EmployeeComparisonItem> _employeeComparisonData = new();
    
    // Bulk operations
    private List<AttendanceRecord> _selectedRecords = new();

    private readonly string[] _attendanceStatuses =
        { "Present", "Reset", "Absent (With Permission)", "Absent (Without Permission)" };

    private AttendanceSummary _attendanceSummary = new();
    private MonthlyAttendanceTotals _monthlySummaryTotals = new();
    private DateTime _summaryMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private MonthlyAttendanceSummary _selectedMonthlySummary;
    private List<AttendanceRecord> _currentMonthRecords = new();
    private string _newUserSalary = "0";

    public AdminViewModel(DatabaseService databaseService, AuthService authService, IServiceProvider serviceProvider, Services.LocalizationService localizationService, Services.IPdfService pdfService, AttendanceRulesService attendanceRulesService)
    {
        _databaseService = databaseService;
        _authService = authService;
        _serviceProvider = serviceProvider;
        _localizationService = localizationService;
        _pdfService = pdfService;
        _attendanceRulesService = attendanceRulesService;

        AddUserCommand = new Command(async () => await AddUserAsync(), () => !IsBusy);
        AddProductCommand = new Command(async () => await AddProductAsync(), () => !IsBusy);
        RefreshCommand = new Command(async () => await InitializeAsync(), () => !IsBusy);
        ToggleUserStatusCommand = new Command<User>(async user => await ToggleUserStatusAsync(user), _ => !IsBusy);
        OpenOrderDetailsCommand = new Command<Order>(async order => await ShowOrderDetailsAsync(order));
        AddAttendanceCommand = new Command(async () => await AddAttendanceAsync(), () => !IsBusy);
        DeleteAttendanceRecordCommand = new Command<AttendanceRecord>(async r => await DeleteAttendanceRecordAsync(r));
        EditAttendanceRecordCommand = new Command<AttendanceRecord>(async r => await EditAttendanceRecordAsync(r));
        UpdateAttendanceRecordCommand = new Command(async () => await UpdateAttendanceRecordAsync(), () => IsEditingAttendance);
        OpenAttendancePageCommand = new Command(async () => await OpenAttendancePage());
        CalendarDayTappedCommand = new Command<DailyAttendanceEntry>(async entry => await HandleCalendarDayTappedAsync(entry));
        EditProductCommand = new Command<Product>(async product => await EditProductAsync(product));
        UpdateProductCommand = new Command(async () => await UpdateProductAsync());
        CancelEditProductCommand = new Command(() => CancelEditProduct());
        ExportPayrollPdfCommand = new Command(async () => await ExportPayrollPdfAsync(), () => !IsBusy);
        AddExpenseCommand = new Command(async () => await AddExpenseAsync());
        DeleteExpenseCommand = new Command<EmployeeExpense>(async e => await DeleteExpenseAsync(e));
        ExportSelectedEmployeePayrollPdfCommand = new Command(async () => await ExportSelectedEmployeePayrollPdfAsync(), () => SelectedMonthlySummary != null);
        
        // Export commands
        ExportAttendanceToExcelCommand = new Command(async () => await ExportAttendanceToExcelAsync(), () => !IsBusy);
        ExportAttendanceToPdfCommand = new Command(async () => await ExportAttendanceToPdfAsync(), () => !IsBusy);
        ExportSalesReportCommand = new Command(async () => await ExportSalesReportAsync(), () => !IsBusy);
        ExportInventoryReportCommand = new Command(async () => await ExportInventoryReportAsync(), () => !IsBusy);
        
        // Bulk operation commands
        BulkDeleteCommand = new Command(async () => await BulkDeleteRecordsAsync(), () => SelectedRecords.Count > 0);
        BulkEditCommand = new Command(async () => await BulkEditRecordsAsync(), () => SelectedRecords.Count > 0);
        SelectAllRecordsCommand = new Command(() => SelectAllRecords());
        ClearSelectionCommand = new Command(() => ClearSelection());
        
        // Filter commands
        ApplyFiltersCommand = new Command(async () => await ApplyFiltersAsync());
        ClearFiltersCommand = new Command(() => ClearFilters());

        _authService.OnUserChanged += _ => OnPropertyChanged(nameof(IsAuthorized));
        TopProducts.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasReportData));
            OnPropertyChanged(nameof(ReportStatusText));
            OnPropertyChanged(nameof(ReportStatusTextColor));
        };

        _ = UpdateAttendancePreviewAsync();
    }

    public User SelectedExpenseUser
    {
        get => _selectedExpenseUser;
        set { if (_selectedExpenseUser != value) { _selectedExpenseUser = value; OnPropertyChanged(); _ = LoadEmployeeExpensesAsync(); } }
    }

    public string ExpenseAmount
    {
        get => _expenseAmount;
        set { if (_expenseAmount != value) { _expenseAmount = value; OnPropertyChanged(); } }
    }

    public string ExpenseCategory
    {
        get => _expenseCategory;
        set { if (_expenseCategory != value) { _expenseCategory = value; OnPropertyChanged(); } }
    }

    public string ExpenseNotes
    {
        get => _expenseNotes;
        set { if (_expenseNotes != value) { _expenseNotes = value; OnPropertyChanged(); } }
    }

    public DateTime ExpenseDate
    {
        get => _expenseDate;
        set { if (_expenseDate != value) { _expenseDate = value; OnPropertyChanged(); } }
    }

    private async Task LoadMonthlySummaryAsync()
    {
        try
        {
            var monthStart = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var today = DateTime.Today;

            _currentMonthRecords = await _databaseService.GetAttendanceRecordsAsync(monthStart, monthEnd);
            if (_currentMonthRecords == null)
                _currentMonthRecords = new List<AttendanceRecord>();

            List<User> userSnapshot = null;
            try
            {
                if (Users != null && Users.Count > 0)
                {
                    userSnapshot = Users.ToList();
                }
                else
                {
                    var usersFromDb = await _databaseService.GetUsersAsync();
                    userSnapshot = usersFromDb?.ToList() ?? new List<User>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting users in LoadMonthlySummaryAsync: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                userSnapshot = new List<User>();
            }
            
            if (userSnapshot == null)
                userSnapshot = new List<User>();

            var summaries = new List<MonthlyAttendanceSummary>();

            if (userSnapshot == null || !userSnapshot.Any())
            {
                MonthlyAttendanceSummaries.Clear();
                MonthlySummaryTotals = new MonthlyAttendanceTotals();
                SelectedMonthlySummary = null;
                return;
            }

            foreach (var user in userSnapshot)
            {
                if (user == null) continue;
                
                try
                {
                    var userRecords = _currentMonthRecords
                        .Where(r => r != null && r.UserId == user.Id)
                        .ToList();
                    
                    // Create dictionary for quick lookups, handling duplicate dates by taking the first
                    var recordByDate = new Dictionary<DateTime, AttendanceRecord>();
                    try
                    {
                        if (userRecords != null && userRecords.Any())
                        {
                            var grouped = userRecords
                                .Where(r => r != null)
                                .GroupBy(r => r.Date.Date)
                                .ToList();
                            
                            foreach (var group in grouped)
                            {
                                if (group != null)
                                {
                                    try
                                    {
                                        var firstRecord = group.FirstOrDefault();
                                        if (firstRecord != null)
                                            recordByDate[group.Key] = firstRecord;
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Error getting first record from group: {ex}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error creating recordByDate dictionary: {ex}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        // Continue with empty dictionary
                    }

                    int presentDays = 0;
                    int absentDays = 0;
                    int absentWithPermission = 0;
                    int absentWithoutPermission = 0;

                    for (var day = monthStart; day <= monthEnd && day <= today; day = day.AddDays(1))
                    {
                        if (recordByDate.TryGetValue(day.Date, out var rec))
                        {
                            if (rec == null) continue;
                            
                            // Reset days don't count as present or absent (they're outside the cycle)
                            if (rec.AbsencePermissionType == "Reset")
                            {
                                // Reset days are paid but don't count in cycle
                                continue;
                            }
                            else if (rec.IsPresent)
                            {
                                presentDays++;
                            }
                            else
                            {
                                absentDays++;
                                if (rec.AbsencePermissionType == "WithPermission")
                                    absentWithPermission++;
                                else if (rec.AbsencePermissionType == "WithoutPermission")
                                    absentWithoutPermission++;
                            }
                        }
                        // Do not count missing days as absent; only recorded absences
                    }

                    // Compute monthly stats with fixed daily rate (/30) and rest rules
                    var stats = _attendanceRulesService.ComputeMonthly(user, SummaryMonth, userRecords ?? new List<AttendanceRecord>());
                    
                    // Start with full monthly salary
                    decimal fullSalary = user?.MonthlySalary ?? 0m;
                    
                    // Calculate overtime pay from records (additional to base salary)
                    decimal overtimePay = 0m;
                    try
                    {
                        overtimePay = userRecords
                            .Where(r => r != null && r.IsPresent)
                            .Sum(r => 
                            {
                                if (r != null && r.OvertimeHours > 0)
                                {
                                    var hourlyRate = stats.dailyRate / 8m;
                                    var otMultiplier = user?.OvertimeMultiplier ?? 1.5m;
                                    return r.OvertimeHours * hourlyRate * otMultiplier;
                                }
                                return 0m;
                            });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error calculating overtime pay: {ex}");
                        overtimePay = 0m;
                    }
                    
                    // Subtract absence deductions (penalties for absences)
                    // Subtract expenses
                    int daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
                    decimal virtualDaysPay = daysInMonth == 28 ? (2m * stats.dailyRate) : 0m;
                    var expenses = await _databaseService.GetEmployeeExpensesAsync(user.Id, monthStart, monthEnd);
                    decimal expensesTotal = expenses != null ? expenses.Sum(e => e?.Amount ?? 0m) : 0m;
                    
                    // Final calculation: Full Salary + Overtime + Rest Bonus - Absence Deductions - Expenses + Virtual Days (if 28-day month)
                    decimal finalPayroll = fullSalary + overtimePay + stats.restPayout - stats.absenceDeductions - expensesTotal + virtualDaysPay;

                    var summary = new MonthlyAttendanceSummary
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        DaysPresent = userRecords.Count(r => r != null && r.IsPresent),
                        DaysAbsent = userRecords.Count(r => r != null && !r.IsPresent),
                        WorkedDays = stats.workedDays,
                        EarnedRestDays = stats.restDays,
                        RestDayPayout = stats.restPayout,
                        AbsenceWithPermission = stats.withPermissionAbsences,
                        AbsenceWithoutPermission = stats.withoutPermissionAbsences,
                        AbsenceDeductions = stats.absenceDeductions,
                        OvertimeHours = userRecords.Where(r => r != null).Sum(r => r.OvertimeHours),
                        TotalHours = userRecords.Where(r => r != null).Sum(r => r.RegularHours + r.OvertimeHours),
                        ExpensesTotal = expensesTotal,
                        Payroll = Math.Max(0m, finalPayroll)
                    };
                    summaries.Add(summary);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing user {user?.Name ?? "Unknown"} in LoadMonthlySummaryAsync: {ex}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException}");
                    // Continue with next user
                }
            }

            MonthlyAttendanceSummaries.Clear();
        if (summaries != null && summaries.Any())
        {
            foreach (var summary in summaries.OrderBy(s => s?.UserName ?? ""))
            {
                if (summary != null)
                    MonthlyAttendanceSummaries.Add(summary);
            }
        }

        // Calculate totals correctly - sum all components from individual summaries
        // The Payroll field in each summary already includes: basePayroll + restPayout - absenceDeductions - expenses + virtualDaysPay
        // So we can simply sum the Payroll values, but we need to recalculate to ensure accuracy
        decimal totalRestPayout = summaries?.Where(s => s != null).Sum(s => s.RestDayPayout) ?? 0m;
        decimal totalAbsenceDeductions = summaries?.Where(s => s != null).Sum(s => s.AbsenceDeductions) ?? 0m;
        decimal totalExpenses = summaries?.Where(s => s != null).Sum(s => s.ExpensesTotal) ?? 0m;
        
        // Recalculate total payroll from individual summaries (which already have correct calculations)
        decimal totalPayroll = summaries?.Where(s => s != null).Sum(s => s.Payroll) ?? 0m;
        
        MonthlySummaryTotals = new MonthlyAttendanceTotals
        {
            TotalPayroll = totalPayroll,
            TotalOvertimeHours = summaries?.Where(s => s != null).Sum(s => s.OvertimeHours) ?? 0m,
            TotalPresentDays = summaries?.Where(s => s != null).Sum(s => s.DaysPresent) ?? 0,
            TotalAbsentDays = summaries?.Where(s => s != null).Sum(s => s.DaysAbsent) ?? 0,
            TotalRestPayout = totalRestPayout,
            TotalAbsenceDeductions = totalAbsenceDeductions
        };

        SelectedMonthlySummary = MonthlyAttendanceSummaries?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadMonthlySummaryAsync: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException}");
            // Ensure collections are initialized even on error
            // MonthlyAttendanceSummaries is read-only, so we can't assign to it
            // Just ensure _currentMonthRecords is initialized
            if (_currentMonthRecords == null)
                _currentMonthRecords = new List<AttendanceRecord>();
            
            // Initialize empty summaries to prevent null reference errors
            if (MonthlyAttendanceSummaries == null || !MonthlyAttendanceSummaries.Any())
            {
                MonthlySummaryTotals = new MonthlyAttendanceTotals();
            }
        }
    }

    // Permission properties
    public bool IsAuthorized => _authService.CanManageUsers || _authService.CanManageStock;
    public bool CanManageUsers => _authService.CanManageUsers;
    public bool CanUseAttendanceTracker => _authService.CanUseAttendanceTracker && Services.FeatureFlags.IsAttendanceTrackerEnabled;
    public bool CanManageStock => _authService.CanManageStock;
    public bool IsDeveloper => _authService.IsDeveloper;

    public string DatabasePath => DatabaseService.DatabasePath;
    public string AppDataDirectory => DatabaseService.AppDataDirectory;

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
                (AddUserCommand as Command)?.ChangeCanExecute();
                (AddProductCommand as Command)?.ChangeCanExecute();
                (UpdateProductCommand as Command)?.ChangeCanExecute();
                (EditProductCommand as Command)?.ChangeCanExecute();
                (RefreshCommand as Command)?.ChangeCanExecute();
                (ToggleUserStatusCommand as Command)?.ChangeCanExecute();
                (AddAttendanceCommand as Command)?.ChangeCanExecute();
                (ExportPayrollPdfCommand as Command)?.ChangeCanExecute();
                (AddAttendanceCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

    public bool IsErrorStatus
    {
        get => _isErrorStatus;
        set
        {
            if (_isErrorStatus != value)
            {
                _isErrorStatus = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal TotalSales
    {
        get => _totalSales;
        set
        {
            if (_totalSales != value)
            {
                _totalSales = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasReportData));
                OnPropertyChanged(nameof(ReportStatusText));
                OnPropertyChanged(nameof(ReportStatusTextColor));
            }
        }
    }

    public int TotalOrders
    {
        get => _totalOrders;
        set
        {
            if (_totalOrders != value)
            {
                _totalOrders = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasReportData));
                OnPropertyChanged(nameof(ReportStatusText));
                OnPropertyChanged(nameof(ReportStatusTextColor));
            }
        }
    }

    public decimal AverageOrderValue
    {
        get => _averageOrderValue;
        set
        {
            if (_averageOrderValue != value)
            {
                _averageOrderValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AverageOrderValueDisplay));
            }
        }
    }

    public string AverageOrderValueDisplay
    {
        get
        {
            var avgText = _localizationService.GetString("Average");
            return $"{avgText} ${_averageOrderValue:F2}";
        }
    }

    public decimal TotalItemsSold
    {
        get => _totalItemsSold;
        set
        {
            if (_totalItemsSold != value)
            {
                _totalItemsSold = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal Last7DaysSales
    {
        get => _last7DaysSales;
        set
        {
            if (_last7DaysSales != value)
            {
                _last7DaysSales = value;
                OnPropertyChanged();
            }
        }
    }

    public string TopProductName
    {
        get => _topProductName;
        set
        {
            if (_topProductName != value)
            {
                _topProductName = value;
                OnPropertyChanged();
            }
        }
    }

    public string TopProductDetails
    {
        get => _topProductDetails;
        set
        {
            if (_topProductDetails != value)
            {
                _topProductDetails = value;
                OnPropertyChanged();
            }
        }
    }

    public bool HasReportData => TotalSales > 0 || TotalOrders > 0 || TopProducts.Any();

    public string ReportStatusText
    {
        get
        {
            return HasReportData 
                ? _localizationService.GetString("DataReady") 
                : _localizationService.GetString("NoDataYet");
        }
    }

    public string ReportStatusTextColor => HasReportData ? "#1f7a4d" : "#c00000";

    public IEnumerable<string> AttendanceStatuses => _attendanceStatuses;

    public User SelectedAttendanceUser
    {
        get => _selectedAttendanceUser;
        set
        {
            if (_selectedAttendanceUser != value)
            {
                _selectedAttendanceUser = value;
                OnPropertyChanged();
                _ = UpdateAttendancePreviewAsync();
                SelectedExpenseUser = value;
                
                // Only load expenses if we have a valid user
                if (value != null)
                {
                    _ = LoadEmployeeExpensesAsync();
                }
            }
        }
    }

    public DateTime AttendanceDate
    {
        get => _attendanceDate;
        set
        {
            if (_attendanceDate != value)
            {
                _attendanceDate = value;
                OnPropertyChanged();
                _ = UpdateAttendancePreviewAsync();
            }
        }
    }

    public string SelectedAttendanceStatus
    {
        get => _selectedAttendanceStatus;
        set
        {
            if (_selectedAttendanceStatus != value)
            {
                _selectedAttendanceStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAttendanceTimeEntryEnabled));
                _ = UpdateAttendancePreviewAsync();
            }
        }
    }

    public string AttendanceNotes
    {
        get => _attendanceNotes;
        set { if (_attendanceNotes != value) { _attendanceNotes = value; OnPropertyChanged(); } }
    }

    public TimeSpan AttendanceCheckInTime
    {
        get => _attendanceCheckInTime;
        set
        {
            if (_attendanceCheckInTime != value)
            {
                _attendanceCheckInTime = value;
                OnPropertyChanged();
                _ = UpdateAttendancePreviewAsync();
            }
        }
    }

    public TimeSpan AttendanceCheckOutTime
    {
        get => _attendanceCheckOutTime;
        set
        {
            if (_attendanceCheckOutTime != value)
            {
                _attendanceCheckOutTime = value;
                OnPropertyChanged();
                _ = UpdateAttendancePreviewAsync();
            }
        }
    }

    public bool IsAttendanceTimeEntryEnabled => StatusRequiresTimes(SelectedAttendanceStatus);

    public string AttendancePreview
    {
        get => _attendancePreview;
        set
        {
            if (_attendancePreview != value)
            {
                _attendancePreview = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEditingAttendance
    {
        get => _isEditingAttendance;
        set { if (_isEditingAttendance != value) { _isEditingAttendance = value; OnPropertyChanged(); (UpdateAttendanceRecordCommand as Command)?.ChangeCanExecute(); } }
    }

    public AttendanceSummary AttendanceSummary
    {
        get => _attendanceSummary;
        set { if (_attendanceSummary != value) { _attendanceSummary = value; OnPropertyChanged(); } }
    }

    // Filtering properties
    public DateTime FilterStartDate
    {
        get => _filterStartDate;
        set { if (_filterStartDate != value) { _filterStartDate = value; OnPropertyChanged(); } }
    }

    public DateTime FilterEndDate
    {
        get => _filterEndDate;
        set { if (_filterEndDate != value) { _filterEndDate = value; OnPropertyChanged(); } }
    }

    public List<User> SelectedFilterEmployees
    {
        get => _selectedFilterEmployees;
        set { if (_selectedFilterEmployees != value) { _selectedFilterEmployees = value ?? new List<User>(); OnPropertyChanged(); } }
    }

    public string FilterStatus
    {
        get => _filterStatus;
        set { if (_filterStatus != value) { _filterStatus = value; OnPropertyChanged(); } }
    }

    public string FilterOvertime
    {
        get => _filterOvertime;
        set { if (_filterOvertime != value) { _filterOvertime = value; OnPropertyChanged(); } }
    }

    public string SearchText
    {
        get => _searchText;
        set { if (_searchText != value) { _searchText = value; OnPropertyChanged(); } }
    }

    // Enhanced statistics properties
    public decimal AverageHoursPerDay
    {
        get => _averageHoursPerDay;
        set { if (_averageHoursPerDay != value) { _averageHoursPerDay = value; OnPropertyChanged(); } }
    }

    public int ConsecutiveWorkingDays
    {
        get => _consecutiveWorkingDays;
        set { if (_consecutiveWorkingDays != value) { _consecutiveWorkingDays = value; OnPropertyChanged(); } }
    }

    public MonthlyComparisonData MonthlyComparison
    {
        get => _monthlyComparison;
        set { if (_monthlyComparison != value) { _monthlyComparison = value; OnPropertyChanged(); } }
    }

    public List<EmployeeComparisonItem> EmployeeComparisonData
    {
        get => _employeeComparisonData;
        set { if (_employeeComparisonData != value) { _employeeComparisonData = value ?? new List<EmployeeComparisonItem>(); OnPropertyChanged(); } }
    }

    // Bulk operations
    public List<AttendanceRecord> SelectedRecords
    {
        get => _selectedRecords;
        set { if (_selectedRecords != value) { _selectedRecords = value ?? new List<AttendanceRecord>(); OnPropertyChanged(); OnPropertyChanged(nameof(HasSelectedRecords)); (BulkDeleteCommand as Command)?.ChangeCanExecute(); (BulkEditCommand as Command)?.ChangeCanExecute(); } }
    }

    public bool HasSelectedRecords => SelectedRecords.Count > 0;

    public MonthlyAttendanceTotals MonthlySummaryTotals
    {
        get => _monthlySummaryTotals;
        set { if (_monthlySummaryTotals != value) { _monthlySummaryTotals = value; OnPropertyChanged(); } }
    }

    public DateTime SummaryMonth
    {
        get => _summaryMonth;
        set
        {
            var normalized = new DateTime(value.Year, value.Month, 1);
            if (_summaryMonth != normalized)
            {
                _summaryMonth = normalized;
                OnPropertyChanged();
                _ = LoadMonthlySummaryAsync();
            }
        }
    }

    public MonthlyAttendanceSummary SelectedMonthlySummary
    {
        get => _selectedMonthlySummary;
        set
        {
            if (_selectedMonthlySummary != value)
            {
                _selectedMonthlySummary = value;
                OnPropertyChanged();
                UpdateCalendarForSelection();
                if (_selectedMonthlySummary != null)
                {
                    SelectedExpenseUser = Users.FirstOrDefault(u => u.Id == _selectedMonthlySummary.UserId) ?? SelectedExpenseUser;
                    _ = LoadEmployeeExpensesAsync();
                }
                (ExportSelectedEmployeePayrollPdfCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public string NewUserName
    {
        get => _newUserName;
        set { if (_newUserName != value) { _newUserName = value; OnPropertyChanged(); } }
    }

    public string NewUserUsername
    {
        get => _newUserUsername;
        set { if (_newUserUsername != value) { _newUserUsername = value; OnPropertyChanged(); } }
    }

    public string NewUserPassword
    {
        get => _newUserPassword;
        set { if (_newUserPassword != value) { _newUserPassword = value; OnPropertyChanged(); } }
    }

    public bool NewUserIsAdmin
    {
        get => _newUserIsAdmin;
        set { if (_newUserIsAdmin != value) { _newUserIsAdmin = value; OnPropertyChanged(); } }
    }

    public string NewUserSalary
    {
        get => _newUserSalary;
        set { if (_newUserSalary != value) { _newUserSalary = value; OnPropertyChanged(); } }
    }

    public string NewProductName
    {
        get => _newProductName;
        set { if (_newProductName != value) { _newProductName = value; OnPropertyChanged(); } }
    }

    public string NewProductEmoji
    {
        get => _newProductEmoji;
        set { if (_newProductEmoji != value) { _newProductEmoji = value; OnPropertyChanged(); } }
    }

    public string NewProductBarcode
    {
        get => _newProductBarcode;
        set { if (_newProductBarcode != value) { _newProductBarcode = value; OnPropertyChanged(); } }
    }

    public string NewProductPrice
    {
        get => _newProductPrice;
        set { if (_newProductPrice != value) { _newProductPrice = value; OnPropertyChanged(); } }
    }

    public string NewProductStock
    {
        get => _newProductStock;
        set { if (_newProductStock != value) { _newProductStock = value; OnPropertyChanged(); } }
    }

    public bool NewProductIsWeight
    {
        get => _newProductIsWeight;
        set { if (_newProductIsWeight != value) { _newProductIsWeight = value; OnPropertyChanged(); } }
    }

    public string NewProductCategory
    {
        get => _newProductCategory;
        set { if (_newProductCategory != value) { _newProductCategory = value; OnPropertyChanged(); } }
    }

    public Product SelectedProduct
    {
        get => _selectedProduct;
        set
        {
            if (_selectedProduct != value)
            {
                _selectedProduct = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditingProduct));
            }
        }
    }

    public bool IsEditingProduct => SelectedProduct != null;

    public string EditProductName
    {
        get => _editProductName;
        set { if (_editProductName != value) { _editProductName = value; OnPropertyChanged(); } }
    }

    public string EditProductEmoji
    {
        get => _editProductEmoji;
        set { if (_editProductEmoji != value) { _editProductEmoji = value; OnPropertyChanged(); } }
    }

    public string EditProductCategory
    {
        get => _editProductCategory;
        set { if (_editProductCategory != value) { _editProductCategory = value; OnPropertyChanged(); } }
    }

    public string EditProductPrice
    {
        get => _editProductPrice;
        set { if (_editProductPrice != value) { _editProductPrice = value; OnPropertyChanged(); } }
    }

    public string ProductSearchText
    {
        get => _productSearchText;
        set
        {
            if (_productSearchText != value)
            {
                _productSearchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }
    }

    public async Task InitializeAsync()
    {
        if (!IsAuthorized)
        {
            StatusMessage = _localizationService.GetString("AdminAccessRequired");
            IsErrorStatus = true;
            return;
        }

        await LoadUsersAsync();
        await LoadProductsAsync();
        await LoadReportsAsync();
        await LoadAttendanceAsync();
        await LoadEmployeeExpensesAsync();
    }

    public void RefreshLocalizedProperties()
    {
        OnPropertyChanged(nameof(AverageOrderValueDisplay));
        OnPropertyChanged(nameof(ReportStatusText));
        OnPropertyChanged(nameof(ReportStatusTextColor));
    }

    private async Task LoadUsersAsync()
    {
        IsBusy = true;
        try
        {
            var users = await _databaseService.GetUsersAsync();
            Users.Clear();
            
            // Filter out Developer users - they should not appear in the list
            if (users != null)
            {
                foreach (var user in users.Where(u => u != null && !u.IsDeveloper))
                    Users.Add(user);
            }
            
            if (SelectedExpenseUser == null)
            {
                SelectedExpenseUser = Users?.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadUsersAsync: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadProductsAsync()
    {
        IsBusy = true;
        try
        {
            var products = await _databaseService.GetProductsAsync();
            Products.Clear();
            foreach (var product in products)
                Products.Add(product);
            
            FilterProducts();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FilterProducts()
    {
        FilteredProducts.Clear();
        
        // If no search text, show all products
        if (string.IsNullOrWhiteSpace(ProductSearchText))
        {
            foreach (var product in Products)
                FilteredProducts.Add(product);
            return;
        }

        var searchText = ProductSearchText.Trim();
        
        // Try to parse as ID (numeric)
        if (int.TryParse(searchText, out int searchId))
        {
            var productById = Products.FirstOrDefault(p => p.Id == searchId);
            if (productById != null)
            {
                FilteredProducts.Add(productById);
                return;
            }
        }

        // Search by barcode (exact match first)
        var productByBarcode = Products.FirstOrDefault(p => 
            !string.IsNullOrEmpty(p.Barcode) && 
            p.Barcode.Equals(searchText, StringComparison.OrdinalIgnoreCase));
        
        if (productByBarcode != null)
        {
            FilteredProducts.Add(productByBarcode);
            return;
        }

        // Search by barcode or name (contains)
        var searchLower = searchText.ToLowerInvariant();
        foreach (var product in Products)
        {
            bool matches = false;
            
            // Check barcode (contains)
            if (!string.IsNullOrEmpty(product.Barcode) && 
                product.Barcode.ToLowerInvariant().Contains(searchLower))
            {
                matches = true;
            }
            // Also check name as fallback
            else if (product.Name.ToLowerInvariant().Contains(searchLower))
            {
                matches = true;
            }
            
            if (matches)
            {
                FilteredProducts.Add(product);
            }
        }
    }

    private async Task LoadReportsAsync()
    {
        IsBusy = true;
        try
        {
            var orders = await _databaseService.GetOrdersAsync();
            if (orders == null)
                orders = new List<Order>();
                
            TotalOrders = orders.Count;
            TotalSales = orders.Sum(o => o.Total);
            AverageOrderValue = TotalOrders > 0 ? TotalSales / TotalOrders : 0m;

            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            Last7DaysSales = orders
                .Where(o => o.OrderDate >= sevenDaysAgo)
                .Sum(o => o.Total);

            RecentOrders.Clear();
            foreach (var order in orders.Take(5))
                RecentOrders.Add(order);

            var orderItems = await _databaseService.GetAllOrderItemsAsync();
            if (orderItems == null)
                orderItems = new List<OrderItem>();
                
            TotalItemsSold = orderItems.Sum(oi => oi.Quantity);

            var groupedProducts = orderItems
                .GroupBy(oi => new { oi.ProductId, oi.Name, oi.Emoji, oi.IsSoldByWeight })
                .Select(g => new ProductReportItem
                {
                    Name = g.Key.Name,
                    Emoji = g.Key.Emoji,
                    Quantity = g.Sum(x => x.Quantity),
                    IsSoldByWeight = g.Key.IsSoldByWeight,
                    TotalSales = g.Sum(x => x.ItemTotal)
                })
                .OrderByDescending(p => p.TotalSales)
                .ToList();

            TopProducts.Clear();
            foreach (var product in groupedProducts.Take(5))
                TopProducts.Add(product);

            if (groupedProducts != null && groupedProducts.Any())
            {
                var top = groupedProducts.FirstOrDefault();
                if (top != null)
                {
                    TopProductName = $"{top.Emoji} {top.Name}";
                    TopProductDetails = $"{top.Quantity:F2} {top.UnitLabel} • ${top.TotalSales:F2}";
                }
                else
                {
                    TopProductName = "No sales yet";
                    TopProductDetails = "Add items to see insights";
                }
            }
            else
            {
                TopProductName = "No sales yet";
                TopProductDetails = "Add items to see insights";
            }

            OnPropertyChanged(nameof(HasReportData));
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LoadAttendanceAsync()
    {
        IsBusy = true;
        try
        {
            var start = DateTime.Today.AddDays(-30);
            var records = await _databaseService.GetAttendanceRecordsAsync(start, DateTime.Today);
            if (records == null)
                records = new List<AttendanceRecord>();
                
            AttendanceRecords.Clear();
            foreach (var record in records)
            {
                if (record != null)
                    AttendanceRecords.Add(record);
            }

            AttendanceSummary = new AttendanceSummary
            {
                PresentCount = records.Count(r => r != null && r.IsPresent),
                AbsentCount = records.Count(r => r != null && !r.IsPresent),
                OvertimeCount = records.Count(r => r != null && r.OvertimeHours > 0m),
                TotalRegularHours = records.Where(r => r != null).Sum(r => r.RegularHours),
                TotalOvertimeHours = records.Where(r => r != null).Sum(r => r.OvertimeHours),
                TotalPayroll = records.Where(r => r != null).Sum(r => r.DailyPay),
                LastUpdated = DateTime.Now
            };

            // Calculate enhanced statistics
            await CalculateEnhancedStatisticsAsync(records);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadAttendanceAsync: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            ShowStatus("Error loading attendance data", true);
        }
        finally
        {
            IsBusy = false;
        }

        try
        {
            await LoadMonthlySummaryAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadMonthlySummaryAsync from LoadAttendanceAsync: {ex}");
        }
    }

    private async Task CalculateEnhancedStatisticsAsync(List<AttendanceRecord> records)
    {
        try
        {
            if (SelectedAttendanceUser == null || records == null || !records.Any())
            {
                AverageHoursPerDay = 0m;
                ConsecutiveWorkingDays = 0;
                MonthlyComparison = new MonthlyComparisonData();
                EmployeeComparisonData = new List<EmployeeComparisonItem>();
                return;
            }

            var userRecords = records
                .Where(r => r != null && r.UserId == SelectedAttendanceUser.Id && r.IsPresent)
                .OrderBy(r => r.Date)
                .ToList();
            
            // Calculate average hours per day
            if (userRecords != null && userRecords.Any())
            {
                var totalDays = userRecords.Count;
                var totalHours = userRecords.Where(r => r != null).Sum(r => r.RegularHours + r.OvertimeHours);
                AverageHoursPerDay = totalDays > 0 ? totalHours / totalDays : 0m;
            }
            else
            {
                AverageHoursPerDay = 0m;
            }

            // Calculate consecutive working days
            ConsecutiveWorkingDays = CalculateConsecutiveWorkingDays(userRecords);

            // Calculate monthly comparison
            var currentMonth = DateTime.Today;
            var previousMonth = currentMonth.AddMonths(-1);
            var currentMonthRecords = userRecords
                .Where(r => r != null && r.Date.Year == currentMonth.Year && r.Date.Month == currentMonth.Month)
                .ToList();
            var previousMonthRecords = records
                .Where(r => r != null && r.UserId == SelectedAttendanceUser.Id && r.IsPresent && r.Date.Year == previousMonth.Year && r.Date.Month == previousMonth.Month)
                .ToList();

            MonthlyComparison = new MonthlyComparisonData
            {
                CurrentMonthDays = currentMonthRecords.Count,
                PreviousMonthDays = previousMonthRecords.Count,
                CurrentMonthHours = currentMonthRecords.Where(r => r != null).Sum(r => r.RegularHours + r.OvertimeHours),
                PreviousMonthHours = previousMonthRecords.Where(r => r != null).Sum(r => r.RegularHours + r.OvertimeHours),
                CurrentMonthPayroll = currentMonthRecords.Where(r => r != null).Sum(r => r.DailyPay),
                PreviousMonthPayroll = previousMonthRecords.Where(r => r != null).Sum(r => r.DailyPay)
            };

            // Calculate employee comparison
            await CalculateEmployeeComparisonAsync(records);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating enhanced statistics: {ex}");
        }
    }

    private int CalculateConsecutiveWorkingDays(List<AttendanceRecord> userRecords)
    {
        try
        {
            if (userRecords == null || !userRecords.Any())
                return 0;

            var sortedRecords = userRecords
                .Where(r => r != null)
                .OrderByDescending(r => r.Date)
                .ToList();
            
            if (!sortedRecords.Any())
                return 0;

            int maxConsecutive = 0;
            int currentConsecutive = 0;
            DateTime? lastDate = null;

            foreach (var record in sortedRecords)
            {
                if (record == null) continue;
                
                if (lastDate == null)
                {
                    currentConsecutive = 1;
                    lastDate = record.Date;
                }
                else
                {
                    var daysDiff = (lastDate.Value - record.Date).Days;
                    if (daysDiff == 1)
                    {
                        currentConsecutive++;
                    }
                    else
                    {
                        maxConsecutive = Math.Max(maxConsecutive, currentConsecutive);
                        currentConsecutive = 1;
                    }
                    lastDate = record.Date;
                }
            }

            return Math.Max(maxConsecutive, currentConsecutive);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CalculateConsecutiveWorkingDays: {ex}");
            return 0;
        }
    }

    private async Task CalculateEmployeeComparisonAsync(List<AttendanceRecord> allRecords)
    {
        try
        {
            var comparisonData = new List<EmployeeComparisonItem>();
            var users = await _databaseService.GetUsersAsync();
            
            if (users == null || !users.Any())
            {
                EmployeeComparisonData = new List<EmployeeComparisonItem>();
                return;
            }

            foreach (var user in users.Where(u => u != null && !u.IsDeveloper))
            {
                var userRecords = allRecords
                    .Where(r => r != null && r.UserId == user.Id && r.IsPresent)
                    .ToList();
                var totalDays = userRecords.Count;
                var totalHours = userRecords.Where(r => r != null).Sum(r => r.RegularHours + r.OvertimeHours);
                var totalPayroll = userRecords.Where(r => r != null).Sum(r => r.DailyPay);
                
                var userTotalRecords = allRecords.Count(r => r != null && r.UserId == user.Id);
                var attendanceRate = userTotalRecords > 0 
                    ? (decimal)totalDays / userTotalRecords * 100m 
                    : 0m;

                comparisonData.Add(new EmployeeComparisonItem
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    DaysWorked = totalDays,
                    TotalHours = totalHours,
                    AverageHoursPerDay = totalDays > 0 ? totalHours / totalDays : 0m,
                    AttendanceRate = attendanceRate,
                    TotalPayroll = totalPayroll
                });
            }

            EmployeeComparisonData = comparisonData.OrderByDescending(e => e.DaysWorked).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error calculating employee comparison: {ex}");
            EmployeeComparisonData = new List<EmployeeComparisonItem>();
        }
    }

    private async Task ShowOrderDetailsAsync(Order order)
    {
        if (order == null)
            return;

        var items = await _databaseService.GetOrderItemsAsync(order.Id);
        if (items == null || items.Count == 0)
        {
            var orderTitle = string.Format(_localizationService.GetString("Order"), order.Id);
            var noItems = _localizationService.GetString("NoItemsFound");
            var ok = _localizationService.GetString("OK");
            await Application.Current.MainPage.DisplayAlert(orderTitle, noItems, ok);
            return;
        }

        var lines = items.Select(i =>
            $"{i.Emoji} {i.Name}\n  {i.Quantity:F2} {i.UnitLabel} × ${i.Price:F2} = ${i.ItemTotal:F2}");

        var body = string.Join("\n\n", lines);

        var orderTitle2 = string.Format(_localizationService.GetString("Order"), order.Id);
        var cashier = string.Format(_localizationService.GetString("Cashier"), order.UserName);
        var itemsCount = string.Format(_localizationService.GetString("Items"), order.ItemCount);
        var header = $"{orderTitle2} • {order.OrderDate:yyyy-MM-dd HH:mm}\n" +
                     $"{cashier}\n" +
                     $"{itemsCount}   Total: ${order.Total:F2}\n\n";

        var orderDetails = _localizationService.GetString("OrderDetails");
        var close = _localizationService.GetString("Close");
        await Application.Current.MainPage.DisplayAlert(orderDetails, header + body, close);
    }

    private async Task AddAttendanceAsync()
    {
        if (SelectedAttendanceUser == null)
        {
            ShowStatus(_localizationService.GetString("PleaseSelectEmployee"), true);
            return;
        }

        if (AttendanceDate.Date > DateTime.Today)
        {
            ShowStatus(_localizationService.GetString("FutureDatesNotAllowed"), true);
            return;
        }

        var existingRecord = await _databaseService.GetAttendanceRecordAsync(
            SelectedAttendanceUser.Id,
            AttendanceDate.Date);

        if (existingRecord != null)
        {
            ShowStatus(_localizationService.GetString("AttendanceExists"), true);
            return;
        }

        var calculation = await CalculateAttendanceForEntryAsync();
        if (!calculation.IsValid)
        {
            ShowStatus($"⚠️ {calculation.ValidationMessage}", true);
            return;
        }

        var record = new AttendanceRecord
        {
            UserId = SelectedAttendanceUser.Id,
            UserName = SelectedAttendanceUser.Name,
            Date = AttendanceDate.Date,
            Status = SelectedAttendanceStatus,
            IsPresent = calculation.IsPresent,
            RegularHours = calculation.RegularHours,
            OvertimeHours = calculation.OvertimeHours,
            DailyPay = calculation.DailyPay,
            CheckInTime = calculation.CheckIn,
            CheckOutTime = calculation.CheckOut,
            Notes = AttendanceNotes?.Trim() ?? "",
            AbsencePermissionType = calculation.AbsencePermissionType
        };

        IsBusy = true;
        try
        {
            await _databaseService.SaveAttendanceRecordAsync(record);
            var recordedMsg = _localizationService.GetString("RecordedAttendance");
            ShowStatus(string.Format(recordedMsg, SelectedAttendanceStatus, SelectedAttendanceUser.Name), false);

            AttendanceNotes = "";
            AttendanceCheckInTime = new TimeSpan(8, 0, 0);
            AttendanceCheckOutTime = new TimeSpan(16, 0, 0);

            await LoadAttendanceAsync();
            _ = UpdateAttendancePreviewAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenAttendancePage()
    {
        System.Diagnostics.Debug.WriteLine($"OpenAttendancePage called. CanUseAttendanceTracker: {CanUseAttendanceTracker}");
        System.Diagnostics.Debug.WriteLine($"AuthService.CanUseAttendanceTracker: {_authService.CanUseAttendanceTracker}");
        System.Diagnostics.Debug.WriteLine($"FeatureFlags.IsAttendanceTrackerEnabled: {Services.FeatureFlags.IsAttendanceTrackerEnabled}");
        
        if (!CanUseAttendanceTracker)
        {
            var reason = !_authService.CanUseAttendanceTracker 
                ? "You don't have permission to use the attendance tracker." 
                : "Attendance tracker is currently disabled.";
            ShowStatus(reason, true);
            return;
        }
        
        try
        {
            System.Diagnostics.Debug.WriteLine("Creating AttendancePage...");
            // Create the page directly with required dependencies
            var attendancePage = new Views.AttendancePage(this, _localizationService);
            System.Diagnostics.Debug.WriteLine("AttendancePage created successfully");
            
            // Navigate first, then load data (page will load data in OnAppearing)
            if (Application.Current?.MainPage != null)
            {
                System.Diagnostics.Debug.WriteLine("Navigating via Application.Current.MainPage.Navigation");
                await Application.Current.MainPage.Navigation.PushAsync(attendancePage);
                System.Diagnostics.Debug.WriteLine("Navigation completed");
                
                // Load data after navigation to avoid blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadAttendanceAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading attendance data after navigation: {ex}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                });
            }
            else if (Shell.Current?.Navigation != null)
            {
                System.Diagnostics.Debug.WriteLine("Navigating via Shell.Current.Navigation");
                await Shell.Current.Navigation.PushAsync(attendancePage);
                System.Diagnostics.Debug.WriteLine("Navigation completed");
                
                // Load data after navigation to avoid blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await LoadAttendanceAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading attendance data after navigation: {ex}");
                        System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: No navigation available");
                ShowStatus("Cannot navigate. Please try again.", true);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening attendance page: {ex}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException}");
            
            var errorMsg = $"Error: {ex.Message}";
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", errorMsg, "OK");
            }
            ShowStatus("Error opening attendance page. Please try again.", true);
        }
    }

    private async Task LoadEmployeeExpensesAsync()
    {
        if (SelectedExpenseUser == null) return;
        
        try
        {
            var monthStart = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var items = await _databaseService.GetEmployeeExpensesAsync(SelectedExpenseUser.Id, monthStart, monthEnd);
            
            EmployeeExpenses.Clear();
            if (items != null)
            {
                foreach (var it in items.Where(e => e != null)) 
                    EmployeeExpenses.Add(it);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LoadEmployeeExpensesAsync: {ex}");
            EmployeeExpenses.Clear();
        }
    }

    private async Task AddExpenseAsync()
    {
        if (SelectedExpenseUser == null)
        {
            ShowStatus("Select employee for expense.", true);
            return;
        }
        if (!decimal.TryParse(ExpenseAmount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ShowStatus("Enter valid amount.", true);
            return;
        }
        var expense = new EmployeeExpense
        {
            UserId = SelectedExpenseUser.Id,
            Amount = amount,
            Category = string.IsNullOrWhiteSpace(ExpenseCategory) ? "General" : ExpenseCategory.Trim(),
            Notes = ExpenseNotes?.Trim() ?? "",
            ExpenseDate = ExpenseDate
        };
        await _databaseService.CreateEmployeeExpenseAsync(expense);
        await LoadEmployeeExpensesAsync();
        ShowStatus("Expense added.", false);
        ExpenseAmount = "";
        ExpenseCategory = "";
        ExpenseNotes = "";
        ExpenseDate = DateTime.Today;
    }

    private async Task DeleteExpenseAsync(EmployeeExpense expense)
    {
        if (expense == null) return;
        await _databaseService.DeleteEmployeeExpenseAsync(expense);
        await LoadEmployeeExpensesAsync();
        ShowStatus("Expense removed.", false);
    }

    private async Task AddUserAsync()
    {
        if (!CanManageUsers)
        {
            ShowStatus("You don't have permission to manage users.", true);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewUserName) ||
            string.IsNullOrWhiteSpace(NewUserUsername) ||
            string.IsNullOrWhiteSpace(NewUserPassword))
        {
            ShowStatus(_localizationService.GetString("PleaseFillAllUserFields"), true);
            return;
        }

        if (NewUserPassword.Length < 4)
        {
            ShowStatus(_localizationService.GetString("PasswordMinLength"), true);
            return;
        }

        if (!decimal.TryParse(NewUserSalary, NumberStyles.Number, CultureInfo.InvariantCulture, out var salary) || salary < 0)
        {
            ShowStatus(_localizationService.GetString("EnterValidSalary"), true);
            return;
        }

        IsBusy = true;
        try
        {
            if (await _databaseService.UsernameExistsAsync(NewUserUsername.Trim()))
            {
                ShowStatus(_localizationService.GetString("UsernameExists"), true);
                return;
            }

            // Determine role based on selection
            string selectedRole = "User"; // Default
            if (NewUserIsAdmin)
            {
                selectedRole = "Admin";
            }
            // Note: Role picker would be better, but for now using the boolean
            
            var user = new User
            {
                Name = NewUserName.Trim(),
                Username = NewUserUsername.Trim(),
                Password = PasswordHelper.HashPassword(NewUserPassword.Trim()),
                Role = selectedRole,
                MonthlySalary = salary,
                OvertimeMultiplier = 1.5m // Default OT multiplier
            };

            await _databaseService.CreateUserAsync(user);
            var userType = _localizationService.GetString(NewUserIsAdmin ? "Admin" : "User");
            var createdMsg = _localizationService.GetString("CreatedUser");
            ShowStatus(string.Format(createdMsg, userType, user.Name), false);

            await LoadUsersAsync();

            NewUserName = "";
            NewUserUsername = "";
            NewUserPassword = "";
            NewUserIsAdmin = false;
            NewUserSalary = "0";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task AddProductAsync()
    {
        if (!IsAuthorized)
        {
            ShowStatus(_localizationService.GetString("OnlyAdminsCanAddProducts"), true);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewProductName) ||
            string.IsNullOrWhiteSpace(NewProductEmoji) ||
            string.IsNullOrWhiteSpace(NewProductPrice) ||
            string.IsNullOrWhiteSpace(NewProductStock))
        {
            ShowStatus(_localizationService.GetString("PleaseFillAllProductFields"), true);
            return;
        }

        if (!decimal.TryParse(NewProductPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            ShowStatus(_localizationService.GetString("EnterValidPrice"), true);
            return;
        }

        if (!decimal.TryParse(NewProductStock, NumberStyles.Number, CultureInfo.InvariantCulture, out var stock) || stock < 0)
        {
            ShowStatus(_localizationService.GetString("EnterValidStock"), true);
            return;
        }

        IsBusy = true;
        try
        {
            if (!string.IsNullOrWhiteSpace(NewProductBarcode) &&
                await _databaseService.ProductBarcodeExistsAsync(NewProductBarcode.Trim()))
            {
                ShowStatus(_localizationService.GetString("BarcodeExists"), true);
                return;
            }

            var product = new Product
            {
                Name = NewProductName.Trim(),
                Emoji = NewProductEmoji.Trim(),
                Barcode = NewProductBarcode?.Trim() ?? "",
                Price = price,
                Stock = stock,
                IsSoldByWeight = NewProductIsWeight,
                Category = string.IsNullOrWhiteSpace(NewProductCategory) ? "All" : NewProductCategory.Trim()
            };

            await _databaseService.SaveProductAsync(product);
            var addedMsg = _localizationService.GetString("AddedProduct");
            ShowStatus(string.Format(addedMsg, product.Name), false);

            await LoadProductsAsync();

            NewProductName = "";
            NewProductEmoji = "🍬";
            NewProductBarcode = "";
            NewProductPrice = "";
            NewProductStock = "";
            NewProductIsWeight = false;
            NewProductCategory = "All";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EditProductAsync(Product product)
    {
        if (product == null) return;

        if (!IsAuthorized)
        {
            ShowStatus(_localizationService.GetString("OnlyAdminsCanAddProducts"), true);
            return;
        }

        SelectedProduct = product;
        EditProductName = product.Name;
        EditProductEmoji = product.Emoji;
        EditProductCategory = product.Category ?? "All";
        EditProductPrice = product.Price.ToString(CultureInfo.InvariantCulture);
    }

    private async Task UpdateProductAsync()
    {
        if (SelectedProduct == null) return;

        if (!IsAuthorized)
        {
            ShowStatus(_localizationService.GetString("OnlyAdminsCanAddProducts"), true);
            return;
        }

        if (string.IsNullOrWhiteSpace(EditProductName) ||
            string.IsNullOrWhiteSpace(EditProductEmoji) ||
            string.IsNullOrWhiteSpace(EditProductPrice))
        {
            ShowStatus(_localizationService.GetString("PleaseFillAllProductFields"), true);
            return;
        }

        if (!decimal.TryParse(EditProductPrice, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            ShowStatus(_localizationService.GetString("EnterValidPrice"), true);
            return;
        }

        IsBusy = true;
        try
        {
            var productName = EditProductName.Trim();
            var productId = SelectedProduct.Id;
            
            SelectedProduct.Name = productName;
            SelectedProduct.Emoji = EditProductEmoji.Trim();
            SelectedProduct.Category = string.IsNullOrWhiteSpace(EditProductCategory) ? "All" : EditProductCategory.Trim();
            SelectedProduct.Price = price;

            // Save the product
            await _databaseService.SaveProductAsync(SelectedProduct);
            
            // Always reload products to ensure UI is updated
            await LoadProductsAsync();
            
            // Show success message
            var updatedMsg = _localizationService.GetString("AddedProduct");
            if (updatedMsg.Contains("Added"))
            {
                updatedMsg = updatedMsg.Replace("Added", "Updated");
            }
            else
            {
                updatedMsg = $"Updated {productName}";
            }
            ShowStatus(string.Format(updatedMsg, productName), false);

            // Clear search and cancel edit
            ProductSearchText = "";
            CancelEditProduct();
        }
        catch (Exception ex)
        {
            ShowStatus($"Error updating product: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CancelEditProduct()
    {
        SelectedProduct = null;
        EditProductName = "";
        EditProductEmoji = "";
        EditProductCategory = "";
        EditProductPrice = "";
    }

    private async Task ToggleUserStatusAsync(User user)
    {
        if (user == null)
            return;

        if (!IsAuthorized)
        {
            ShowStatus(_localizationService.GetString("OnlyAdminsCanChangeStatus"), true);
            return;
        }

        if (_authService.CurrentUser?.Id == user.Id && user.IsEnabled)
        {
            ShowStatus(_localizationService.GetString("CannotDisableCurrentAccount"), true);
            return;
        }

        IsBusy = true;
        try
        {
            user.IsEnabled = !user.IsEnabled;
            await _databaseService.UpdateUserAsync(user);
            var action = user.IsEnabled ? _localizationService.GetString("Enabled") : _localizationService.GetString("Disabled2");
            var statusMsg = _localizationService.GetString("UserStatusChanged");
            ShowStatus(string.Format(statusMsg, action, user.Name), false);
            await LoadUsersAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowStatus(string message, bool isError)
    {
        StatusMessage = message;
        IsErrorStatus = isError;
    }

    private bool StatusRequiresTimes(string status) =>
        string.Equals(status, "Present", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Reset", StringComparison.OrdinalIgnoreCase);
    
    /// <summary>
    /// Determines if a date falls on a reset day (every 13 days starting from day 7).
    /// Reset days: 7, 13 (7+6), 19 (13+6), 25 (19+6), and last day of month.
    /// Reset days are paid at normal rate (not OT).
    /// </summary>
    private bool IsResetDay(DateTime date)
    {
        int dayOfMonth = date.Day;
        // Reset days: 7, 13, 19, 25, and last day of month
        if (dayOfMonth == 7 || dayOfMonth == 13 || dayOfMonth == 19 || dayOfMonth == 25)
            return true;
        
        // Last day of month (could be 28, 29, 30, or 31)
        int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        if (dayOfMonth == daysInMonth)
            return true;
        
        return false;
    }
    
    /// <summary>
    /// Determines if a date falls on an OT day (days after reset days: 8, 14, 20, 26).
    /// These days are counted as OT if worked.
    /// </summary>
    private bool IsOvertimeDay(DateTime date)
    {
        int dayOfMonth = date.Day;
        // OT days: 8 (after reset day 7), 14 (after reset day 13), 20 (after reset day 19), 26 (after reset day 25)
        return dayOfMonth == 8 || dayOfMonth == 14 || dayOfMonth == 20 || dayOfMonth == 26;
    }

    

    /// <summary>
    /// Checks if a specific date was a reset day by counting working days before it.
    /// Uses iterative approach to avoid recursion issues.
    /// </summary>

    private async Task<AttendanceCalculationResult> CalculateAttendanceForEntryAsync(bool validateOnly = false)
    {
        if (SelectedAttendanceUser == null)
            return AttendanceCalculationResult.Invalid("Select an employee.");

        try
        {
            var status = SelectedAttendanceStatus ?? "Present";
            DateTime? checkIn = null;
            DateTime? checkOut = null;
            if (StatusRequiresTimes(status))
            {
                checkIn = AttendanceDate.Date + AttendanceCheckInTime;
                checkOut = AttendanceDate.Date + AttendanceCheckOutTime;
            }

            var calc = await _attendanceRulesService.CalculateAsync(SelectedAttendanceUser, AttendanceDate, status, checkIn, checkOut);
            if (calc.NeedsSalaryInput)
            {
                calc.ValidationMessage = "Set monthly salary to calculate pay.";
                if (!validateOnly)
                {
                    calc.IsValid = false;
                    return calc;
                }
            }
            calc.IsValid = true;
            return calc;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in CalculateAttendanceForEntryAsync: {ex}");
            return AttendanceCalculationResult.Invalid($"Error calculating attendance: {ex.Message}");
        }
    }
    
    private async Task UpdateAttendancePreviewAsync()
    {
        if (SelectedAttendanceUser == null)
        {
            AttendancePreview = "Select an employee to preview pay.";
            return;
        }

        DateTime? checkIn = null;
        DateTime? checkOut = null;
        var status = SelectedAttendanceStatus ?? "Present";
        if (StatusRequiresTimes(status))
        {
            checkIn = AttendanceDate.Date + AttendanceCheckInTime;
            checkOut = AttendanceDate.Date + AttendanceCheckOutTime;
        }

        var calc = await _attendanceRulesService.CalculateAsync(SelectedAttendanceUser, AttendanceDate, status, checkIn, checkOut);
        if (!calc.IsValid || !string.IsNullOrWhiteSpace(calc.ValidationMessage))
        {
            AttendancePreview = calc.ValidationMessage;
            return;
        }

        AttendancePreview = $"Regular: {calc.RegularHours:F2}h | OT: {calc.OvertimeHours:F2}h | Pay ${calc.DailyPay:F2}";
    }

    private async Task ExportPayrollPdfAsync()
    {
        System.Diagnostics.Debug.WriteLine("ExportPayrollPdfAsync called");
        
        if (MonthlyAttendanceSummaries == null || !MonthlyAttendanceSummaries.Any())
        {
            ShowStatus("No attendance data to export.", true);
            return;
        }

        IsBusy = true;
        try
        {
            System.Diagnostics.Debug.WriteLine($"Exporting PDF for {MonthlyAttendanceSummaries.Count} users");
            var summaries = MonthlyAttendanceSummaries.ToList();
            var pdfPath = await _pdfService.GeneratePayrollPdfAsync(summaries, SummaryMonth, MonthlySummaryTotals);
            System.Diagnostics.Debug.WriteLine($"PDF generated at: {pdfPath}");

            if (string.IsNullOrEmpty(pdfPath))
            {
                ShowStatus("Failed to generate PDF.", true);
                return;
            }

            // Open the PDF file
            try
            {
                if (System.IO.File.Exists(pdfPath))
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(pdfPath)
                    });
                    ShowStatus("PDF exported and opened successfully.", false);
                }
                else
                {
                    ShowStatus($"PDF file was not created at: {pdfPath}", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"PDF generated at {pdfPath}. Error opening: {ex.Message}", true);
                System.Diagnostics.Debug.WriteLine($"PDF Export Error: {ex}");
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Error exporting PDF: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportSelectedEmployeePayrollPdfAsync()
    {
        if (SelectedMonthlySummary == null)
        {
            ShowStatus("Select an employee in monthly summary first.", true);
            return;
        }
        
        try
        {
            var monthStart = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var expenses = await _databaseService.GetEmployeeExpensesAsync(SelectedMonthlySummary.UserId, monthStart, monthEnd);
            
            if (expenses == null)
                expenses = new List<EmployeeExpense>();
                
            var pdfPath = await _pdfService.GenerateEmployeePayrollPdfAsync(SelectedMonthlySummary, SummaryMonth, expenses);
            if (string.IsNullOrEmpty(pdfPath))
            {
                ShowStatus("Failed to generate PDF.", true);
                return;
            }
            try
            {
                if (System.IO.File.Exists(pdfPath))
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(pdfPath)
                    });
                    ShowStatus("Employee payroll PDF exported and opened.", false);
                }
                else
                {
                    ShowStatus($"PDF file was not created at: {pdfPath}", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"PDF generated at {pdfPath}. Error opening: {ex.Message}", true);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ExportSelectedEmployeePayrollPdfAsync: {ex}");
            ShowStatus($"Error generating employee payroll PDF: {ex.Message}", true);
        }
    }

    private async Task DeleteAttendanceRecordAsync(AttendanceRecord record)
    {
        if (record == null) return;
        IsBusy = true;
        try
        {
            await _databaseService.DeleteAttendanceRecordAsync(record);
            await LoadAttendanceAsync();
            ShowStatus("Deleted attendance record.", false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EditAttendanceRecordAsync(AttendanceRecord record)
    {
        if (record == null) return;
        _editingAttendanceRecord = record;
        IsEditingAttendance = true;
        SelectedAttendanceUser = Users?.FirstOrDefault(u => u.Id == record.UserId) ?? SelectedAttendanceUser;
        AttendanceDate = record.Date;
        SelectedAttendanceStatus = record.Status;
        AttendanceNotes = record.Notes;
        AttendanceCheckInTime = record.CheckInTime.HasValue ? record.CheckInTime.Value.TimeOfDay : new TimeSpan(8,0,0);
        AttendanceCheckOutTime = record.CheckOutTime.HasValue ? record.CheckOutTime.Value.TimeOfDay : new TimeSpan(16,0,0);
        _ = UpdateAttendancePreviewAsync();
    }

    private async Task UpdateAttendanceRecordAsync()
    {
        if (_editingAttendanceRecord == null) return;
        var calc = await CalculateAttendanceForEntryAsync();
        if (!calc.IsValid)
        {
            ShowStatus($"⚠️ {calc.ValidationMessage}", true);
            return;
        }
        var record = _editingAttendanceRecord;
        record.UserId = SelectedAttendanceUser.Id;
        record.UserName = SelectedAttendanceUser.Name;
        record.Date = AttendanceDate.Date;
        record.Status = SelectedAttendanceStatus;
        record.IsPresent = calc.IsPresent;
        record.RegularHours = calc.RegularHours;
        record.OvertimeHours = calc.OvertimeHours;
        record.DailyPay = calc.DailyPay;
        record.CheckInTime = calc.CheckIn;
        record.CheckOutTime = calc.CheckOut;
        record.Notes = AttendanceNotes?.Trim() ?? "";
        record.AbsencePermissionType = calc.AbsencePermissionType;

        IsBusy = true;
        try
        {
            await _databaseService.SaveAttendanceRecordAsync(record);
            ShowStatus("Updated attendance record.", false);
            IsEditingAttendance = false;
            _editingAttendanceRecord = null;
            await LoadAttendanceAsync();
            _ = UpdateAttendancePreviewAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateCalendarForSelection()
    {
        AttendanceCalendarDays.Clear();
        if (SelectedMonthlySummary == null)
            return;

        var monthStart = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var today = DateTime.Today;

        if (_currentMonthRecords == null)
            _currentMonthRecords = new List<AttendanceRecord>();

        var userRecords = new Dictionary<DateTime, AttendanceRecord>();
        if (_currentMonthRecords != null && SelectedMonthlySummary != null)
        {
            try
            {
                var filtered = _currentMonthRecords
                    .Where(r => r != null && r.UserId == SelectedMonthlySummary.UserId)
                    .ToList();
                
                if (filtered.Any())
                {
                    var grouped = filtered.GroupBy(r => r.Date.Date).ToList();
                    foreach (var group in grouped)
                    {
                        if (group != null)
                        {
                            try
                            {
                                var firstRecord = group.FirstOrDefault();
                                if (firstRecord != null)
                                    userRecords[group.Key] = firstRecord;
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error getting first record from group in UpdateCalendarForSelection: {ex}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating userRecords dictionary in UpdateCalendarForSelection: {ex}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Continue with empty dictionary
            }
        }

        int leadingPlaceholders = (int)monthStart.DayOfWeek;
        for (int i = 0; i < leadingPlaceholders; i++)
            AttendanceCalendarDays.Add(DailyAttendanceEntry.Placeholder());

        for (var day = monthStart; day <= monthEnd; day = day.AddDays(1))
        {
            userRecords.TryGetValue(day.Date, out var record);
            AttendanceCalendarDays.Add(new DailyAttendanceEntry
            {
                Date = day,
                IsCurrentMonth = true,
                IsFuture = day > today,
                Record = record
            });
        }

        while (AttendanceCalendarDays.Count % 7 != 0)
            AttendanceCalendarDays.Add(DailyAttendanceEntry.Placeholder());
    }

    private async Task HandleCalendarDayTappedAsync(DailyAttendanceEntry entry)
    {
        try
        {
            // Ignore placeholder days and future days
            if (entry == null || entry.IsPlaceholder || entry.IsFuture)
                return;

            // If no record exists, allow adding new one
            if (entry.Record == null)
            {
                // Set the selected user and date, then user can add attendance
                if (SelectedMonthlySummary != null)
                {
                    var user = Users.FirstOrDefault(u => u.Id == SelectedMonthlySummary.UserId);
                    if (user != null)
                    {
                        SelectedAttendanceUser = user;
                        AttendanceDate = entry.Date;
                        ShowStatus("Select status and add attendance", false);
                    }
                }
                return;
            }

            // Record exists - show options to edit or delete
            var editText = _localizationService.GetString("Edit");
            var deleteText = _localizationService.GetString("Delete");
            var cancelText = _localizationService.GetString("Cancel");
            var recordDate = entry.Record.Date.ToString("yyyy-MM-dd");
            var recordStatus = entry.Record.Status;

            if (Application.Current?.MainPage == null) return;

            var action = await Application.Current.MainPage.DisplayActionSheet(
                $"Attendance Record - {recordDate}",
                cancelText,
                deleteText, // Destructive action (delete) as cancel button
                editText);

            if (action == editText)
            {
                // Edit the record
                await EditAttendanceRecordAsync(entry.Record);
            }
            else if (action == deleteText)
            {
                // Confirm deletion
                var confirmText = _localizationService.GetString("Yes");
                var noText = _localizationService.GetString("No");
                
                var confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Delete Attendance",
                    $"Are you sure you want to delete the attendance record for {recordDate}?",
                    confirmText,
                    noText);

                if (confirmed)
                {
                    await DeleteAttendanceRecordAsync(entry.Record);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling calendar day tap: {ex}");
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    _localizationService.GetString("Error"),
                    "An error occurred while processing the request",
                    _localizationService.GetString("OK"));
            }
        }
    }

    // Filter and search methods
    private async Task ApplyFiltersAsync()
    {
        IsBusy = true;
        try
        {
            var records = await _databaseService.GetAttendanceRecordsAsync(FilterStartDate, FilterEndDate);
            if (records == null)
                records = new List<AttendanceRecord>();
            
            // Apply employee filter
            if (SelectedFilterEmployees != null && SelectedFilterEmployees.Any())
            {
                var employeeIds = SelectedFilterEmployees.Select(e => e.Id).ToHashSet();
                records = records.Where(r => employeeIds.Contains(r.UserId)).ToList();
            }

            // Apply status filter
            if (FilterStatus != "All")
            {
                if (FilterStatus == "Present")
                    records = records.Where(r => r.IsPresent).ToList();
                else if (FilterStatus == "Absent")
                    records = records.Where(r => !r.IsPresent).ToList();
            }

            // Apply overtime filter
            if (FilterOvertime == "With OT")
                records = records.Where(r => r.OvertimeHours > 0).ToList();
            else if (FilterOvertime == "Without OT")
                records = records.Where(r => r.OvertimeHours == 0).ToList();

            // Apply search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLowerInvariant();
                records = records.Where(r => 
                    r.UserName.ToLowerInvariant().Contains(searchLower) ||
                    (r.Notes ?? "").ToLowerInvariant().Contains(searchLower)
                ).ToList();
            }

            AttendanceRecords.Clear();
            foreach (var record in records.OrderByDescending(r => r.Date))
                AttendanceRecords.Add(record);

            await CalculateEnhancedStatisticsAsync(records);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearFilters()
    {
        FilterStartDate = DateTime.Today.AddDays(-30);
        FilterEndDate = DateTime.Today;
        SelectedFilterEmployees = new List<User>();
        FilterStatus = "All";
        FilterOvertime = "All";
        SearchText = "";
        _ = ApplyFiltersAsync();
    }

    // Bulk operations
    private async Task BulkDeleteRecordsAsync()
    {
        if (SelectedRecords == null || !SelectedRecords.Any())
            return;

        try
        {
            var confirmText = _localizationService.GetString("Yes");
            var noText = _localizationService.GetString("No");
            
            if (Application.Current?.MainPage != null)
            {
                var confirmed = await Application.Current.MainPage.DisplayAlert(
                    "Confirm Delete",
                    $"Are you sure you want to delete {SelectedRecords.Count} attendance record(s)?",
                    confirmText,
                    noText);

                if (!confirmed) return;
            }

            IsBusy = true;
            var count = SelectedRecords.Count;
            foreach (var record in SelectedRecords)
            {
                await _databaseService.DeleteAttendanceRecordAsync(record);
            }

            SelectedRecords.Clear();
            await LoadAttendanceAsync();
            ShowStatus($"Deleted {count} record(s)", false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in bulk delete: {ex}");
            ShowStatus("Error deleting records", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BulkEditRecordsAsync()
    {
        if (SelectedRecords == null || !SelectedRecords.Any())
            return;

        // For now, show a message - can be enhanced later
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Bulk Edit",
                "Bulk edit functionality will be implemented. Please edit records individually for now.",
                _localizationService.GetString("OK"));
        }
    }

    private void SelectAllRecords()
    {
        SelectedRecords = AttendanceRecords.ToList();
    }

    private void ClearSelection()
    {
        SelectedRecords.Clear();
    }

    // Export methods (stubs - will be implemented with ExportService)
    private async Task ExportAttendanceToExcelAsync()
    {
        IsBusy = true;
        try
        {
            ShowStatus("Excel export will be implemented soon", false);
            // TODO: Implement Excel export
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportAttendanceToPdfAsync()
    {
        IsBusy = true;
        try
        {
            ShowStatus("PDF export will be implemented soon", false);
            // TODO: Implement PDF export
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportSalesReportAsync()
    {
        IsBusy = true;
        try
        {
            ShowStatus("Sales report export will be implemented soon", false);
            // TODO: Implement sales report export
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExportInventoryReportAsync()
    {
        IsBusy = true;
        try
        {
            ShowStatus("Inventory report export will be implemented soon", false);
            // TODO: Implement inventory report export
        }
        finally
        {
            IsBusy = false;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class ProductReportItem
{
    public string Name { get; set; }
    public string Emoji { get; set; }
    public decimal Quantity { get; set; }
    public bool IsSoldByWeight { get; set; }
    public decimal TotalSales { get; set; }

    public string UnitLabel => IsSoldByWeight ? "KGS" : "PCS";
    public string QuantityDisplay => $"{Quantity:F2} {UnitLabel}";
    public string TotalSalesDisplay => $"${TotalSales:F2}";
}

public class AttendanceSummary
{
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int OvertimeCount { get; set; }
    public decimal TotalRegularHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalPayroll { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

public class AttendanceCalculationResult
{
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; } = "";
    public bool IsPresent { get; set; }
    public bool NeedsSalaryInput { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal DailyPay { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string AbsencePermissionType { get; set; } = "None";

    public static AttendanceCalculationResult Invalid(string message) =>
        new AttendanceCalculationResult { IsValid = false, ValidationMessage = message };

    public static AttendanceCalculationResult From(User user, DateTime date, string status, DateTime? checkIn, DateTime? checkOut, bool meetsConsecutiveDaysRequirement = false)
    {
        var result = new AttendanceCalculationResult
        {
            IsValid = true,
            ValidationMessage = "",
            CheckIn = checkIn,
            CheckOut = checkOut
        };

        // Check for absence types and reset status
        bool isAbsentWithPermission = status.Contains("Absent (With Permission)", StringComparison.OrdinalIgnoreCase);
        bool isAbsentWithoutPermission = status.Contains("Absent (Without Permission)", StringComparison.OrdinalIgnoreCase);
        bool isAbsent = isAbsentWithPermission || isAbsentWithoutPermission;
        bool isReset = string.Equals(status, "Reset", StringComparison.OrdinalIgnoreCase);
        bool requiresTimes = string.Equals(status, "Present", StringComparison.OrdinalIgnoreCase) || isReset;

        result.IsPresent = !isAbsent && !isReset; // Reset is not counted as present for cycle tracking
        
        // Store absence permission type
        if (isAbsentWithPermission)
            result.AbsencePermissionType = "WithPermission";
        else if (isAbsentWithoutPermission)
            result.AbsencePermissionType = "WithoutPermission";
        else if (isReset)
            result.AbsencePermissionType = "Reset";
        else
            result.AbsencePermissionType = "None";

        if (requiresTimes)
        {
            if (!checkIn.HasValue || !checkOut.HasValue)
                return Invalid("Specify check-in and out times.");

            var actualIn = checkIn.Value;
            var actualOut = checkOut.Value;
            if (actualOut <= actualIn)
                return Invalid("Checkout must be after check-in.");

            var scheduleStart = date.Date.AddHours(8);
            var scheduleEnd = scheduleStart.AddHours(8);

            var overlapStart = actualIn > scheduleStart ? actualIn : scheduleStart;
            var overlapEnd = actualOut < scheduleEnd ? actualOut : scheduleEnd;
            decimal regularHours = overlapEnd > overlapStart
                ? (decimal)(overlapEnd - overlapStart).TotalHours
                : 0m;

            regularHours = Math.Max(0m, Math.Min(regularHours, 8m));
            decimal overtimeHours = actualOut > scheduleEnd
                ? (decimal)(actualOut - scheduleEnd).TotalHours
                : 0m;

            // Month-based rules
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            
            // Rule 1: If month has 31 days and this is the 31st day, add 8h OT
            if (daysInMonth == 31 && date.Day == 31)
            {
                overtimeHours += 8m;
            }
            
            // Rule 2: Check if this is a reset day based on work cycle
            // Reset days are determined by work cycle: 7 days work → reset, 6 days work → reset, 7 days work → reset, 6 days work → reset...
            // meetsConsecutiveDaysRequirement indicates if this day is a reset day (reached 7 or 6 consecutive working days)
            
            // If status is "Reset", pay at normal rate (don't convert to OT, don't count in cycle)
            // If status is "Present" on a reset day (reached required consecutive days), count all hours as OT
            if (isReset && regularHours > 0)
            {
                // Reset status: paid at normal rate, don't convert to OT
                // regularHours stays as is
            }
            else if (meetsConsecutiveDaysRequirement && !isReset && regularHours > 0)
            {
                // Present on reset day (reached required consecutive days): count all hours as OT
                overtimeHours += regularHours;
                regularHours = 0m;
            }
            // If present but not a reset day, pay at normal rate

            result.RegularHours = Math.Round(regularHours, 2);
            result.OvertimeHours = Math.Round(overtimeHours, 2);
        }
        else
        {
            result.RegularHours = 0m;
            result.OvertimeHours = 0m;
        }

        // Reset status should still have hours and pay calculated (at normal rate)
        // Only absent days should have zero hours
        if (!result.IsPresent && !isReset)
        {
            result.RegularHours = 0m;
            result.OvertimeHours = 0m;
        }

        // Calculate pay using user's OT multiplier
        var salary = user?.MonthlySalary ?? 0m;
        var daysInMonthForCalc = DateTime.DaysInMonth(date.Year, date.Month);
        var hourlyRate = daysInMonthForCalc > 0 ? (salary / daysInMonthForCalc) / 8m : 0m;
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
}

public class MonthlyAttendanceSummary
{
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int WorkedDays { get; set; }
    public int EarnedRestDays { get; set; }
    public decimal RestDayPayout { get; set; }
    public int AbsenceWithPermission { get; set; }
    public int AbsenceWithoutPermission { get; set; }
    public decimal AbsenceDeductions { get; set; }
    public decimal ExpensesTotal { get; set; }
    public decimal TotalHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal Payroll { get; set; }

    public string TotalHoursDisplay => $"{TotalHours:F1}h";
    public string OvertimeDisplay => $"{OvertimeHours:F1}h";
    public string PayrollDisplay => $"${Payroll:F2}";
}

public class MonthlyAttendanceTotals
{
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalPayroll { get; set; }
    public decimal TotalRestPayout { get; set; }
    public decimal TotalAbsenceDeductions { get; set; }
}

public class DailyAttendanceEntry : INotifyPropertyChanged
{
    public DateTime Date { get; set; }
    public bool IsPlaceholder { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsFuture { get; set; }
    public AttendanceRecord Record { get; set; }

    public string DayText => IsPlaceholder ? "" : Date.Day.ToString();
    public string DetailText
    {
        get
        {
            if (IsPlaceholder || Record == null)
                return "";
            var total = Record.RegularHours + Record.OvertimeHours;
            if (Record.OvertimeHours > 0)
                return $"{Record.OvertimeHours:F1}h OT";
            if (total > 0)
                return $"{total:F1}h";
            return "";
        }
    }

    public bool IsPresent => Record?.IsPresent == true;
    public bool HasOvertime => (Record?.OvertimeHours ?? 0) > 0;
    public bool IsAbsent => Record != null && !IsPlaceholder && !IsFuture && !IsPresent;

    public static DailyAttendanceEntry Placeholder() => new()
    {
        IsPlaceholder = true,
        IsCurrentMonth = false,
        IsFuture = true
    };

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

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
