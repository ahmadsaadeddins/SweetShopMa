using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

/// <summary>
/// Main ViewModel for the Admin Panel.
/// Acts as a coordinator for specialized sub-ViewModels.
/// </summary>
public partial class AdminViewModel : BaseViewModel
{
    private readonly IServiceProvider _serviceProvider;

    public UserManagementViewModel UserManagement { get; }
    public ProductManagementViewModel ProductManagement { get; }
    public AttendanceViewModel Attendance { get; }
    public ReportsViewModel Reports { get; }

    // Navigation and General Properties
    public bool IsAdmin => _authService.IsAdmin;
    public bool IsDeveloper => _authService.IsDeveloper;
    public bool CanManageUsers => _authService.CanManageUsers;
    public bool CanManageStock => _authService.CanManageStock;
    public bool CanUseAttendanceTracker => _authService.CanUseAttendanceTracker && Services.FeatureFlags.IsAttendanceTrackerEnabled;

    public bool IsAuthorized => IsAdmin || IsDeveloper;

    public AdminViewModel(
        DatabaseService databaseService, 
        AuthService authService, 
        Services.LocalizationService localizationService,
        Services.LoggingService loggingService,
        IServiceProvider serviceProvider,
        UserManagementViewModel userManagement,
        ProductManagementViewModel productManagement,
        AttendanceViewModel attendance,
        ReportsViewModel reports) 
        : base(databaseService, authService, localizationService, loggingService)
    {
        _serviceProvider = serviceProvider;
        UserManagement = userManagement;
        ProductManagement = productManagement;
        Attendance = attendance;
        Reports = reports;

        // Sync status messages and property changes from sub-viewmodels
        UserManagement.PropertyChanged += (s, e) => { 
            if (e.PropertyName == nameof(StatusMessage)) ShowStatus(UserManagement.StatusMessage, UserManagement.IsErrorStatus);
            OnPropertyChanged(e.PropertyName); 
        };
        ProductManagement.PropertyChanged += (s, e) => { 
            if (e.PropertyName == nameof(StatusMessage)) ShowStatus(ProductManagement.StatusMessage, ProductManagement.IsErrorStatus);
            if (e.PropertyName == nameof(ProductManagement.SelectedProduct))
            {
                OnPropertyChanged(nameof(IsEditingProduct));
                UpdateProductCommand.NotifyCanExecuteChanged();
                _loggingService?.LogDebug("AdminViewModel", "IsEditingProduct notification triggered by SelectedProduct change");
            }
            OnPropertyChanged(e.PropertyName); 
        };
        Attendance.PropertyChanged += (s, e) => { 
            if (e.PropertyName == nameof(StatusMessage)) ShowStatus(Attendance.StatusMessage, Attendance.IsErrorStatus);
            OnPropertyChanged(e.PropertyName); 
        };
        Reports.PropertyChanged += (s, e) => { 
            if (e.PropertyName == nameof(StatusMessage)) ShowStatus(Reports.StatusMessage, Reports.IsErrorStatus);
            OnPropertyChanged(e.PropertyName); 
        };
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        _loggingService?.LogMethodEntry("AdminViewModel", nameof(InitializeAsync));
        IsBusy = true;
        try
        {
            _loggingService?.LogDebug("AdminViewModel", "Initializing sub-viewmodels...");
            await Task.WhenAll(
                UserManagement.LoadUsersAsync(),
                ProductManagement.LoadProductsAsync(),
                Attendance.LoadAttendanceAsync(),
                Reports.LoadReportsAsync()
            );
            _loggingService?.LogDebug("AdminViewModel", "Sub-viewmodels initialized successfully");
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("AdminViewModel.InitializeAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorInitializingAdmin"), true);
        }
        finally
        {
            IsBusy = false;
            _loggingService?.LogMethodExit("AdminViewModel", nameof(InitializeAsync));
        }
    }

    // Reports Proxies
    public decimal TotalSales => Reports.TotalSales;
    public int TotalOrders => Reports.TotalOrders;
    public string AverageOrderValueDisplay => Reports.AverageOrderValueDisplay;
    public decimal TotalItemsSold => Reports.TotalItemsSold;
    public decimal Last7DaysSales => Reports.Last7DaysSales;
    public string TopProductName => Reports.TopProductName;
    public string TopProductDetails => Reports.TopProductDetails;
    public string ReportStatusText => Reports.HasReportData ? _localizationService.GetString("DataReady") : _localizationService.GetString("NoDataYet");
    public string ReportStatusTextColor => Reports.HasReportData ? "#1f7a4d" : "#c00000";
    public ObservableCollection<ProductReportItem> TopProducts => Reports.TopProducts;
    public ObservableCollection<Order> RecentOrders => Reports.RecentOrders;
    public ObservableCollection<ShopLocation> ReportLocations => Reports.ReportLocations;
    public ShopLocation SelectedReportLocation { get => Reports.SelectedReportLocation; set => Reports.SelectedReportLocation = value; }

    // User Management Proxies
    public ObservableCollection<User> Users => UserManagement.Users;
    public IAsyncRelayCommand ToggleUserStatusCommand => UserManagement.ToggleUserStatusCommand;

    // Product Management Proxies
    public ObservableCollection<Product> FilteredProducts => ProductManagement.FilteredProducts;
    public bool IsEditingProduct => ProductManagement.SelectedProduct != null;
    
    // Commands
    public IAsyncRelayCommand ExportSalesReportCommand => Reports.ExportSalesReportCommand;
    public IAsyncRelayCommand ExportInventoryReportCommand => Reports.ExportInventoryReportCommand;
    public IAsyncRelayCommand ExportPayrollCommand => Attendance.ExportPayrollCommand;
    public IAsyncRelayCommand ExportAttendanceCommand => Attendance.ExportAttendanceCommand;
    public IAsyncRelayCommand OpenAttendancePageCommand => Attendance.OpenAttendancePageCommand;
    public IAsyncRelayCommand AddProductCommand => ProductManagement.AddProductCommand;
    public IRelayCommand<Product> EditProductCommand => ProductManagement.EditProductCommand;
    public IAsyncRelayCommand UpdateProductCommand => ProductManagement.UpdateProductCommand;
    public IRelayCommand CancelEditProductCommand => ProductManagement.CancelEditProductCommand;

    // Product Form Properties (Direct pass-through for now)
    public string NewProductName { get => ProductManagement.NewProductName; set => ProductManagement.NewProductName = value; }
    public string NewProductEmoji { get => ProductManagement.NewProductEmoji; set => ProductManagement.NewProductEmoji = value; }
    public string NewProductBarcode { get => ProductManagement.NewProductBarcode; set => ProductManagement.NewProductBarcode = value; }
    public string NewProductPrice { get => ProductManagement.NewProductPrice; set => ProductManagement.NewProductPrice = value; }
    public string NewProductStock { get => ProductManagement.NewProductStock; set => ProductManagement.NewProductStock = value; }
    public bool NewProductIsWeight { get => ProductManagement.NewProductIsWeight; set => ProductManagement.NewProductIsWeight = value; }
    public string NewProductCategory { get => ProductManagement.NewProductCategory; set => ProductManagement.NewProductCategory = value; }
    public string ProductSearchText { get => ProductManagement.ProductSearchText; set => ProductManagement.ProductSearchText = value; }
    public string EditProductName { get => ProductManagement.EditProductName; set => ProductManagement.EditProductName = value; }
    public string EditProductEmoji { get => ProductManagement.EditProductEmoji; set => ProductManagement.EditProductEmoji = value; }
    public string EditProductCategory { get => ProductManagement.EditProductCategory; set => ProductManagement.EditProductCategory = value; }
    public string EditProductPrice { get => ProductManagement.EditProductPrice; set => ProductManagement.EditProductPrice = value; }

    // Attendance Proxies
    public ObservableCollection<EmployeeExpense> EmployeeExpenses => Attendance.EmployeeExpenses;
    public string SelectedAttendanceStatusKey => Attendance.SelectedAttendanceStatusKey;
    public async Task LoadAttendanceAsync() => await Attendance.LoadAttendanceAsync();

    // Attendance Records and Calendar
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

    // Commands
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

    public void RefreshLocalizedProperties()
    {
        OnPropertyChanged(nameof(AverageOrderValueDisplay));
        OnPropertyChanged(nameof(ReportStatusText));
    }
}
