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
    private IShopSettingsService? _settingsService;
    private SessionContext? _sessionContext;

    /// <summary>
    /// Parameterless constructor (used by Shell DataTemplate).
    /// Services will be loaded in OnAppearing when Handler is available.
    /// </summary>
    public LoginPage()
    {
        InitializeComponent();
        // Services will be loaded in OnAppearing when Handler is available
    }

    public LoginPage(AuthService authService, DatabaseService databaseService, LocalizationService localizationService, IShopSettingsService settingsService, SessionContext sessionContext)
    {
        InitializeComponent();
        _authService = authService;
        _databaseService = databaseService;
        _localizationService = localizationService;
        _settingsService = settingsService;
        _sessionContext = sessionContext;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        _ = UpdateLocalizedStringsAsync(); // Fire and forget with proper error handling inside
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
            _settingsService = Handler.MauiContext.Services.GetService<IShopSettingsService>();
            _sessionContext = Handler.MauiContext.Services.GetService<SessionContext>();
            
            if (_localizationService != null)
            {
                _localizationService.LanguageChanged += OnLanguageChanged;
                _ = UpdateLocalizedStringsAsync();
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
                    _settingsService = Shell.Current.Handler.MauiContext.Services.GetService<IShopSettingsService>();
                    
                    if (_localizationService != null)
                    {
                        _localizationService.LanguageChanged += OnLanguageChanged;
                        _ = UpdateLocalizedStringsAsync();
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

    protected override void OnDisappearing()
    {
        if (_localizationService != null)
            _localizationService.LanguageChanged -= OnLanguageChanged;
            
        base.OnDisappearing();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Load services if not already loaded (for Shell DataTemplate)
        if (_authService == null || _databaseService == null || _localizationService == null)
        {
            LoadServices();
        }
        
        // Check if setup is needed
        if (_databaseService != null && !await _databaseService.HasAnyUsersAsync())
        {
            await MainThread.InvokeOnMainThreadAsync(async () => 
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//initialsetup");
                }
            });
            return;
        }

        // Seed products and users when login page appears (if not already seeded)
        // Seed logic is now handled by Initial Setup for new installs
        /*
        if (_databaseService != null)
        {
            await _databaseService.SeedUsersAsync();
            await _databaseService.SeedProductsAsync();
        }
        */
        
        // Auto-focus username field for quick entry (with small delay to ensure page is ready)
        await Task.Delay(250); // Slightly longer delay for stability
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (UsernameEntry != null)
            {
                UsernameEntry.Focus();
            }
        });
    }

    private void OnLanguageChanged()
    {
        _ = UpdateLocalizedStringsAsync();
        UpdateRTL();
    }

    private async Task UpdateLocalizedStringsAsync()
    {
        if (_localizationService == null) return;
        
        Title = _localizationService.GetString("Login");
        if (AppTitleLabel != null)
        {
            try 
            {
                var settings = await (_settingsService?.GetSettingsAsync() ?? Task.FromResult<Models.ShopSettings>(null)); 
                string bizName = (_localizationService.IsRTL ? settings?.BusinessNameArabic : settings?.BusinessName) 
                                 ?? _localizationService.GetString("AppTitle");
                AppTitleLabel.Text = bizName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings for title: {ex.Message}");
                AppTitleLabel.Text = _localizationService.GetString("AppTitle");
            }
        }
        
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
        if (SelectLocationLabel != null)
            SelectLocationLabel.Text = _localizationService.GetString("SelectLocation");
        if (ConfirmLocationButton != null)
            ConfirmLocationButton.Text = "✅ " + (_localizationService.GetString("LoginButton") ?? "Login");
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
                var user = _authService.CurrentUser;
                var availableLocations = await _settingsService.GetLocationsForUserAsync(user.Id);

                if (availableLocations.Count == 0)
                {
                    // No locations assigned to this user, but maybe they are admin/dev
                    if (user.IsAdmin || user.IsDeveloper)
                    {
                        availableLocations = await _settingsService.GetLocationsAsync();
                    }
                }

                if (availableLocations.Count == 0)
                {
                    ShowError("No locations assigned to this user. Please contact administrator.");
                    return;
                }

                if (availableLocations.Count == 1)
                {
                    _sessionContext.ActiveLocation = availableLocations[0];
                    // Clear fields
                    UsernameEntry.Text = "";
                    PasswordEntry.Text = "";

                    // Navigate to shop page
                    await Shell.Current.GoToAsync("//shop");
                }
                else
                {
                    // Multiple locations, show picker
                    LocationPicker.ItemsSource = availableLocations;
                    LocationPicker.SelectedIndex = 0;
                    
                    // Hide login fields, show location picker
                    UsernameLabel.IsVisible = false;
                    UsernameEntry.IsVisible = false;
                    PasswordLabel.IsVisible = false;
                    PasswordEntry.IsVisible = false;
                    LoginButton.IsVisible = false;
                    
                    LocationSelectionLayout.IsVisible = true;
                }
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

    private async void OnConfirmLocationClicked(object sender, EventArgs e)
    {
        if (LocationPicker.SelectedItem is SweetShopMa.Models.ShopLocation selectedLocation && _sessionContext != null)
        {
            _sessionContext.ActiveLocation = selectedLocation;
            
            // Clear fields
            UsernameEntry.Text = "";
            PasswordEntry.Text = "";
            
            // Navigate to shop page
            await Shell.Current.GoToAsync("//shop");
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
