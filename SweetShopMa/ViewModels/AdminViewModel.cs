using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
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
    public ObservableCollection<Order> RecentOrders { get; } = new();
    public ObservableCollection<ProductReportItem> TopProducts { get; } = new();
    public ObservableCollection<AttendanceRecord> AttendanceRecords { get; } = new();
    public ObservableCollection<DailyAttendanceEntry> AttendanceCalendarDays { get; } = new();
    public ObservableCollection<MonthlyAttendanceSummary> MonthlyAttendanceSummaries { get; } = new();

    public ICommand AddUserCommand { get; }
    public ICommand AddProductCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ToggleUserStatusCommand { get; }
    public ICommand OpenOrderDetailsCommand { get; }
    public ICommand AddAttendanceCommand { get; }
    public ICommand OpenAttendancePageCommand { get; }
    public ICommand EditProductCommand { get; }
    public ICommand UpdateProductCommand { get; }
    public ICommand CancelEditProductCommand { get; }

    // Attendance form fields
    private User _selectedAttendanceUser;
    private DateTime _attendanceDate = DateTime.Today;
    private string _selectedAttendanceStatus = "Present";
    private string _attendanceNotes = "";
    private TimeSpan _attendanceCheckInTime = new(8, 0, 0);
    private TimeSpan _attendanceCheckOutTime = new(16, 0, 0);
    private string _attendancePreview = "Regular: 0h • OT: 0h • Pay $0.00";

    private readonly string[] _attendanceStatuses =
        { "Present", "Absent" };

    private AttendanceSummary _attendanceSummary = new();
    private MonthlyAttendanceTotals _monthlySummaryTotals = new();
    private DateTime _summaryMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private MonthlyAttendanceSummary _selectedMonthlySummary;
    private List<AttendanceRecord> _currentMonthRecords = new();
    private string _newUserSalary = "0";

    public AdminViewModel(DatabaseService databaseService, AuthService authService, IServiceProvider serviceProvider, Services.LocalizationService localizationService)
    {
        _databaseService = databaseService;
        _authService = authService;
        _serviceProvider = serviceProvider;
        _localizationService = localizationService;

        AddUserCommand = new Command(async () => await AddUserAsync(), () => !IsBusy);
        AddProductCommand = new Command(async () => await AddProductAsync(), () => !IsBusy);
        RefreshCommand = new Command(async () => await InitializeAsync(), () => !IsBusy);
        ToggleUserStatusCommand = new Command<User>(async user => await ToggleUserStatusAsync(user), _ => !IsBusy);
        OpenOrderDetailsCommand = new Command<Order>(async order => await ShowOrderDetailsAsync(order));
        AddAttendanceCommand = new Command(async () => await AddAttendanceAsync(), () => !IsBusy);
        OpenAttendancePageCommand = new Command(async () => await OpenAttendancePage());
        EditProductCommand = new Command<Product>(async product => await EditProductAsync(product));
        UpdateProductCommand = new Command(async () => await UpdateProductAsync());
        CancelEditProductCommand = new Command(() => CancelEditProduct());

        _authService.OnUserChanged += _ => OnPropertyChanged(nameof(IsAuthorized));
        TopProducts.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasReportData));
            OnPropertyChanged(nameof(ReportStatusText));
            OnPropertyChanged(nameof(ReportStatusTextColor));
        };

        UpdateAttendancePreview();
    }

    private async Task LoadMonthlySummaryAsync()
    {
        var monthStart = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var today = DateTime.Today;

        _currentMonthRecords = await _databaseService.GetAttendanceRecordsAsync(monthStart, monthEnd);

        var userSnapshot = Users.Any()
            ? Users.ToList()
            : await _databaseService.GetUsersAsync();

        var summaries = new List<MonthlyAttendanceSummary>();

        foreach (var user in userSnapshot)
        {
            var userRecords = _currentMonthRecords.Where(r => r.UserId == user.Id).ToList();
            var recordByDate = userRecords.ToDictionary(r => r.Date.Date, r => r);

            int presentDays = 0;
            int absentDays = 0;

            for (var day = monthStart; day <= monthEnd && day <= today; day = day.AddDays(1))
            {
                if (recordByDate.TryGetValue(day.Date, out var rec))
                {
                    if (rec.IsPresent)
                        presentDays++;
                    else
                        absentDays++;
                }
                else
                {
                    absentDays++;
                }
            }

            var summary = new MonthlyAttendanceSummary
            {
                UserId = user.Id,
                UserName = user.Name,
                DaysPresent = presentDays,
                DaysAbsent = absentDays,
                OvertimeHours = userRecords.Sum(r => r.OvertimeHours),
                TotalHours = userRecords.Sum(r => r.RegularHours + r.OvertimeHours),
                Payroll = userRecords.Sum(r => r.DailyPay)
            };
            summaries.Add(summary);
        }

        MonthlyAttendanceSummaries.Clear();
        foreach (var summary in summaries.OrderBy(s => s.UserName))
            MonthlyAttendanceSummaries.Add(summary);

        MonthlySummaryTotals = new MonthlyAttendanceTotals
        {
            TotalPayroll = summaries.Sum(s => s.Payroll),
            TotalOvertimeHours = summaries.Sum(s => s.OvertimeHours),
            TotalPresentDays = summaries.Sum(s => s.DaysPresent),
            TotalAbsentDays = summaries.Sum(s => s.DaysAbsent)
        };

        SelectedMonthlySummary = MonthlyAttendanceSummaries.FirstOrDefault();
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
                UpdateAttendancePreview();
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
                UpdateAttendancePreview();
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
                UpdateAttendancePreview();
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
                UpdateAttendancePreview();
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
                UpdateAttendancePreview();
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

    public AttendanceSummary AttendanceSummary
    {
        get => _attendanceSummary;
        set { if (_attendanceSummary != value) { _attendanceSummary = value; OnPropertyChanged(); } }
    }

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
            foreach (var user in users.Where(u => !u.IsDeveloper))
                Users.Add(user);
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
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadReportsAsync()
    {
        IsBusy = true;
        try
        {
            var orders = await _databaseService.GetOrdersAsync();
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

            if (groupedProducts.Any())
            {
                var top = groupedProducts.First();
                TopProductName = $"{top.Emoji} {top.Name}";
                TopProductDetails = $"{top.Quantity:F2} {top.UnitLabel} • ${top.TotalSales:F2}";
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
            AttendanceRecords.Clear();
            foreach (var record in records)
                AttendanceRecords.Add(record);

            AttendanceSummary = new AttendanceSummary
            {
                PresentCount = records.Count(r => string.Equals(r.Status, "Present", StringComparison.OrdinalIgnoreCase)),
                AbsentCount = records.Count(r => string.Equals(r.Status, "Absent", StringComparison.OrdinalIgnoreCase)),
                OvertimeCount = records.Count(r => string.Equals(r.Status, "Overtime", StringComparison.OrdinalIgnoreCase)),
                TotalRegularHours = records.Sum(r => r.RegularHours),
                TotalOvertimeHours = records.Sum(r => r.OvertimeHours),
                TotalPayroll = records.Sum(r => r.DailyPay),
                LastUpdated = DateTime.Now
            };
        }
        finally
        {
            IsBusy = false;
        }

        await LoadMonthlySummaryAsync();
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

        var calculation = CalculateAttendanceForEntry();
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
            Notes = AttendanceNotes?.Trim() ?? ""
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
            UpdateAttendancePreview();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenAttendancePage()
    {
        if (!CanUseAttendanceTracker)
        {
            ShowStatus("You don't have permission to use the attendance tracker.", true);
            return;
        }
        
        var attendancePage = _serviceProvider.GetService<Views.AttendancePage>();
        if (attendancePage != null)
        {
            await LoadAttendanceAsync();
            await Shell.Current.Navigation.PushAsync(attendancePage);
        }
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
                MonthlySalary = salary
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
        string.Equals(status, "Present", StringComparison.OrdinalIgnoreCase);

    private void UpdateAttendancePreview()
    {
        if (SelectedAttendanceUser == null)
        {
            AttendancePreview = "Select an employee to preview pay.";
            return;
        }

        var calc = CalculateAttendanceForEntry(validateOnly: true);
        if (!calc.IsValid || !string.IsNullOrWhiteSpace(calc.ValidationMessage))
        {
            AttendancePreview = calc.ValidationMessage;
            return;
        }

        AttendancePreview = $"Regular: {calc.RegularHours:F2}h | OT: {calc.OvertimeHours:F2}h | Pay ${calc.DailyPay:F2}";
    }

    private AttendanceCalculationResult CalculateAttendanceForEntry(bool validateOnly = false)
    {
        if (SelectedAttendanceUser == null)
            return AttendanceCalculationResult.Invalid("Select an employee.");

        var status = SelectedAttendanceStatus ?? "Present";
        bool requiresTimes = StatusRequiresTimes(status);

        DateTime? checkIn = null;
        DateTime? checkOut = null;

        if (requiresTimes)
        {
            checkIn = AttendanceDate.Date + AttendanceCheckInTime;
            checkOut = AttendanceDate.Date + AttendanceCheckOutTime;

            if (checkOut <= checkIn)
                return AttendanceCalculationResult.Invalid("Checkout must be after check-in.");
        }

        var result = AttendanceCalculationResult.From(
            SelectedAttendanceUser,
            AttendanceDate,
            status,
            checkIn,
            checkOut);

        if (result.NeedsSalaryInput)
        {
            result.ValidationMessage = "Set monthly salary to calculate pay.";
            if (!validateOnly)
            {
                result.IsValid = false;
                return result;
            }
        }

        result.IsValid = true;
        return result;
    }

    private void UpdateCalendarForSelection()
    {
        AttendanceCalendarDays.Clear();
        if (SelectedMonthlySummary == null)
            return;

        var monthStart = new DateTime(SummaryMonth.Year, SummaryMonth.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var today = DateTime.Today;

        var userRecords = _currentMonthRecords
            .Where(r => r.UserId == SelectedMonthlySummary.UserId)
            .ToDictionary(r => r.Date.Date, r => r);

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

    public static AttendanceCalculationResult Invalid(string message) =>
        new AttendanceCalculationResult { IsValid = false, ValidationMessage = message };

    public static AttendanceCalculationResult From(User user, DateTime date, string status, DateTime? checkIn, DateTime? checkOut)
    {
        var result = new AttendanceCalculationResult
        {
            IsValid = true,
            ValidationMessage = "",
            CheckIn = checkIn,
            CheckOut = checkOut
        };

        bool isAbsent = string.Equals(status, "Absent", StringComparison.OrdinalIgnoreCase);
        bool requiresTimes = string.Equals(status, "Present", StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(status, "Overtime", StringComparison.OrdinalIgnoreCase);

        result.IsPresent = !isAbsent;

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

            result.RegularHours = Math.Round(regularHours, 2);
            result.OvertimeHours = Math.Round(overtimeHours, 2);
        }
        else
        {
            result.RegularHours = 0m;
            result.OvertimeHours = 0m;
        }

        if (!result.IsPresent)
        {
            result.RegularHours = 0m;
            result.OvertimeHours = 0m;
        }

        var salary = user?.MonthlySalary ?? 0m;
        var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        var hourlyRate = daysInMonth > 0 ? (salary / daysInMonth) / 8m : 0m;
        var overtimeRate = hourlyRate * 1.5m;
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
    public bool IsAbsent => !IsPlaceholder && !IsFuture && !IsPresent;

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

