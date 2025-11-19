using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.ViewModels;
using SweetShopMa.Services;
#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Input;
using Windows.System;
#endif

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
        SetupKeyboardShortcuts();
        
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
        if (QuickCheckoutButton != null)
            QuickCheckoutButton.Text = _localizationService.GetString("Checkout") + " (F1)";
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

        // Note: QuantityEntry Focused event is now handled in OnQuantityEntryFocused method
    }

    private void SetupKeyboardShortcuts()
    {
        // Set up keyboard shortcuts for quick checkout
#if WINDOWS
        // On Windows, handle KeyDown at the page level
        this.Loaded += OnPageLoaded;
#endif
    }

    private void OnPageLoaded(object sender, EventArgs e)
    {
#if WINDOWS
        // Set up F1 key handler for quick checkout
        if (Handler?.PlatformView != null)
        {
            var platformView = Handler.PlatformView as Microsoft.UI.Xaml.Controls.Page;
            if (platformView != null)
            {
                platformView.KeyDown += OnPageKeyDown;
            }
        }
#endif
    }

#if WINDOWS
    private void OnPageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == VirtualKey.F1)
        {
            if (BindingContext is ShopViewModel viewModel && viewModel.IsCheckoutEnabled)
            {
                if (viewModel.CheckoutCommand.CanExecute(null))
                {
                    viewModel.CheckoutCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
#endif

    private void OnBarcodeEntryCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShopViewModel viewModel)
        {
            // If barcode field is empty, ALWAYS keep focus in barcode field
            // This handles all cases: after checkout, after drawer, empty field, etc.
            if (string.IsNullOrWhiteSpace(ProductSearchEntry?.Text))
            {
                // If cart has items, trigger quick checkout
                if (viewModel.CartItems.Count > 0)
                {
                    if (viewModel.CheckoutCommand.CanExecute(null))
                    {
                        viewModel.CheckoutCommand.Execute(null);
                    }
                    return;
                }
                
                // Otherwise, just keep focus in barcode field - use BeginInvoke to ensure it happens after any automatic focus changes
                if (ProductSearchEntry != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Unfocus quantity entry if it somehow got focus
                        if (QuantityEntry != null && QuantityEntry.IsFocused)
                        {
                            QuantityEntry.Unfocus();
                        }
                        // Focus barcode field
                        ProductSearchEntry.Focus();
                    });
                }
                return;
            }
            
            // If barcode field has text, move to quantity field (normal flow) - only if product is selected
            if (QuantityEntry != null && viewModel.SelectedQuickProduct != null)
            {
                QuantityEntry.Focus();
            }
            else
            {
                // No product selected, keep focus in barcode
                if (ProductSearchEntry != null)
                {
                    ProductSearchEntry.Focus();
                }
            }
        }
    }

    private async void OnQuantityEntryFocused(object sender, FocusEventArgs e)
    {
        // If we're in post-checkout state, immediately unfocus and move to barcode
        if (BindingContext is ShopViewModel viewModel && viewModel.IsPostCheckout)
        {
            if (ProductSearchEntry != null)
            {
                ProductSearchEntry.Focus();
            }
            return;
        }
        
        // When quantity entry gets focus, select all text for easy replacement
        if (QuantityEntry != null && QuantityEntry.Text?.Length > 0)
        {
            // Use a small delay to ensure the entry is fully focused
            await Task.Delay(100);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Select all text by positioning cursor at start
                // On most platforms, this will allow typing to replace
                QuantityEntry.CursorPosition = 0;
                
                // Try to select all text using platform-specific approach
                SelectAllText(QuantityEntry);
            });
        }
    }

    private void SelectAllText(Entry entry)
    {
        if (entry == null || string.IsNullOrEmpty(entry.Text)) return;

#if WINDOWS
        // On Windows, use platform-specific code to select all
        try
        {
            var handler = entry.Handler;
            if (handler?.PlatformView != null)
            {
                var platformView = handler.PlatformView as Microsoft.UI.Xaml.Controls.TextBox;
                if (platformView != null)
                {
                    platformView.SelectAll();
                }
            }
        }
        catch
        {
            // Fallback: just position cursor at start
            entry.CursorPosition = 0;
        }
#else
        // For other platforms, position cursor at start
        // User can manually select all or just type to replace
        entry.CursorPosition = 0;
#endif
    }

    private async void OnQuantityEntryCompleted(object sender, EventArgs e)
    {
        if (BindingContext is ShopViewModel viewModel)
        {
            // If we're in post-checkout state, don't process this and move focus back to barcode
            if (viewModel.IsPostCheckout)
            {
                if (ProductSearchEntry != null)
                {
                    ProductSearchEntry.Focus();
                }
                return;
            }
            
            // This prevents the "please select a product" error when Enter is pressed after checkout
            if (viewModel.QuickAddCommand.CanExecute(null))
            {
                // When Enter is pressed in quantity field, add to cart and refocus barcode field
                await AddToCartAndRefocusBarcode();
            }
        }
    }

    private async Task AddToCartAndRefocusBarcode()
    {
        if (BindingContext is ShopViewModel viewModel)
        {
            // Execute the quick add command
            if (viewModel.QuickAddCommand.CanExecute(null))
            {
                var hadProduct = viewModel.SelectedQuickProduct != null;
                viewModel.QuickAddCommand.Execute(null);
                
                // Wait a bit for command to process
                await Task.Delay(150);
                
                // If we're handling an error, don't refocus - FocusBarcodeFieldImmediate already handled it
                if (viewModel.IsHandlingError)
                {
                    return;
                }
                
                // If we had a product but now it's null and search is empty, command succeeded
                if (hadProduct && viewModel.SelectedQuickProduct == null && string.IsNullOrWhiteSpace(viewModel.QuickSearchText))
                {
                    // Command succeeded, refocus barcode field
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
                // If command failed, FocusBarcodeFieldImmediate already handled focus
            }
            else
            {
                // Command can't execute, focus barcode field
                if (ProductSearchEntry != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (QuantityEntry != null && QuantityEntry.IsFocused)
                        {
                            QuantityEntry.Unfocus();
                        }
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

