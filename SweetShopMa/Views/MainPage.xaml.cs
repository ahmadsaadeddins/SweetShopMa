using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.ViewModels;
using SweetShopMa.Services;

namespace SweetShopMa.Views;

public partial class MainPage : ContentPage
{
    public MainPage(ShopViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Set up keyboard navigation
        SetupKeyboardNavigation();
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
        
        if (BindingContext is ShopViewModel viewModel)
        {
            // Always refresh auth status when page appears
            viewModel.RefreshAuthStatus();
            
            // Check if user is authenticated - if not, redirect to login
            if (!viewModel.IsAuthenticated)
            {
                // User not logged in, navigate to login
                // Get AuthService from Handler.MauiContext
                var authService = Handler?.MauiContext?.Services.GetService<AuthService>();
                if (authService != null)
                {
                    var loginPage = new Views.LoginPage(authService);
                    await Shell.Current.Navigation.PushAsync(loginPage);
                }
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

