using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

/// <summary>
/// ViewModel for managing products.
/// Extracted from AdminViewModel to follow SRP.
/// </summary>
public partial class ProductManagementViewModel : BaseViewModel
{
    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Product> _filteredProducts = new();

    // Product form fields
    [ObservableProperty] private string _newProductName = "";
    [ObservableProperty] private string _newProductEmoji = "🍬";
    [ObservableProperty] private string _newProductBarcode = "";
    [ObservableProperty] private string _newProductPrice = "";
    [ObservableProperty] private string _newProductStock = "";
    [ObservableProperty] private bool _newProductIsWeight;
    [ObservableProperty] private string _newProductCategory = "All";

    // Edit product fields
    [ObservableProperty] private Product _selectedProduct;
    [ObservableProperty] private string _editProductName = "";
    [ObservableProperty] private string _editProductEmoji = "";
    [ObservableProperty] private string _editProductCategory = "";
    [ObservableProperty] private string _editProductPrice = "";

    // Product search
    [ObservableProperty]
    private string _productSearchText = "";

    partial void OnProductSearchTextChanged(string value) => FilterProducts();

    partial void OnSelectedProductChanged(Product value)
    {
        UpdateProductCommand.NotifyCanExecuteChanged();
        _loggingService?.LogDebug("ProductManagementViewModel", $"SelectedProduct changed to: {value?.Name ?? "null"} (ID: {value?.Id ?? 0})");
    }

    public bool CanManageStock => _authService.CanManageStock;

    public ProductManagementViewModel(
        DatabaseService databaseService, 
        AuthService authService, 
        Services.LocalizationService localizationService,
        Services.LoggingService loggingService) 
        : base(databaseService, authService, localizationService, loggingService)
    {
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        _loggingService?.LogMethodEntry("ProductManagementViewModel", nameof(LoadProductsAsync));
        IsBusy = true;
        try
        {
            var products = await _databaseService.GetProductsAsync();
            Products.Clear();
            if (products != null)
            {
                _loggingService?.LogDebug("ProductManagementViewModel", $"Loaded {products.Count()} products");
                foreach (var product in products.OrderBy(p => p.Name))
                {
                    Products.Add(product);
                }
            }
            FilterProducts();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("LoadProductsAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorLoadingProducts"), true);
        }
        finally
        {
            IsBusy = false;
            _loggingService?.LogMethodExit("ProductManagementViewModel", nameof(LoadProductsAsync));
        }
    }

    [RelayCommand]
    public void FilterProducts()
    {
        if (string.IsNullOrWhiteSpace(ProductSearchText))
        {
            FilteredProducts = new ObservableCollection<Product>(Products);
            return;
        }

        var searchLower = ProductSearchText.ToLowerInvariant();
        var filtered = Products.Where(p => 
            p.Name.ToLowerInvariant().Contains(searchLower) || 
            (!string.IsNullOrEmpty(p.Barcode) && p.Barcode.ToLowerInvariant().Contains(searchLower))
        );

        FilteredProducts = new ObservableCollection<Product>(filtered);
    }

    [RelayCommand(CanExecute = nameof(CanManageStock))]
    public async Task AddProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProductName) || string.IsNullOrWhiteSpace(NewProductPrice))
        {
            ShowStatus(_localizationService.GetString("PleaseFillAllProductFields"), true);
            return;
        }

        if (!decimal.TryParse(NewProductPrice, out var price))
        {
            ShowStatus(_localizationService.GetString("EnterValidPrice"), true);
            return;
        }

        decimal.TryParse(NewProductStock, out var stock);

        IsBusy = true;
        try
        {
            if (!string.IsNullOrWhiteSpace(NewProductBarcode) && await _databaseService.ProductBarcodeExistsAsync(NewProductBarcode.Trim()))
            {
                ShowStatus(_localizationService.GetString("BarcodeExists"), true);
                return;
            }

            var product = new Product
            {
                Name = NewProductName.Trim(),
                Emoji = NewProductEmoji.Trim(),
                Barcode = NewProductBarcode.Trim(),
                Price = price,
                IsSoldByWeight = NewProductIsWeight,
                Category = NewProductCategory
            };

            await _databaseService.SaveProductAsync(product);

            // Add initial stock if provided
            if (stock > 0)
            {
                // This would normally be handled by a StockManagement service, 
                // but following original logic from AdminViewModel
                await _databaseService.UpdateProductStockAsync(product.Id, 1, stock); // Default to location 1
            }

            ShowStatus(string.Format(_localizationService.GetString("CreatedProduct"), product.Name), false);

            await LoadProductsAsync();

            // Clear form
            NewProductName = "";
            NewProductEmoji = "🍬";
            NewProductBarcode = "";
            NewProductPrice = "";
            NewProductStock = "";
            NewProductIsWeight = false;
            NewProductCategory = "All";
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("AddProductAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorCreatingProduct"), true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanManageStock))]
    public void EditProduct(Product product)
    {
        if (product == null)
        {
            _loggingService?.LogWarning("ProductManagementViewModel.EditProduct", "Attempted to edit null product");
            return;
        }
        
        _loggingService?.LogDebug("ProductManagementViewModel", $"Starting edit for product: {product.Name} (ID: {product.Id})");
        SelectedProduct = product;
        EditProductName = product.Name;
        EditProductEmoji = product.Emoji;
        EditProductCategory = product.Category;
        EditProductPrice = product.Price.ToString();
    }

    [RelayCommand(CanExecute = nameof(CanManageStock))]
    public async Task UpdateProductAsync()
    {
        _loggingService?.LogMethodEntry("ProductManagementViewModel", nameof(UpdateProductAsync), $"ID: {SelectedProduct?.Id}");
        if (SelectedProduct == null)
        {
            _loggingService?.LogWarning("ProductManagementViewModel.UpdateProductAsync", "SelectedProduct is null");
            return;
        }

        if (string.IsNullOrWhiteSpace(EditProductName) || string.IsNullOrWhiteSpace(EditProductPrice))
        {
            _loggingService?.LogDebug("ProductManagementViewModel", "Validation failed: Name or Price is empty");
            ShowStatus(_localizationService.GetString("PleaseFillAllProductFields"), true);
            return;
        }

        if (!decimal.TryParse(EditProductPrice, out var price))
        {
            _loggingService?.LogDebug("ProductManagementViewModel", $"Validation failed: Invalid price format '{EditProductPrice}'");
            ShowStatus(_localizationService.GetString("EnterValidPrice"), true);
            return;
        }

        IsBusy = true;
        try
        {
            _loggingService?.LogDebug("ProductManagementViewModel", $"Updating product {SelectedProduct.Id}: {SelectedProduct.Name} -> {EditProductName}, Price: {SelectedProduct.Price} -> {price}");
            SelectedProduct.Name = EditProductName.Trim();
            SelectedProduct.Emoji = EditProductEmoji.Trim();
            SelectedProduct.Category = EditProductCategory;
            SelectedProduct.Price = price;

            await _databaseService.SaveProductAsync(SelectedProduct);
            _loggingService?.LogDebug("ProductManagementViewModel", "Product saved successfully");
            ShowStatus(_localizationService.GetString("ProductUpdated"), false);

            await LoadProductsAsync();
            CancelEditProduct();
        }
        catch (Exception ex)
        {
            _loggingService?.LogError("UpdateProductAsync", ex);
            ShowStatus(_localizationService.GetString("ErrorUpdatingProduct"), true);
        }
        finally
        {
            IsBusy = false;
            _loggingService?.LogMethodExit("ProductManagementViewModel", nameof(UpdateProductAsync));
        }
    }

    [RelayCommand]
    public void CancelEditProduct()
    {
        _loggingService?.LogDebug("ProductManagementViewModel", "Cancelling product edit");
        SelectedProduct = null;
        EditProductName = "";
        EditProductEmoji = "";
        EditProductCategory = "";
        EditProductPrice = "";
    }
}
