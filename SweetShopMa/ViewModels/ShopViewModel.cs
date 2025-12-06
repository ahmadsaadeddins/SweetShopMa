using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.Models;
using SweetShopMa.Services;
using SweetShopMa.Views;

namespace SweetShopMa.ViewModels;

/// <summary>
/// Main ViewModel for the shopping interface (MainPage).
/// 
/// WHAT IS A VIEWMODEL?
/// A ViewModel is part of the MVVM (Model-View-ViewModel) pattern. It acts as a bridge
/// between the View (UI) and the Model (data). The ViewModel contains:
/// - Business logic (what happens when user clicks buttons, etc.)
/// - Data that the UI displays (products, cart items, totals)
/// - Commands that the UI can bind to (AddToCartCommand, CheckoutCommand, etc.)
/// 
/// KEY RESPONSIBILITIES:
/// - Display and filter products
/// - Manage shopping cart (add, remove, update quantities)
/// - Handle barcode scanning for quick product lookup
/// - Process checkout (create order, update inventory)
/// - Restock products (if user has permission)
/// - Print receipts and open cash drawer
/// - Manage focus between UI fields (for keyboard navigation)
/// 
/// DATA BINDING:
/// Properties in this ViewModel are bound to UI elements in MainPage.xaml.
/// When a property changes, the UI automatically updates (thanks to INotifyPropertyChanged).
/// 
/// COMMANDS:
/// Commands are a way to handle user actions (button clicks) in MVVM.
/// Instead of event handlers in code-behind, we use Commands in the ViewModel.
/// 
/// EXAMPLE FLOW:
/// 1. User types barcode → QuickSearchText property changes
/// 2. FilterProducts() is called automatically
/// 3. FilteredProducts collection updates
/// 4. UI automatically shows matching products (data binding)
/// 5. User selects product → SelectedQuickProduct property changes
/// 6. User enters quantity and presses Enter → QuickAddCommand executes
/// 7. Product is added to cart → CartItems collection updates
/// 8. UI automatically shows updated cart (data binding)
/// </summary>
public class ShopViewModel : INotifyPropertyChanged
{
    private readonly CartService _cartService;
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly Services.LocalizationService _localizationService;
    private readonly Services.IPrintService _printService;
    private readonly Services.ICashDrawerService _cashDrawerService;
    
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<CartItem> CartItems { get; } = new();
    public ObservableCollection<Product> FilteredProducts { get; } = new();
    public ObservableCollection<string> Categories { get; } = new();

    private decimal _total;
    public decimal Total
    {
        get => _total;
        set { if (_total != value) { _total = value; OnPropertyChanged(); } }
    }

    private bool _isCheckoutEnabled;
    public bool IsCheckoutEnabled
    {
        get => _isCheckoutEnabled;
        set { if (_isCheckoutEnabled != value) { _isCheckoutEnabled = value; OnPropertyChanged(); } }
    }

    private bool _isPostCheckout = false; // Flag to prevent add command immediately after checkout
    private bool _isHandlingError = false; // Flag to prevent focus changes during error handling
    
    public bool IsHandlingError => _isHandlingError; // Public property to check error handling state
    public bool IsPostCheckout => _isPostCheckout; // Public property to check post-checkout state
    
    private void UpdateQuickAddCommandCanExecute()
    {
        (QuickAddCommand as Command)?.ChangeCanExecute();
    }

    private bool _isAdmin;
    public bool IsAdmin
    {
        get => _isAdmin;
        set 
        { 
            if (_isAdmin != value) 
            { 
                _isAdmin = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsAuthenticated));
                OnPropertyChanged(nameof(CanRestock));
                OnPropertyChanged(nameof(CanManageStock));
            } 
        }
    }
    
    // Permission properties
    public bool CanRestock => _authService.CanRestock;
    public bool CanManageStock => _authService.CanManageStock;
    public bool CanAccessAdminPanel => _authService.CanManageUsers || _authService.CanManageStock;

    private string _currentUserName;
    public string CurrentUserName
    {
        get => _currentUserName;
        set { if (_currentUserName != value) { _currentUserName = value; OnPropertyChanged(); } }
    }

    // Quick input for cashier
    private string _quickSearchText = "";
    public string QuickSearchText
    {
        get => _quickSearchText;
        set
        {
            if (_quickSearchText != value)
            {
                _quickSearchText = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }
    }

    private string _quickQuantityText = "1";
    public string QuickQuantityText
    {
        get => _quickQuantityText;
        set
        {
            if (_quickQuantityText != value)
            {
                _quickQuantityText = value;
                OnPropertyChanged();
            }
        }
    }

    private Product _selectedQuickProduct;
    public Product SelectedQuickProduct
    {
        get => _selectedQuickProduct;
        set
        {
            if (_selectedQuickProduct != value)
            {
                _selectedQuickProduct = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedProduct));
                UpdateQuickAddCommandCanExecute();
                
                // When product is selected, set default quantity based on product type
                // For weight-based products, default to 0.5 (500 grams)
                // For unit products, default to 1
                if (value != null)
                {
                    QuickQuantityText = value.IsSoldByWeight 
                        ? Utils.AppConstants.DefaultWeightQuantity.ToString(CultureInfo.InvariantCulture) 
                        : Utils.AppConstants.DefaultUnitQuantity.ToString(CultureInfo.InvariantCulture);
                }
            }
        }
    }

    public bool HasSelectedProduct => SelectedQuickProduct != null;

    private string _selectedCategory = "All";
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterProducts();
            }
        }
    }

    // Notification message for subtle feedback
    private string _notificationMessage = "";
    public string NotificationMessage
    {
        get => _notificationMessage;
        set
        {
            if (_notificationMessage != value)
            {
                _notificationMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNotification));
                OnPropertyChanged(nameof(IsErrorNotification));
            }
        }
    }

    public bool HasNotification => !string.IsNullOrWhiteSpace(NotificationMessage);
    public bool IsErrorNotification => HasNotification && NotificationMessage.StartsWith("⚠️");

    public bool IsAuthenticated => _authService.IsAuthenticated;

    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand CheckoutCommand { get; }

    // Single-step legacy commands (kept for compatibility)
    public ICommand IncreaseQtyCommand { get; }
    public ICommand DecreaseQtyCommand { get; }

    // Multi-step increment / decrement commands for weight-based products
    public ICommand Increase250Command { get; }
    public ICommand Increase100Command { get; }
    public ICommand Increase50Command { get; }
    public ICommand Decrease250Command { get; }
    public ICommand Decrease100Command { get; }
    public ICommand Decrease50Command { get; }

    public ICommand RestockCommand { get; }
    public ICommand LoginCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand QuickAddCommand { get; }
    public ICommand OpenAdminPanelCommand { get; }
    public ICommand OpenDrawerCommand { get; }
    public ICommand SelectCategoryCommand { get; }

    public ShopViewModel(CartService cartService,
                         DatabaseService databaseService,
                         AuthService authService,
                         IServiceProvider serviceProvider,
                         Services.LocalizationService localizationService,
                         Services.IPrintService printService,
                         Services.ICashDrawerService cashDrawerService)
    {
        _cartService = cartService;
        _databaseService = databaseService;
        _authService = authService;
        _serviceProvider = serviceProvider;
        _localizationService = localizationService;
        _printService = printService;
        _cashDrawerService = cashDrawerService;

        AddToCartCommand = new Command<Product>(async product => await AddToCartAsync(product));
        RemoveFromCartCommand = new Command<CartItem>(async item => await RemoveFromCartAsync(item));
        CheckoutCommand = new Command(async () => await CheckoutAsync());

        // Legacy: single-step increase/decrease (maps to 250g)
        IncreaseQtyCommand = new Command<Product>(p => IncreaseBy(p, 0.25m));
        DecreaseQtyCommand = new Command<Product>(p => DecreaseBy(p, 0.25m));

        // New multi-step commands (weight steps are in kilos)
        Increase250Command = new Command<Product>(p => IncreaseBy(p, 0.25m));
        Increase100Command = new Command<Product>(p => IncreaseBy(p, 0.10m));
        Increase50Command = new Command<Product>(p => IncreaseBy(p, 0.05m));
        Decrease250Command = new Command<Product>(p => DecreaseBy(p, 0.25m));
        Decrease100Command = new Command<Product>(p => DecreaseBy(p, 0.10m));
        Decrease50Command = new Command<Product>(p => DecreaseBy(p, 0.05m));

        RestockCommand = new Command<Product>(async product => await RestockAsync(product));
        LoginCommand = new Command(async () => await LoginAsync());
        LogoutCommand = new Command(async () => await LogoutAsync());
        QuickAddCommand = new Command(async () => await QuickAddToCartAsync(), () => !_isPostCheckout && SelectedQuickProduct != null);
        OpenAdminPanelCommand = new Command(async () => await OpenAdminPanel());
        OpenDrawerCommand = new Command(async () => await OpenDrawer());
        SelectCategoryCommand = new Command<string>(category => SelectedCategory = category);

        _cartService.OnCartChanged += UpdateCart;
        _authService.OnUserChanged += OnUserChanged;
        UpdateAuthStatus();
    }

    public async Task InitializeAsync()
    {
        await _databaseService.SeedUsersAsync();
        await _databaseService.SeedProductsAsync();

        var products = await _databaseService.GetProductsAsync();
        
        // Ensure ObservableCollection operations are on the main thread
        if (MainThread.IsMainThread)
        {
            Products.Clear();
            foreach (var product in products)
                Products.Add(product);

            // Update categories
            UpdateCategories();
            
            // Filter products
            FilterProducts();
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Products.Clear();
                foreach (var product in products)
                    Products.Add(product);

                // Update categories
                UpdateCategories();
                
                // Filter products
                FilterProducts();
            });
        }

        await _cartService.InitializeAsync();
        UpdateCart();
    }

    private void UpdateCategories()
    {
        if (!MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(UpdateCategories);
            return;
        }
        
        Categories.Clear();
        Categories.Add("All");
        
        var uniqueCategories = Products
            .Where(p => !string.IsNullOrWhiteSpace(p.Category) && p.Category != "All")
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
        
        foreach (var category in uniqueCategories)
        {
            Categories.Add(category);
        }
    }

    private void FilterProducts()
    {
        // Ensure ObservableCollection operations are on the main thread
        if (!MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(FilterProducts);
            return;
        }
        
        // First filter by category
        var categoryFiltered = Products.AsEnumerable();
        if (SelectedCategory != "All")
        {
            categoryFiltered = categoryFiltered.Where(p => p.Category == SelectedCategory);
        }
        
        FilteredProducts.Clear();
        
        // If no search text, show all products in selected category
        if (string.IsNullOrWhiteSpace(QuickSearchText))
        {
            foreach (var product in categoryFiltered)
                FilteredProducts.Add(product);
            SelectedQuickProduct = null;
            return;
        }

        var searchText = QuickSearchText.Trim();
        
        // Try to parse as ID (numeric)
        if (int.TryParse(searchText, out int searchId))
        {
            var productById = categoryFiltered.FirstOrDefault(p => p.Id == searchId);
            if (productById != null)
            {
                FilteredProducts.Add(productById);
                SelectedQuickProduct = productById;
                return;
            }
        }

        // Search by barcode (exact match first)
        var productByBarcode = categoryFiltered.FirstOrDefault(p => 
            !string.IsNullOrEmpty(p.Barcode) && 
            p.Barcode.Equals(searchText, StringComparison.OrdinalIgnoreCase));
        
        if (productByBarcode != null)
        {
            FilteredProducts.Add(productByBarcode);
            SelectedQuickProduct = productByBarcode;
            return;
        }

        // Search by barcode (contains)
        var searchLower = searchText.ToLowerInvariant();
        foreach (var product in categoryFiltered)
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

        // Auto-select first match if only one result
        if (FilteredProducts.Count == 1)
        {
            SelectedQuickProduct = FilteredProducts[0];
        }
        else if (FilteredProducts.Count > 1)
        {
            // Try to find exact barcode match first
            var exactBarcodeMatch = FilteredProducts.FirstOrDefault(p => 
                !string.IsNullOrEmpty(p.Barcode) &&
                p.Barcode.Equals(searchText, StringComparison.OrdinalIgnoreCase));
            
            if (exactBarcodeMatch != null)
            {
                SelectedQuickProduct = exactBarcodeMatch;
            }
            else
            {
                // Try exact name match
                var exactNameMatch = FilteredProducts.FirstOrDefault(p => 
                    p.Name.Equals(searchText, StringComparison.OrdinalIgnoreCase));
                SelectedQuickProduct = exactNameMatch ?? FilteredProducts[0];
            }
        }
        else
        {
            SelectedQuickProduct = null;
        }
    }

    private async Task QuickAddToCartAsync()
    {
        try
        {
            // If we just finished checkout, ignore this call (prevents error when Enter is pressed after checkout)
            if (_isPostCheckout)
            {
                return;
            }

            // Always clear notification first
            NotificationMessage = "";

            if (SelectedQuickProduct == null)
            {
                ShowNotification("⚠️ Select a product first", true);
                // Set error handling flag to prevent other focus changes
                _isHandlingError = true;
                // Instead of showing error, focus barcode field immediately
                await FocusBarcodeFieldImmediate();
                // Clear flag after a short delay
                _ = Task.Delay(Utils.AppConstants.ErrorHandlingDelayMs).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => _isHandlingError = false));
                return;
            }

            if (string.IsNullOrWhiteSpace(QuickQuantityText) || !decimal.TryParse(QuickQuantityText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal quantity) || quantity <= 0)
            {
                ShowNotification("⚠️ Enter a valid quantity", true);
                // Set error handling flag to prevent other focus changes
                _isHandlingError = true;
                // Instead of showing error, focus barcode field immediately
                await FocusBarcodeFieldImmediate();
                // Clear flag after a short delay
                _ = Task.Delay(Utils.AppConstants.ErrorHandlingDelayMs).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => _isHandlingError = false));
                return;
            }

            var product = SelectedQuickProduct;
            
            // Check stock
            if (quantity > product.Stock)
            {
                var stockUnit = product.IsSoldByWeight ? "KGS" : "PCS";
                QuickQuantityText = product.Stock.ToString();
                ShowNotification($"⚠️ Only {product.Stock} {stockUnit} in stock", true);
                // Set error handling flag to prevent other focus changes
                _isHandlingError = true;
                // Instead of showing error, focus barcode field immediately
                await FocusBarcodeFieldImmediate();
                // Clear flag after a short delay
                _ = Task.Delay(Utils.AppConstants.ErrorHandlingDelayMs).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => _isHandlingError = false));
                return;
            }

            var success = await _cartService.AddToCartAsync(product, quantity);
            if (!success)
            {
                var updatedProduct = await _databaseService.GetProductAsync(product.Id);
                if (updatedProduct != null)
                {
                    product.Stock = updatedProduct.Stock;
                    OnPropertyChanged(nameof(Products));
                }
                ShowNotification("⚠️ Not enough stock available", true);
                // Set error handling flag to prevent other focus changes
                _isHandlingError = true;
                // Instead of showing error, focus barcode field immediately
                await FocusBarcodeFieldImmediate();
                // Clear flag after a short delay
                _ = Task.Delay(Utils.AppConstants.ErrorHandlingDelayMs).ContinueWith(_ => MainThread.BeginInvokeOnMainThread(() => _isHandlingError = false));
                return;
            }

            // Success - show brief success message
            var successUnit = product.IsSoldByWeight ? "KGS" : "PCS";
            ShowNotification($"✅ Added {quantity} {successUnit} {product.Name}", isError: false);

            // Clear search but keep quantity for next item
            QuickSearchText = "";
            // For weight-based products, leave empty for manual entry, for unit products default to 1
            QuickQuantityText = product.IsSoldByWeight 
                ? "" 
                : Utils.AppConstants.DefaultUnitQuantity.ToString(CultureInfo.InvariantCulture);
            SelectedQuickProduct = null;
            
            var refreshedProduct = await _databaseService.GetProductAsync(product.Id);
            if (refreshedProduct != null)
            {
                product.Stock = refreshedProduct.Stock;
                OnPropertyChanged(nameof(Products));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in QuickAddToCart: {ex}");
            ShowNotification("⚠️ An error occurred while adding to cart", true);
        }
    }

    private async Task ShowNotificationAsync(string message, bool isError = false)
    {
        try
        {
            NotificationMessage = message;
            
            // Auto-hide after configured timeout
            await Task.Delay(Utils.AppConstants.NotificationTimeoutMs);
            if (NotificationMessage == message) // Only clear if it hasn't changed
            {
                NotificationMessage = "";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in ShowNotification: {ex}");
            // Silently fail - notification is not critical
        }
    }
    
    // Synchronous wrapper for backward compatibility
    private void ShowNotification(string message, bool isError = false)
    {
        _ = ShowNotificationAsync(message, isError);
    }

    // Generic increase helper
    private void IncreaseBy(Product product, decimal step)
    {
        if (product == null) return;

        if (product.IsSoldByWeight)
        {
            var newQty = product.Quantity + step;
            if (newQty <= product.Stock)
                product.Quantity = newQty;
            else if (product.Quantity < product.Stock)
                product.Quantity = product.Stock;
        }
        else
        {
            // For unit-based products, treat any step as 1
            if (product.Quantity < product.Stock)
                product.Quantity++;
        }
    }

    // Generic decrease helper
    private void DecreaseBy(Product product, decimal step)
    {
        if (product == null) return;

        if (product.IsSoldByWeight)
        {
            product.Quantity = Math.Max(0m, product.Quantity - step);
        }
        else
        {
            if (product.Quantity > 0)
                product.Quantity--;
        }
    }

    private async Task AddToCartAsync(Product product)
    {
        try
        {
            if (product?.Quantity <= 0)
            {
                var unit = product?.IsSoldByWeight == true ? "weight" : "quantity";
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Please select a {unit}", "OK");
                }
                return;
            }

            if (product.Quantity > product.Stock)
            {
                var unit = product.IsSoldByWeight ? "KGS" : "PCS";
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Only {product.Stock} {unit} available in stock", "OK");
                }
                product.Quantity = product.Stock;
                return;
            }

            var success = await _cartService.AddToCartAsync(product, product.Quantity);
            if (!success)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Not enough stock available", "OK");
                }
                var updatedProduct = await _databaseService.GetProductAsync(product.Id);
                if (updatedProduct != null)
                {
                    product.Stock = updatedProduct.Stock;
                    OnPropertyChanged(nameof(Products));
                }
                return;
            }

            product.Quantity = 0;
            var refreshedProduct = await _databaseService.GetProductAsync(product.Id);
            if (refreshedProduct != null)
            {
                product.Stock = refreshedProduct.Stock;
                OnPropertyChanged(nameof(Products));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in AddToCart: {ex}");
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "An error occurred while adding to cart", "OK");
            }
        }
    }

    private async Task RemoveFromCartAsync(CartItem item)
    {
        try
        {
            await _cartService.RemoveFromCartAsync(item);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error removing from cart: {ex}");
            ShowNotification("⚠️ Error removing item from cart", true);
        }
    }

    private async Task CheckoutAsync()
    {
        try
        {
            if (CartItems.Count == 0) return;

            if (_authService.CurrentUser == null)
            {
                ShowNotification("⚠️ Please log in to checkout", true);
                return;
            }

            var itemCount = CartItems.Sum(x => x.Quantity);
            var confirmOrder = _localizationService.GetString("ConfirmOrder");
            var yes = _localizationService.GetString("Yes");
            var no = _localizationService.GetString("No");
            var ok = _localizationService.GetString("OK");
            
            if (Application.Current?.MainPage == null)
            {
                ShowNotification("⚠️ Unable to display confirmation", true);
                return;
            }
            
            bool confirmed = await Application.Current.MainPage.DisplayAlert(
                confirmOrder,
                $"{itemCount} item(s) for ${Total:F2}?\n\nCashier: {CurrentUserName}",
                yes, no);

            if (confirmed)
            {
                // ✨ Pass userId and userName to checkout
                var order = await _cartService.CheckoutAsync(
                    _authService.CurrentUser.Id,
                    _authService.CurrentUser.Name
                );

                if (order == null)
                {
                    ShowNotification("⚠️ Checkout failed. Please try again.", true);
                    return;
                }

                var orderPlaced = _localizationService.GetString("OrderPlaced");
                var printReceipt = "🖨️ Print Receipt";
                var openDrawer = _localizationService.GetString("OpenCashDrawer");
                var cancel = _localizationService.GetString("Cancel");
                
                // Order: Open Drawer first, Print Receipt second
                var action = await Application.Current.MainPage.DisplayActionSheet(
                    "Success",
                    cancel,
                    null,
                    openDrawer,
                    printReceipt,
                    ok);
                
                if (action == openDrawer)
                {
                    await OpenDrawer();
                    // After opening drawer, ensure barcode field is focused and post-checkout flag is set
                    _isPostCheckout = true;
                    UpdateQuickAddCommandCanExecute();
                    OnPropertyChanged(nameof(IsPostCheckout));
                    await FocusBarcodeFieldImmediate();
                }
                else if (action == printReceipt)
                {
                    await PrintReceiptAsync(order);
                    // After printing, ensure barcode field is focused and post-checkout flag is set
                    _isPostCheckout = true;
                    UpdateQuickAddCommandCanExecute();
                    OnPropertyChanged(nameof(IsPostCheckout));
                    await FocusBarcodeFieldImmediate();
                }
                
                await RefreshProductsAsync();
                
                // Clear the flag after a delay to allow normal operation
                _ = Task.Delay(Utils.AppConstants.PostCheckoutResetDelayMs).ContinueWith(_ => 
                {
                    MainThread.BeginInvokeOnMainThread(() => 
                    {
                        _isPostCheckout = false;
                        UpdateQuickAddCommandCanExecute();
                        // Notify that post-checkout state changed so UI can update
                        OnPropertyChanged(nameof(IsPostCheckout));
                    });
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Checkout: {ex}");
            ShowNotification("⚠️ An error occurred during checkout", true);
        }
    }

    private async Task FocusBarcodeFieldImmediate()
    {
        // Clear selection and search text to reset the quick sell interface
        // Do this first to ensure state is cleared
        QuickSearchText = "";
        SelectedQuickProduct = null;
        
        // Use MainThread to ensure UI updates happen on the correct thread
        // Use InvokeOnMainThreadAsync to ensure it completes before returning
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Try to get MainPage and focus the barcode field immediately
            if (Application.Current?.MainPage is MainPage mainPage)
            {
                if (mainPage.FindByName("ProductSearchEntry") is Entry barcodeEntry)
                {
                    // Clear the text explicitly
                    barcodeEntry.Text = "";
                    // Unfocus quantity field if it has focus
                    if (mainPage.FindByName("QuantityEntry") is Entry qtyEntry)
                    {
                        if (qtyEntry.IsFocused)
                        {
                            qtyEntry.Unfocus();
                        }
                    }
                    // Now focus barcode field immediately
                    barcodeEntry.Focus();
                }
            }
        });
    }

    private async Task FocusBarcodeField()
    {
        // Clear selection and search text to reset the quick sell interface
        // Do this first to ensure state is cleared
        QuickSearchText = "";
        SelectedQuickProduct = null;
        
        // Use MainThread to ensure UI updates happen on the correct thread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            // Small delay to ensure any pending events are processed
            await Task.Delay(Utils.AppConstants.FocusDelayMs);
            
            // Try to get MainPage and focus the barcode field
            if (Application.Current?.MainPage is MainPage mainPage)
            {
                if (mainPage.FindByName("ProductSearchEntry") is Entry barcodeEntry)
                {
                    // Clear the text explicitly
                    barcodeEntry.Text = "";
                    // Unfocus quantity field if it has focus
                    if (mainPage.FindByName("QuantityEntry") is Entry qtyEntry && qtyEntry.IsFocused)
                    {
                        qtyEntry.Unfocus();
                        await Task.Delay(Utils.AppConstants.UnfocusDelayMs);
                    }
                    // Now focus barcode field
                    barcodeEntry.Focus();
                }
            }
        });
    }

    private async Task RefreshProductsAsync()
    {
        var products = await _databaseService.GetProductsAsync();
        Products.Clear();
        foreach (var product in products)
            Products.Add(product);
        
        // Update categories
        UpdateCategories();
        
        // Refresh filtered products
        FilterProducts();
        
        OnPropertyChanged(nameof(Products));
    }

    private async Task PrintReceiptAsync(Order order)
    {
        if (order == null) return;

        try
        {
            // Get order items
            var orderItems = await _databaseService.GetOrderItemsAsync(order.Id);
            if (orderItems == null || orderItems.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No items found in order", "OK");
                return;
            }

            // Build receipt text
            var receipt = new StringBuilder();
            receipt.AppendLine("═══════════════════════════");
            receipt.AppendLine("      SWEET SHOP RECEIPT");
            receipt.AppendLine("═══════════════════════════");
            receipt.AppendLine();
            receipt.AppendLine($"Order #: {order.Id}");
            receipt.AppendLine($"Date: {order.OrderDate:yyyy-MM-dd HH:mm:ss}");
            receipt.AppendLine($"Cashier: {order.UserName}");
            receipt.AppendLine();
            receipt.AppendLine("───────────────────────────");
            receipt.AppendLine("ITEMS:");
            receipt.AppendLine("───────────────────────────");
            
            foreach (var item in orderItems)
            {
                receipt.AppendLine($"{item.Emoji} {item.Name}");
                receipt.AppendLine($"  {item.Quantity:F2} {item.UnitLabel} × ${item.Price:F2} = ${item.ItemTotal:F2}");
            }
            
            receipt.AppendLine("───────────────────────────");
            receipt.AppendLine($"TOTAL: ${order.Total:F2}");
            receipt.AppendLine("═══════════════════════════");
            receipt.AppendLine();
            receipt.AppendLine("Thank you for your purchase!");
            receipt.AppendLine("═══════════════════════════");

            // Print the receipt
            var success = await _printService.PrintReceiptAsync(receipt.ToString(), $"Receipt - Order #{order.Id}");
            if (!success && Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "Failed to open print dialog. Please try again.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Failed to print receipt: {ex.Message}", "OK");
        }
    }

    private async Task RestockAsync(Product product)
    {
        try
        {
            if (product == null) return;
            
            if (!_authService.CanRestock)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        _localizationService.GetString("AccessDenied"),
                        _localizationService.GetString("YouDontHavePermissionToRestock"), 
                        _localizationService.GetString("OK"));
                }
                return;
            }

            var message = string.Format(_localizationService.GetString("EnterQuantityToAdd"), product.Name, product.Stock)
                .Replace("\\n", Environment.NewLine)
                .Replace("&#10;", Environment.NewLine);
            
            if (Application.Current?.MainPage == null) return;
            
            string result = await Application.Current.MainPage.DisplayPromptAsync(
                _localizationService.GetString("RestockProduct"),
                message,
                _localizationService.GetString("Add"),
                _localizationService.GetString("Cancel"),
                "0",
                keyboard: Microsoft.Maui.Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(result) || !decimal.TryParse(result, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal quantity) || quantity <= 0)
                return;

            var stockBefore = product.Stock;
            var success = await _databaseService.UpdateProductStockAsync(product.Id, quantity);
            if (success)
            {
                var updatedProduct = await _databaseService.GetProductAsync(product.Id);
                var newStock = updatedProduct?.Stock ?? (product.Stock + quantity);

                // Create restock record
                var restockRecord = new RestockRecord
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductEmoji = product.Emoji,
                    QuantityAdded = quantity,
                    StockBefore = stockBefore,
                    StockAfter = newStock,
                    UserId = _authService.CurrentUser?.Id ?? 0,
                    UserName = _authService.CurrentUser?.Name ?? "Unknown",
                    RestockDate = DateTime.UtcNow
                };
                await _databaseService.CreateRestockRecordAsync(restockRecord);

                var successMessage = string.Format(_localizationService.GetString("AddedItemsToProduct"), quantity, product.Name, newStock)
                    .Replace("\\n", Environment.NewLine)
                    .Replace("&#10;", Environment.NewLine);
                
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        _localizationService.GetString("Success"),
                        successMessage, 
                        _localizationService.GetString("OK"));
                }

                await RefreshProductsAsync();
            }
            else
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        _localizationService.GetString("Error"), 
                        _localizationService.GetString("FailedToRestockProduct"), 
                        _localizationService.GetString("OK"));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Restock: {ex}");
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    _localizationService.GetString("Error"), 
                    "An error occurred while restocking the product", 
                    _localizationService.GetString("OK"));
            }
        }
    }

    //private async void Login()
    //{
    //    string username = await Application.Current.MainPage.DisplayPromptAsync(
    //        "Login",
    //        "Enter username:",
    //        "Next",
    //        "Cancel",
    //        "");

    //    if (string.IsNullOrWhiteSpace(username)) return;

    //    string password = await Application.Current.MainPage.DisplayPromptAsync(
    //        "Login",
    //        "Enter password:",
    //        "Login",
    //        "Cancel",
    //        "",
    //        keyboard: Microsoft.Maui.Keyboard.Default);

    //    if (string.IsNullOrWhiteSpace(password)) return;

    //    var success = await _authService.LoginAsync(username, password);
    //    if (success)
    //        await Application.Current.MainPage.DisplayAlert("Success", $"Welcome, {_authService.CurrentUser.Name}!", "OK");
    //    else
    //        await Application.Current.MainPage.DisplayAlert("Error", "Invalid username or password", "OK");
    //}

    private async Task LoginAsync()
    {
        try
        {
            // Navigate to login page - create new instance with injected services
            var loginPage = new Views.LoginPage(_authService, _databaseService, _localizationService);
            await Shell.Current.Navigation.PushAsync(loginPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to login: {ex}");
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            _authService.Logout();
            
            // Navigate to login page - create new instance with injected services
            var loginPage = new Views.LoginPage(_authService, _databaseService, _localizationService);
            await Shell.Current.Navigation.PushAsync(loginPage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during logout: {ex}");
        }
    }

    private void OnUserChanged(User user) => UpdateAuthStatus();

    public void RefreshAuthStatus()
    {
        UpdateAuthStatus();
    }

    private void UpdateAuthStatus()
    {
        IsAdmin = _authService.IsAdmin;
        CurrentUserName = _authService.CurrentUser?.Name ?? "";
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(CanRestock));
        OnPropertyChanged(nameof(CanManageStock));
        OnPropertyChanged(nameof(CanAccessAdminPanel));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(IsAuthenticated));
    }

    /// <summary>
    /// Updates the cart display when cart contents change.
    /// No longer needs to rebuild collection since CartItem now properly implements INotifyPropertyChanged.
    /// </summary>
    private void UpdateCart()
    {
        var currentItems = _cartService.GetCartItems();
        
        // Ensure ObservableCollection operations are on the main thread
        if (!MainThread.IsMainThread)
        {
            MainThread.BeginInvokeOnMainThread(UpdateCart);
            return;
        }
        
        // Sync CartItems collection with cart service items
        // Only update if items have changed (not just quantity updates)
        var existingIds = new HashSet<int>(CartItems.Select(x => x.Id));
        var currentIds = new HashSet<int>(currentItems.Select(x => x.Id));
        
        // Remove items no longer in cart
        foreach (var item in CartItems.Where(x => !currentIds.Contains(x.Id)).ToList())
        {
            CartItems.Remove(item);
        }
        
        // Add new items to cart
        foreach (var item in currentItems.Where(x => !existingIds.Contains(x.Id)))
        {
            CartItems.Add(item);
        }
        
        // Note: Quantity/Price updates are handled by CartItem's PropertyChanged events
        // So the UI will update in real-time without needing to rebuild the collection
        
        Total = _cartService.GetTotal();
        IsCheckoutEnabled = CartItems.Count > 0;
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(IsCheckoutEnabled));
    }

    private async Task OpenAdminPanel()
    {
        if (!CanAccessAdminPanel)
        {
            var accessDenied = _localizationService.GetString("AccessDenied");
            var onlyAdmins = _localizationService.GetString("OnlyAdminsCanOpenAdminPanel");
            var ok = _localizationService.GetString("OK");
            await Application.Current.MainPage.DisplayAlert(accessDenied, onlyAdmins, ok);
            return;
        }

        var adminPage = _serviceProvider.GetService<Views.AdminPage>();
        if (adminPage != null)
        {
            await Shell.Current.Navigation.PushAsync(adminPage);
        }
    }

    private async Task OpenDrawer()
    {
        try
        {
            var success = await _cashDrawerService.OpenDrawerAsync();
            if (!success)
            {
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "Failed to open cash drawer. Please check your printer connection.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                $"Failed to open cash drawer: {ex.Message}", "OK");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
