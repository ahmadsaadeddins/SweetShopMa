using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.ViewModels;
using SweetShopMa.Services;

namespace SweetShopMa.Views;

public partial class MainPage : ContentPage
{
    private readonly LocalizationService _localizationService;

    public MainPage(ShopViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _localizationService = localizationService;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        // Set up keyboard navigation
        SetupKeyboardNavigation();
        
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("AppTitle");
        if (AppTitleLabel != null)
            AppTitleLabel.Text = _localizationService.GetString("AppTitle");
        if (AppSubtitleLabel != null)
            AppSubtitleLabel.Text = _localizationService.GetString("AppSubtitle");
        if (LoginButton != null)
            LoginButton.Text = _localizationService.GetString("LoginButton");
        if (AdminPanelButton != null)
            AdminPanelButton.Text = _localizationService.GetString("AdminPanel");
        if (LogoutButton != null)
            LogoutButton.Text = _localizationService.GetString("Logout");
        if (ScanBarcodeLabel != null)
            ScanBarcodeLabel.Text = _localizationService.GetString("ScanBarcode");
        if (ProductSearchEntry != null)
            ProductSearchEntry.Placeholder = _localizationService.GetString("BarcodePlaceholder");
        if (QuantityLabel != null)
        {
            if (BindingContext is ShopViewModel viewModel && viewModel.SelectedQuickProduct != null)
            {
                var qtyFormat = _localizationService.GetString("Quantity");
                QuantityLabel.Text = $"{qtyFormat} ({viewModel.SelectedQuickProduct.UnitLabel})";
            }
            else
            {
                QuantityLabel.Text = _localizationService.GetString("Quantity");
            }
        }
        if (QuickAddButton != null)
            QuickAddButton.Text = _localizationService.GetString("Add");
        if (CartLabel != null)
            CartLabel.Text = _localizationService.GetString("Cart");
        if (TotalLabel != null)
            TotalLabel.Text = _localizationService.GetString("Total") + ":";
        if (CheckoutButton != null)
            CheckoutButton.Text = _localizationService.GetString("Checkout");
        if (OpenDrawerButton != null)
            OpenDrawerButton.Text = _localizationService.GetString("OpenDrawer");
        
        // Update LoggedInAs label
        if (LoggedInAsLabel != null && BindingContext is ShopViewModel vm)
        {
            var loggedInAsFormat = _localizationService.GetString("LoggedInAs");
            LoggedInAsLabel.Text = string.Format(loggedInAsFormat, vm.CurrentUserName);
        }
    }

    private void UpdateRTL()
    {
        FlowDirection = _localizationService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        var currentLang = _localizationService.CurrentLanguage;
        var newLang = currentLang == "en" ? "ar" : "en";
        _localizationService.SetLanguage(newLang);
    }

    private void SetupKeyboardNavigation()
    {
        // When product search entry gets focus, select all text for quick replacement
        if (ProductSearchEntry != null)
        {
            ProductSearchEntry.Focused += (s, e) =>
            {
                if (ProductSearchEntry.Text?.Length > 0)
                {
                    ProductSearchEntry.CursorPosition = ProductSearchEntry.Text.Length;
                }
            };
        }

        // When quantity entry gets focus, select all text
        if (QuantityEntry != null)
        {
            QuantityEntry.Focused += (s, e) =>
            {
                if (QuantityEntry.Text?.Length > 0)
                {
                    QuantityEntry.CursorPosition = QuantityEntry.Text.Length;
                }
            };
        }
    }

    private void OnBarcodeEntryCompleted(object sender, EventArgs e)
    {
        // When Enter is pressed in barcode field, move focus to quantity field
        if (QuantityEntry != null)
        {
            QuantityEntry.Focus();
        }
    }

    private async void OnQuantityEntryCompleted(object sender, EventArgs e)
    {
        // When Enter is pressed in quantity field, add to cart and refocus barcode field
        await AddToCartAndRefocusBarcode();
    }

    private async Task AddToCartAndRefocusBarcode()
    {
        if (BindingContext is ShopViewModel viewModel)
        {
            // Execute the quick add command
            if (viewModel.QuickAddCommand.CanExecute(null))
            {
                viewModel.QuickAddCommand.Execute(null);
                
                // Wait for command to complete, then refocus barcode field
                await Task.Delay(250);
                
                // Refocus on main thread - always go back to barcode field
                if (ProductSearchEntry != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Unfocus quantity field first
                        if (QuantityEntry != null && QuantityEntry.IsFocused)
                        {
                            QuantityEntry.Unfocus();
                        }
                        
                        // Clear and focus barcode field
                        ProductSearchEntry.Text = "";
                        ProductSearchEntry.Focus();
                    });
                }
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Update localized strings when page appears
        UpdateLocalizedStrings();
        UpdateRTL();
        
        if (BindingContext is ShopViewModel viewModel)
        {
            // Get services from Handler.MauiContext
            var databaseService = Handler?.MauiContext?.Services.GetService<DatabaseService>();
            var authService = Handler?.MauiContext?.Services.GetService<AuthService>();
            
            // Always refresh auth status when page appears
            viewModel.RefreshAuthStatus();
            
            // Check if user is authenticated - if not, redirect to login
            if (!viewModel.IsAuthenticated)
            {
                // User not logged in, navigate to login
                await Shell.Current.GoToAsync("//login");
                return;
            }
            
            // Display database path in debug output
            System.Diagnostics.Debug.WriteLine($"Database Path: {DatabaseService.DatabasePath}");
            System.Diagnostics.Debug.WriteLine($"App Data Directory: {DatabaseService.AppDataDirectory}");
            
            await viewModel.InitializeAsync();
            
            // Auto-focus product search for quick entry
            if (ProductSearchEntry != null)
            {
                ProductSearchEntry.Focus();
            }
        }
    }
}

