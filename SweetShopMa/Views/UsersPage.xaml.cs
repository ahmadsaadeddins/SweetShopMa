using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using SweetShopMa.Services;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class UsersPage : ContentPage
{
    private readonly AdminViewModel _viewModel;
    private readonly LocalizationService _localizationService;
    private readonly IServiceProvider _serviceProvider;

    public UsersPage(AdminViewModel viewModel, LocalizationService localizationService, IServiceProvider serviceProvider)
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
        Title = _localizationService.GetString("UserManagement") ?? "User Management";
        if (PageTitleLabel != null)
            PageTitleLabel.Text = _localizationService.GetString("UserManagement") ?? "User Management";
        if (BackButton != null)
            BackButton.Text = _localizationService.GetString("BackButton");
        if (CreateUserLabel != null)
            CreateUserLabel.Text = _localizationService.GetString("UserManagement") ?? "User Management";
        if (CreateUserButton != null)
            CreateUserButton.Text = _localizationService.GetString("CreateNewUser");
        if (ExistingUsersLabel != null)
            ExistingUsersLabel.Text = _localizationService.GetString("ExistingUsers");
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
}

