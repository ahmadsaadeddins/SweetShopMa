using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;
using Microsoft.Maui.ApplicationModel;
using System.Linq;

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
            var orders = await _databaseService.GetOrdersAsync();
            var allOrderItems = await _databaseService.GetAllOrderItemsAsync();
            var products = await _databaseService.GetProductsAsync();
            
            RecentOrders.Clear();
            if (orders != null && orders.Any())
            {
                var sortedOrders = orders.OrderByDescending(o => o.OrderDate).ToList();
                foreach (var order in sortedOrders.Take(20))
                {
                    RecentOrders.Add(order);
                }
                
                TotalSales = orders.Sum(o => o.Total);
                TotalOrders = orders.Count;
                AverageOrderValue = TotalOrders > 0 ? TotalSales / TotalOrders : 0;
                
                var sevenDaysAgo = DateTime.Today.AddDays(-7);
                Last7DaysSales = orders.Where(o => o.OrderDate >= sevenDaysAgo).Sum(o => o.Total);
            }
            else
            {
                TotalSales = 0;
                TotalOrders = 0;
                AverageOrderValue = 0;
                Last7DaysSales = 0;
            }

            if (allOrderItems != null && allOrderItems.Any())
            {
                TotalItemsSold = allOrderItems.Sum(i => i.Quantity);
                
                // Aggregate top products
                var topItems = allOrderItems
                    .GroupBy(i => i.ProductId)
                    .Select(g =>
                    {
                        var product = products?.FirstOrDefault(p => p.Id == g.Key);
                        return new ProductReportItem
                        {
                            Id = g.Key,
                            Name = product?.Name ?? g.FirstOrDefault()?.Name ?? "Unknown",
                            Quantity = g.Sum(i => i.Quantity),
                            TotalSales = g.Sum(i => i.ItemTotal),
                            Emoji = product?.Emoji ?? "📦",
                            IsSoldByWeight = product?.IsSoldByWeight ?? false
                        };
                    })
                    .OrderByDescending(i => i.TotalSales)
                    .ToList();

                TopProducts.Clear();
                foreach (var item in topItems.Take(10))
                {
                    TopProducts.Add(item);
                }

                if (topItems.Any())
                {
                    var top = topItems.First();
                    TopProductName = top.Name;
                    TopProductDetails = $"{top.QuantityDisplay} {top.UnitLabel} - {top.TotalSales:F2} ج.م";
                }
            }
            else
            {
                TotalItemsSold = 0;
                TopProducts.Clear();
                TopProductName = _localizationService.GetString("NoSalesYet");
                TopProductDetails = _localizationService.GetString("AddItemsToSeeInsights");
            }
            
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
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingSalesReport"), false);
            
            var filePath = await _pdfService.GenerateSalesReportPdfAsync(
                TotalSales,
                TotalOrders,
                AverageOrderValue,
                TotalItemsSold,
                Last7DaysSales,
                TopProducts.ToList(),
                RecentOrders.ToList());

            if (!string.IsNullOrEmpty(filePath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = _localizationService.GetString("SalesReport"),
                    File = new ShareFile(filePath)
                });
                ShowStatus(_localizationService.GetString("SalesReportExported"), false);
            }
            else
            {
                ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
            }
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
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            ShowStatus(_localizationService.GetString("GeneratingInventoryReport"), false);
            
            var products = await _databaseService.GetProductsAsync();
            var filePath = await _pdfService.GenerateInventoryReportPdfAsync(products);

            if (!string.IsNullOrEmpty(filePath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = _localizationService.GetString("InventoryReport"),
                    File = new ShareFile(filePath)
                });
                ShowStatus(_localizationService.GetString("InventoryReportExported"), false);
            }
            else
            {
                ShowStatus(_localizationService.GetString("FailedToExportReport"), true);
            }
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
