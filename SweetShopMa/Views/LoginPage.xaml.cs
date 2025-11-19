using SweetShopMa.Services;
using SweetShopMa.Utils;

namespace SweetShopMa.Views;

/// <summary>
/// Code-behind for the Login Page (LoginPage.xaml).
/// 
/// WHAT IS CODE-BEHIND?
/// Code-behind files contain C# code that handles UI events and interactions.
/// They work together with XAML files (which define the UI layout).
/// 
/// KEY RESPONSIBILITIES:
/// - Handle user login (validate credentials, navigate to shop)
/// - Manage keyboard navigation (Enter key moves focus, triggers login)
/// - Update localized strings when language changes
/// - Handle RTL (Right-to-Left) layout for Arabic
/// - Auto-focus username field when page appears
/// - Seed initial data (users, products) if database is empty
/// 
/// KEYBOARD NAVIGATION:
/// - Enter in UsernameEntry → Moves focus to PasswordEntry
/// - Enter in PasswordEntry → Triggers login
/// - Auto-focuses UsernameEntry when page appears
/// 
/// SERVICE LOADING:
/// This page can be created in two ways:
/// 1. Via dependency injection (constructor with services)
/// 2. Via Shell DataTemplate (loads services from Handler.MauiContext)
/// 
/// NAVIGATION:
/// On successful login → Navigates to "//shop" (MainPage)
/// On failed login → Shows error message, refocuses UsernameEntry
/// </summary>
public partial class LoginPage : ContentPage
{
    // Services loaded via dependency injection or from Handler.MauiContext
    private AuthService? _authService;
    private DatabaseService? _databaseService;
    private LocalizationService? _localizationService;

    /// <summary>
    /// Parameterless constructor (used by Shell DataTemplate).
    /// Services will be loaded in OnAppearing when Handler is available.
    /// </summary>
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
        
        // Seed products and users when login page appears (if not already seeded)
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
        if (AppTitleLabel != null)
            AppTitleLabel.Text = _localizationService.GetString("AppTitle");
        if (SecureLoginLabel != null)
            SecureLoginLabel.Text = _localizationService.GetString("SecureLogin");
        if (UsernameLabel != null)
            UsernameLabel.Text = _localizationService.GetString("Username");
        if (PasswordLabel != null)
            PasswordLabel.Text = _localizationService.GetString("Password");
        if (UsernameEntry != null)
            UsernameEntry.Placeholder = _localizationService.GetString("EnterUsername");
        if (PasswordEntry != null)
            PasswordEntry.Placeholder = _localizationService.GetString("EnterPassword");
        if (LoginButton != null)
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
