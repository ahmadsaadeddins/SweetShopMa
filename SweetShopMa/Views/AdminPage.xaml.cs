using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using SweetShopMa.Services;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class AdminPage : ContentPage
{
    private readonly AdminViewModel _viewModel;
    private readonly LocalizationService _localizationService;
    private readonly IServiceProvider _serviceProvider;

    public AdminPage(AdminViewModel viewModel, LocalizationService localizationService, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _localizationService = localizationService;
        _serviceProvider = serviceProvider;
        BindingContext = _viewModel;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedStrings();
        UpdateRTL();
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

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("AdministratorPanel");
        if (AdminPanelTitleLabel != null)
            AdminPanelTitleLabel.Text = _localizationService.GetString("AdministratorPanel");
        if (AdminBackButton != null)
            AdminBackButton.Text = _localizationService.GetString("BackButton");
        // DatabaseInfoLabel is now hidden/removed, so we don't update it
        if (UsersButtonLabel != null)
            UsersButtonLabel.Text = _localizationService.GetString("UserManagement") ?? "User Management";
        if (ProductsButtonLabel != null)
            ProductsButtonLabel.Text = _localizationService.GetString("ProductManagement") ?? "Product Management";
        if (AttendanceButtonLabel != null)
            AttendanceButtonLabel.Text = _localizationService.GetString("AttendanceTracker") ?? "Attendance Tracker";
        if (RestockReportButtonLabel != null)
            RestockReportButtonLabel.Text = _localizationService.GetString("RestockReport") ?? "Restock Report";
        if (ReportsLabel != null)
            ReportsLabel.Text = _localizationService.GetString("ReportsInsights");
        if (TopProductsLabel != null)
            TopProductsLabel.Text = _localizationService.GetString("TopProducts");
        if (RecentOrdersLabel != null)
            RecentOrdersLabel.Text = _localizationService.GetString("RecentOrders");
        if (TotalSalesLabel != null)
            TotalSalesLabel.Text = _localizationService.GetString("TotalSales");
        if (OrdersLabel != null)
            OrdersLabel.Text = _localizationService.GetString("Orders");
        if (ItemsSoldLabel != null)
            ItemsSoldLabel.Text = _localizationService.GetString("ItemsSold");
        if (Last7DaysLabel != null)
            Last7DaysLabel.Text = _localizationService.GetString("Last7Days");
        if (TopProductLabel != null)
            TopProductLabel.Text = _localizationService.GetString("TopProduct");
        if (ReportStatusLabel != null)
            ReportStatusLabel.Text = _localizationService.GetString("ReportStatus");
        
        // Update ViewModel properties that depend on localization
        if (BindingContext is AdminViewModel adminViewModel)
        {
            adminViewModel.RefreshLocalizedProperties();
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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (Navigation?.NavigationStack?.Count > 1)
        {
            await Navigation.PopAsync();
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnCreateUserClicked(object sender, EventArgs e)
    {
        var developerSetupPage = _serviceProvider.GetService<DeveloperSetupPage>();
        if (developerSetupPage != null)
        {
            await Shell.Current.Navigation.PushAsync(developerSetupPage);
        }
    }

    private async void OnUsersPageTapped(object sender, EventArgs e)
    {
        var usersPage = new UsersPage(_viewModel, _localizationService, _serviceProvider);
        await Shell.Current.Navigation.PushAsync(usersPage);
    }

    private async void OnProductsPageTapped(object sender, EventArgs e)
    {
        var productsPage = new ProductsPage(_viewModel, _localizationService, _serviceProvider);
        await Shell.Current.Navigation.PushAsync(productsPage);
    }

    private async void OnAttendanceTrackerTapped(object sender, EventArgs e)
    {
        if (_viewModel.OpenAttendancePageCommand.CanExecute(null))
        {
            _viewModel.OpenAttendancePageCommand.Execute(null);
        }
    }

    private async void OnRestockReportTapped(object sender, EventArgs e)
    {
        var restockReportPage = _serviceProvider.GetService<RestockReportPage>();
        if (restockReportPage != null)
        {
            await Shell.Current.Navigation.PushAsync(restockReportPage);
        }
    }
}

