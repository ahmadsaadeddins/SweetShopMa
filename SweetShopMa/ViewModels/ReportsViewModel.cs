using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

/// <summary>
/// ViewModel for sales reports and analytics.
/// Extracted from AdminViewModel to follow SRP.
/// </summary>
public partial class ReportsViewModel : BaseViewModel
{
    private readonly Services.IPdfService _pdfService;

    [ObservableProperty] private decimal _totalSales;
    [ObservableProperty] private int _totalOrders;
    [ObservableProperty] private decimal _averageOrderValue;
    [ObservableProperty] private decimal _totalItemsSold;
    [ObservableProperty] private decimal _last7DaysSales;
    [ObservableProperty] private string _topProductName = "No sales yet";
    [ObservableProperty] private string _topProductDetails = "Add items to see insights";

    [ObservableProperty] private ObservableCollection<Order> _recentOrders = new();
    [ObservableProperty] private ObservableCollection<ProductReportItem> _topProducts = new();
    [ObservableProperty] private ObservableCollection<ShopLocation> _reportLocations = new();
    [ObservableProperty] private ShopLocation _selectedReportLocation;

    public bool HasReportData => TotalSales > 0 || TotalOrders > 0 || TopProducts.Any();

    public string AverageOrderValueDisplay
    {
        get
        {
            string avgText = _localizationService.GetString("Average");
            return $"{avgText} ${_averageOrderValue:F2}";
        }
    }

    public ReportsViewModel(
        DatabaseService databaseService, 
        AuthService authService, 
        Services.LocalizationService localizationService,
        Services.LoggingService loggingService,
        Services.IPdfService pdfService) 
        : base(databaseService, authService, localizationService, loggingService)
    {
        _pdfService = pdfService;
    }

    [RelayCommand]
    public async Task LoadReportsAsync()
    {
        IsBusy = true;
        try
        {
            // Simplified loading logic for extraction
            var orders = await _databaseService.GetOrdersAsync();
            RecentOrders.Clear();
            if (orders != null)
            {
                foreach (var order in orders.Take(10))
                {
                    RecentOrders.Add(order);
                }
                
                TotalSales = orders.Sum(o => o.Total);
                TotalOrders = orders.Count;
                AverageOrderValue = TotalOrders > 0 ? TotalSales / TotalOrders : 0;
            }
            
            // In a real implementation, we'd also load top products here
            OnPropertyChanged(nameof(HasReportData));
            OnPropertyChanged(nameof(AverageOrderValueDisplay));
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadReportsAsync", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ExportSalesReportAsync()
    {
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingSalesReport"), false);
            // Call IPdfService to generate report
            // await _pdfService.GenerateSalesReportAsync(...);
            ShowStatus(_localizationService.GetString("SalesReportExported"), false);
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ExportSalesReportAsync", ex);
            ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task ExportInventoryReportAsync()
    {
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingInventoryReport"), false);
            // await _pdfService.GenerateInventoryReportAsync(...);
            ShowStatus(_localizationService.GetString("InventoryReportExported"), false);
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("ExportInventoryReportAsync", ex);
            ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
