using Microsoft.Maui.Controls;
using SweetShopMa.Services;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class AdminPage : ContentPage
{
    private readonly AdminViewModel _viewModel;
    private readonly LocalizationService _localizationService;

    public AdminPage(AdminViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _localizationService = localizationService;
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
        if (DatabaseInfoLabel != null)
            DatabaseInfoLabel.Text = _localizationService.GetString("DatabaseInformation");
        if (CreateUserLabel != null)
            CreateUserLabel.Text = _localizationService.GetString("CreateNewUser");
        if (FullNameEntry != null)
            FullNameEntry.Placeholder = _localizationService.GetString("FullName");
        if (UsernameEntry != null)
            UsernameEntry.Placeholder = _localizationService.GetString("UsernamePlaceholder");
        if (PasswordEntry != null)
            PasswordEntry.Placeholder = _localizationService.GetString("PasswordPlaceholder");
        if (SalaryEntry != null)
            SalaryEntry.Placeholder = _localizationService.GetString("MonthlySalary");
        if (GrantAdminLabel != null)
            GrantAdminLabel.Text = _localizationService.GetString("GrantAdminAccess");
        if (AddUserButton != null)
            AddUserButton.Text = _localizationService.GetString("AddUser");
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
}

