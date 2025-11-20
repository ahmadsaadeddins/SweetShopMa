using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.Services;
using SweetShopMa.ViewModels;
#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
#endif

namespace SweetShopMa.Views;

public partial class ProductsPage : ContentPage
{
    private readonly AdminViewModel _viewModel;
    private readonly LocalizationService _localizationService;
    private readonly IServiceProvider _serviceProvider;

    public ProductsPage(AdminViewModel viewModel, LocalizationService localizationService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _localizationService = localizationService;
        _serviceProvider = serviceProvider;
        BindingContext = _viewModel;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        // Watch for when product editing is cancelled (after update)
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private async void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // When IsEditingProduct becomes false (after update), focus search field
        if (e.PropertyName == nameof(AdminViewModel.IsEditingProduct) && !_viewModel.IsEditingProduct)
        {
            // Small delay to ensure UI has updated
            await Task.Delay(200);
            await FocusSearchField();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        UpdateLocalizedStrings();
        UpdateRTL();

        if (!_viewModel.IsAuthorized)
        {
            var accessDenied = _localizationService.GetString("AccessDenied");
            var adminRequired = _localizationService.GetString("AdminPrivilegesRequired");
            var ok = _localizationService.GetString("OK");
            await DisplayAlert(accessDenied, adminRequired, ok);
            await Shell.Current.Navigation.PopAsync();
            return;
        }

        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Unsubscribe from property changes when page disappears
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("ProductManagement") ?? "Product Management";
        if (PageTitleLabel != null)
            PageTitleLabel.Text = _localizationService.GetString("ProductManagement") ?? "Product Management";
        if (BackButton != null)
            BackButton.Text = _localizationService.GetString("BackButton");
        if (AddProductLabel != null)
            AddProductLabel.Text = _localizationService.GetString("AddProduct");
        if (AddProductButton != null)
            AddProductButton.Text = _localizationService.GetString("AddProduct");
        if (ProductsLabel != null)
            ProductsLabel.Text = _localizationService.GetString("Products");
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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (Navigation?.NavigationStack?.Count > 1)
        {
            await Navigation.PopAsync();
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnProductSearchCompleted(object sender, EventArgs e)
    {
        if (BindingContext is AdminViewModel viewModel)
        {
            // Get the first product from filtered results
            if (viewModel.FilteredProducts != null && viewModel.FilteredProducts.Count > 0)
            {
                var firstProduct = viewModel.FilteredProducts[0];
                // Trigger edit command for the first product
                if (viewModel.EditProductCommand.CanExecute(firstProduct))
                {
                    viewModel.EditProductCommand.Execute(firstProduct);
                    
                    // Wait a bit for the form to populate, then focus and select the name field
                    await Task.Delay(150);
                    
                    if (EditProductNameEntry != null)
                    {
                        // Focus the name field
                        EditProductNameEntry.Focus();
                        
                        // Select all text using platform-specific approach
                        if (!string.IsNullOrEmpty(EditProductNameEntry.Text))
                        {
                            await Task.Delay(50);
                            SelectAllText(EditProductNameEntry);
                        }
                    }
                }
            }
        }
    }

    private async void OnEditNameCompleted(object sender, EventArgs e)
    {
        await MoveToNextFieldAndSelect(EditProductEmojiEntry);
    }

    private async void OnEditEmojiCompleted(object sender, EventArgs e)
    {
        await MoveToNextFieldAndSelect(EditProductCategoryEntry);
    }

    private async void OnEditCategoryCompleted(object sender, EventArgs e)
    {
        await MoveToNextFieldAndSelect(EditProductPriceEntry);
    }

    private async void OnEditPriceCompleted(object sender, EventArgs e)
    {
        // When Enter is pressed in Price field, trigger Update command
        if (BindingContext is AdminViewModel viewModel)
        {
            if (viewModel.UpdateProductCommand.CanExecute(null))
            {
                viewModel.UpdateProductCommand.Execute(null);
                // Focus will be handled by PropertyChanged event when IsEditingProduct becomes false
            }
        }
    }

    private async Task FocusSearchField()
    {
        if (ProductSearchEntry != null)
        {
            // Clear the search field
            ProductSearchEntry.Text = "";
            
            // Focus the search field
            await Task.Delay(50);
            ProductSearchEntry.Focus();
        }
    }

    private async Task MoveToNextFieldAndSelect(Entry nextEntry)
    {
        if (nextEntry == null) return;

        // Small delay to ensure current field processing is complete
        await Task.Delay(50);
        
        // Focus the next field
        nextEntry.Focus();
        
        // Select all text in the next field
        if (!string.IsNullOrEmpty(nextEntry.Text))
        {
            await Task.Delay(50);
            SelectAllText(nextEntry);
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
                var platformView = handler.PlatformView as TextBox;
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
}

