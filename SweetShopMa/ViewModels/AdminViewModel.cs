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

    public void RefreshLocalizedProperties()
    {
        OnPropertyChanged(nameof(AverageOrderValueDisplay));
        OnPropertyChanged(nameof(ReportStatusText));
    }
}
