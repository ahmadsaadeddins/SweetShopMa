using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SweetShopMa.Models;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

/// <summary>
/// Main view model for the shopping interface.
/// Manages products, cart, search, and user authentication.
/// </summary>
public class ShopViewModel : INotifyPropertyChanged
{
    private readonly CartService _cartService;
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    
    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<CartItem> CartItems { get; } = new();
    public ObservableCollection<Product> FilteredProducts { get; } = new();

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
            } 
        }
    }

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
                
                // When product is selected, set default quantity based on product type
                // For weight-based products, leave empty for manual entry
                // For unit products, default to 1
                if (value != null)
                {
                    QuickQuantityText = value.IsSoldByWeight ? "" : "1";
                }
            }
        }
    }

    public bool HasSelectedProduct => SelectedQuickProduct != null;

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

    public ShopViewModel(CartService cartService,
                         DatabaseService databaseService,
                         AuthService authService,
                         IServiceProvider serviceProvider)
    {
        _cartService = cartService;
        _databaseService = databaseService;
        _authService = authService;
        _serviceProvider = serviceProvider;

        AddToCartCommand = new Command<Product>(AddToCart);
        RemoveFromCartCommand = new Command<CartItem>(RemoveFromCart);
        CheckoutCommand = new Command(Checkout);

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

        RestockCommand = new Command<Product>(Restock);
        LoginCommand = new Command(Login);
        LogoutCommand = new Command(Logout);
        QuickAddCommand = new Command(QuickAddToCart);
        OpenAdminPanelCommand = new Command(async () => await OpenAdminPanel());

        _cartService.OnCartChanged += UpdateCart;
        _authService.OnUserChanged += OnUserChanged;
        UpdateAuthStatus();
    }

    public async Task InitializeAsync()
    {
        await _databaseService.SeedUsersAsync();
        await _databaseService.SeedProductsAsync();

        var products = await _databaseService.GetProductsAsync();
        Products.Clear();
        foreach (var product in products)
            Products.Add(product);

        FilteredProducts.Clear();
        foreach (var product in products)
            FilteredProducts.Add(product);

        await _cartService.InitializeAsync();
        UpdateCart();
    }

    private void FilterProducts()
    {
        FilteredProducts.Clear();
        if (string.IsNullOrWhiteSpace(QuickSearchText))
        {
            foreach (var product in Products)
                FilteredProducts.Add(product);
            SelectedQuickProduct = null;
            return;
        }

        var searchText = QuickSearchText.Trim();
        
        // Try to parse as ID (numeric)
        if (int.TryParse(searchText, out int searchId))
        {
            var productById = Products.FirstOrDefault(p => p.Id == searchId);
            if (productById != null)
            {
                FilteredProducts.Add(productById);
                SelectedQuickProduct = productById;
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
            SelectedQuickProduct = productByBarcode;
            return;
        }

        // Search by barcode (contains)
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

    private async void QuickAddToCart()
    {
        // Always clear notification first
        NotificationMessage = "";

        if (SelectedQuickProduct == null)
        {
            ShowNotification("⚠️ Please select a product", isError: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(QuickQuantityText) || !decimal.TryParse(QuickQuantityText, out decimal quantity) || quantity <= 0)
        {
            ShowNotification("⚠️ Please enter a valid quantity", isError: true);
            return;
        }

        var product = SelectedQuickProduct;
        
        // Check stock
        if (quantity > product.Stock)
        {
            var stockUnit = product.IsSoldByWeight ? "kg" : "items";
            ShowNotification($"⚠️ Only {product.Stock} {stockUnit} in stock", isError: true);
            QuickQuantityText = product.Stock.ToString();
            return;
        }

        var success = await _cartService.AddToCartAsync(product, quantity);
        if (!success)
        {
            ShowNotification("⚠️ Not enough stock available", isError: true);
            var updatedProduct = await _databaseService.GetProductAsync(product.Id);
            if (updatedProduct != null)
            {
                product.Stock = updatedProduct.Stock;
                OnPropertyChanged(nameof(Products));
            }
            return;
        }

        // Success - show brief success message
        var successUnit = product.IsSoldByWeight ? "kg" : "pcs";
        ShowNotification($"✅ Added {quantity} {successUnit} {product.Name}", isError: false);

        // Clear search but keep quantity for next item
        QuickSearchText = "";
        // For weight-based products, leave empty for manual entry, for unit products default to 1
        QuickQuantityText = product.IsSoldByWeight ? "" : "1";
        SelectedQuickProduct = null;
        
        var refreshedProduct = await _databaseService.GetProductAsync(product.Id);
        if (refreshedProduct != null)
        {
            product.Stock = refreshedProduct.Stock;
            OnPropertyChanged(nameof(Products));
        }
    }

    private async void ShowNotification(string message, bool isError = false)
    {
        NotificationMessage = message;
        
        // Auto-hide after 2 seconds
        await Task.Delay(2000);
        if (NotificationMessage == message) // Only clear if it hasn't changed
        {
            NotificationMessage = "";
        }
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

    private async void AddToCart(Product product)
    {
        if (product.Quantity <= 0)
        {
            var unit = product.IsSoldByWeight ? "weight" : "quantity";
            await Application.Current.MainPage.DisplayAlert("Error", $"Please select a {unit}", "OK");
            return;
        }

        if (product.Quantity > product.Stock)
        {
            var unit = product.IsSoldByWeight ? "kg" : "items";
            await Application.Current.MainPage.DisplayAlert("Error", $"Only {product.Stock} {unit} available in stock", "OK");
            product.Quantity = product.Stock;
            return;
        }

        var success = await _cartService.AddToCartAsync(product, product.Quantity);
        if (!success)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Not enough stock available", "OK");
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

    private async void RemoveFromCart(CartItem item) => await _cartService.RemoveFromCartAsync(item);

    private async void Checkout()
    {
        if (CartItems.Count == 0) return;

        var itemCount = CartItems.Sum(x => x.Quantity);
        bool confirmed = await Application.Current.MainPage.DisplayAlert(
            "Confirm Order",
            $"{itemCount} item(s) for ${Total:F2}?\n\nCashier: {CurrentUserName}",  // ← Show who's checking out
            "Yes", "No");

        if (confirmed)
        {
            // ✨ Pass userId and userName to checkout
            var order = await _cartService.CheckoutAsync(
                _authService.CurrentUser.Id,
                _authService.CurrentUser.Name
            );

            if (order != null)
            {
                await Application.Current.MainPage.DisplayAlert("Success",
                    $"Order #{order.Id} placed!\nCashier: {order.UserName}\nTotal: ${order.Total:F2}", "OK");
                await RefreshProductsAsync();
            }
        }
    }

    private async Task RefreshProductsAsync()
    {
        var products = await _databaseService.GetProductsAsync();
        Products.Clear();
        foreach (var product in products)
            Products.Add(product);
        
        // Refresh filtered products
        FilterProducts();
        
        OnPropertyChanged(nameof(Products));
    }

    private async void Restock(Product product)
    {
        if (product == null) return;
        if (!_authService.IsAdmin)
        {
            await Application.Current.MainPage.DisplayAlert("Access Denied",
                "Only administrators can restock products. Please login as admin.", "OK");
            return;
        }

        string result = await Application.Current.MainPage.DisplayPromptAsync(
            "Restock Product",
            $"Enter quantity to add to {product.Name}:\nCurrent Stock: {product.Stock}",
            "Add",
            "Cancel",
            "0",
            keyboard: Microsoft.Maui.Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(result) || !decimal.TryParse(result, out decimal quantity) || quantity <= 0)
            return;

        var success = await _databaseService.UpdateProductStockAsync(product.Id, quantity);
        if (success)
        {
            var updatedProduct = await _databaseService.GetProductAsync(product.Id);
            var newStock = updatedProduct?.Stock ?? (product.Stock + quantity);

            await Application.Current.MainPage.DisplayAlert("Success",
                $"Added {quantity} items to {product.Name}\nNew Stock: {newStock}", "OK");

            await RefreshProductsAsync();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Failed to restock product", "OK");
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

    private async void Login()
    {
        // Navigate to login page - create new instance with injected AuthService
        var loginPage = new Views.LoginPage(_authService);
        await Shell.Current.Navigation.PushAsync(loginPage);
    }

    private async void Logout()
    {
        _authService.Logout();
        
        // Navigate to login page - create new instance with injected AuthService
        var loginPage = new Views.LoginPage(_authService);
        await Shell.Current.Navigation.PushAsync(loginPage);
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
        if (!_authService.IsAdmin)
        {
            await Application.Current.MainPage.DisplayAlert("Access Denied",
                "Only administrators can open the admin panel.", "OK");
            return;
        }

        var adminPage = _serviceProvider.GetService<Views.AdminPage>();
        if (adminPage != null)
        {
            await Shell.Current.Navigation.PushAsync(adminPage);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
