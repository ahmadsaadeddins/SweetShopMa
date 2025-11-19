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
        if (CreateUserLabel != null)
            CreateUserLabel.Text = _localizationService.GetString("UserManagement") ?? "User Management";
        if (CreateUserButton != null)
            CreateUserButton.Text = _localizationService.GetString("CreateNewUser");
        if (ExistingUsersLabel != null)
            ExistingUsersLabel.Text = _localizationService.GetString("ExistingUsers");
        if (ReportsLabel != null)
            ReportsLabel.Text = _localizationService.GetString("ReportsInsights");
        if (TopProductsLabel != null)
            TopProductsLabel.Text = _localizationService.GetString("TopProducts");
        if (RecentOrdersLabel != null)
            RecentOrdersLabel.Text = _localizationService.GetString("RecentOrders");
        if (AttendanceTrackerLabel != null)
            AttendanceTrackerLabel.Text = _localizationService.GetString("AttendanceTracker");
        if (OpenAttendanceButton != null)
            OpenAttendanceButton.Text = _localizationService.GetString("OpenAttendanceTracker");
        if (AddProductLabel != null)
            AddProductLabel.Text = _localizationService.GetString("AddProduct");
        if (AddProductButton != null)
            AddProductButton.Text = _localizationService.GetString("AddProduct");
        if (ProductsLabel != null)
            ProductsLabel.Text = _localizationService.GetString("Products");
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
        if (RestockReportLabel != null)
            RestockReportLabel.Text = _localizationService.GetString("RestockReport");
        if (OpenRestockReportButton != null)
            OpenRestockReportButton.Text = _localizationService.GetString("ViewRestockReport");
        
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

    private async void OnOpenRestockReportClicked(object sender, EventArgs e)
    {
        var restockReportPage = _serviceProvider.GetService<RestockReportPage>();
        if (restockReportPage != null)
        {
            await Shell.Current.Navigation.PushAsync(restockReportPage);
        }
    }
}

