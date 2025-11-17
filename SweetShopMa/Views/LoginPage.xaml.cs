using SweetShopMa.Services;
using SweetShopMa.Utils;

namespace SweetShopMa.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly DatabaseService _databaseService;
    private readonly LocalizationService _localizationService;

    public LoginPage(AuthService authService, DatabaseService databaseService, LocalizationService localizationService)
    {
        InitializeComponent();
        _authService = authService;
        _databaseService = databaseService;
        _localizationService = localizationService;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Seed database when login page appears to ensure users exist
        if (_databaseService != null)
        {
            await _databaseService.SeedUsersAsync();
            await _databaseService.SeedProductsAsync();
        }
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("Login");
        AppTitleLabel.Text = _localizationService.GetString("AppTitle");
        SecureLoginLabel.Text = _localizationService.GetString("SecureLogin");
        UsernameLabel.Text = _localizationService.GetString("Username");
        PasswordLabel.Text = _localizationService.GetString("Password");
        UsernameEntry.Placeholder = _localizationService.GetString("EnterUsername");
        PasswordEntry.Placeholder = _localizationService.GetString("EnterPassword");
        LoginButton.Text = _localizationService.GetString("LoginButton");
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

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;

        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = "";

        // Validation
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowError(_localizationService.GetString("PleaseEnterBoth"));
            return;
        }

        // Show loading
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        LoginButton.IsEnabled = false;

        try
        {
            var success = await _authService.LoginAsync(username, password);

            if (success)
            {
                // Clear fields
                UsernameEntry.Text = "";
                PasswordEntry.Text = "";

                // Close login page
                await Shell.Current.Navigation.PopAsync();
                
                // The MainPage OnAppearing will detect authentication and show the shop
            }
            else
            {
                ShowError(_localizationService.GetString("InvalidCredentials"));
            }
        }
        catch (Exception ex)
        {
            ShowError(string.Format(_localizationService.GetString("Error"), ex.Message));
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoginButton.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
