using SweetShopMa.Services;
using SweetShopMa.Utils;

namespace SweetShopMa.Views;

public partial class LoginPage : ContentPage
{
    private AuthService? _authService;
    private DatabaseService? _databaseService;
    private LocalizationService? _localizationService;

    public LoginPage()
    {
        InitializeComponent();
        // Services will be loaded in OnAppearing when Handler is available
    }

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

    private void LoadServices()
    {
        // Get services from Handler.MauiContext (for Shell DataTemplate)
        if (Handler?.MauiContext?.Services != null)
        {
            _authService = Handler.MauiContext.Services.GetService<AuthService>();
            _databaseService = Handler.MauiContext.Services.GetService<DatabaseService>();
            _localizationService = Handler.MauiContext.Services.GetService<LocalizationService>();
            
            if (_localizationService != null)
            {
                _localizationService.LanguageChanged += OnLanguageChanged;
                UpdateLocalizedStrings();
                UpdateRTL();
            }
        }
        else
        {
            // Fallback: try to get from Shell
            try
            {
                if (Shell.Current?.Handler?.MauiContext?.Services != null)
                {
                    _authService = Shell.Current.Handler.MauiContext.Services.GetService<AuthService>();
                    _databaseService = Shell.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
                    _localizationService = Shell.Current.Handler.MauiContext.Services.GetService<LocalizationService>();
                    
                    if (_localizationService != null)
                    {
                        _localizationService.LanguageChanged += OnLanguageChanged;
                        UpdateLocalizedStrings();
                        UpdateRTL();
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Load services if not already loaded (for Shell DataTemplate)
        if (_authService == null || _databaseService == null || _localizationService == null)
        {
            LoadServices();
        }
        
        // Seed database when login page appears to ensure users exist
        if (_databaseService != null)
        {
            await _databaseService.SeedUsersAsync();
            await _databaseService.SeedProductsAsync();
        }
        
        // Auto-focus username field for quick entry (with small delay to ensure page is ready)
        await Task.Delay(100);
        if (UsernameEntry != null)
        {
            UsernameEntry.Focus();
        }
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        if (_localizationService == null) return;
        
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
        if (_localizationService == null) return;
        FlowDirection = _localizationService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    private void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        if (_localizationService == null) return;
        var currentLang = _localizationService.CurrentLanguage;
        var newLang = currentLang == "en" ? "ar" : "en";
        _localizationService.SetLanguage(newLang);
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (_authService == null || _localizationService == null)
        {
            LoadServices();
            if (_authService == null || _localizationService == null)
            {
                ShowError("Services not available. Please restart the app.");
                return;
            }
        }

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

                // Navigate to shop page
                await Shell.Current.GoToAsync("//shop");
            }
            else
            {
                ShowError(_localizationService.GetString("InvalidCredentials"));
                // Refocus username field after failed login
                if (UsernameEntry != null)
                {
                    UsernameEntry.Focus();
                }
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

    private void OnUsernameEntryCompleted(object sender, EventArgs e)
    {
        // When Enter is pressed in username field, move focus to password field
        if (PasswordEntry != null)
        {
            PasswordEntry.Focus();
        }
    }

    private void OnPasswordEntryCompleted(object sender, EventArgs e)
    {
        // When Enter is pressed in password field, trigger login
        OnLoginClicked(sender, e);
    }
}
