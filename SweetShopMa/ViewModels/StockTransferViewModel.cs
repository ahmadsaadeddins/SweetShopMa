using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Resources;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

public partial class StockTransferViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly IShopSettingsService _settingsService;

    [ObservableProperty] private ObservableCollection<Product> _products = new();
    [ObservableProperty] private ObservableCollection<ShopLocation> _locations = new();
    
    [ObservableProperty] private Product _selectedProduct;
    [ObservableProperty] private ShopLocation _fromLocation;
    [ObservableProperty] private ShopLocation _toLocation;
    [ObservableProperty] private decimal _quantity;

    [ObservableProperty] private decimal _currentSourceStock;
    [ObservableProperty] private bool _isBusy;

    public StockTransferViewModel(DatabaseService db, IShopSettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            var productList = await _db.GetProductsAsync();
            Products = new ObservableCollection<Product>(productList);

            var locationList = await _settingsService.GetLocationsAsync();
            Locations = new ObservableCollection<ShopLocation>(locationList);
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnFromLocationChanged(ShopLocation value) => UpdateSourceStock();
    partial void OnSelectedProductChanged(Product value) => UpdateSourceStock();

    private async void UpdateSourceStock()
    {
        if (SelectedProduct != null && FromLocation != null)
        {
            var stock = await _db.GetProductStockAsync(SelectedProduct.Id, FromLocation.Id);
            CurrentSourceStock = stock?.Stock ?? 0m;
        }
        else
        {
            CurrentSourceStock = 0m;
        }
    }

    [RelayCommand]
    private async Task ExecuteTransferAsync()
    {
        if (SelectedProduct == null || FromLocation == null || ToLocation == null || Quantity <= 0)
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.PleaseFillAllFieldsCorrectly, Strings.OK);
            return;
        }

        if (FromLocation.Id == ToLocation.Id)
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.SourceAndDestinationCannotBeSame, Strings.OK);
            return;
        }

        if (Quantity > CurrentSourceStock)
        {
            await Shell.Current.DisplayAlert(Strings.Error, Strings.InsufficientStockAtSource, Strings.OK);
            return;
        }

        IsBusy = true;
        try
        {
            var transfer = new StockTransfer
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                FromLocationId = FromLocation.Id,
                FromLocationName = FromLocation.LocationName,
                ToLocationId = ToLocation.Id,
                ToLocationName = ToLocation.LocationName,
                Quantity = Quantity,
                TransferDate = DateTime.Now
                // PerformedByUserId - will set in service or here if we have SessionContext
            };

            await _db.ExecuteStockTransferAsync(transfer);
            await Shell.Current.DisplayAlert(Strings.Success, Strings.TransferCompleted, Strings.OK);
            
            Quantity = 0;
            UpdateSourceStock();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
