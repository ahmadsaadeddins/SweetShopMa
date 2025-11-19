using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa.Services;
using SweetShopMa.Utils;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class DeveloperSetupPage : ContentPage
{
    private DatabaseService? _databaseService;
    private LocalizationService? _localizationService;

    public DeveloperSetupPage()
    {
        InitializeComponent();
        // Services will be loaded in OnAppearing when Handler is available
    }

    public DeveloperSetupPage(DatabaseService databaseService, LocalizationService localizationService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _localizationService = localizationService;
    }

    private void LoadServices()
    {
        // Get services from Handler.MauiContext (for Shell DataTemplate)
        if (Handler?.MauiContext?.Services != null)
        {
            _databaseService = Handler.MauiContext.Services.GetService<DatabaseService>();
            _localizationService = Handler.MauiContext.Services.GetService<LocalizationService>();
        }
        else
        {
            // Fallback: try to get from Shell
            try
            {
                if (Shell.Current?.Handler?.MauiContext?.Services != null)
                {
                    _databaseService = Shell.Current.Handler.MauiContext.Services.GetService<DatabaseService>();
                    _localizationService = Shell.Current.Handler.MauiContext.Services.GetService<LocalizationService>();
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
        if (_databaseService == null || _localizationService == null)
        {
            LoadServices();
        }
        
        // Subscribe to language changes
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged += OnLanguageChanged;
        }
        
        UpdateLocalizedStrings();
        UpdateRTL();
        
        // Auto-focus name field
        await Task.Delay(100);
        if (NameEntry != null)
        {
            NameEntry.Focus();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe from language changes
        if (_localizationService != null)
        {
            _localizationService.LanguageChanged -= OnLanguageChanged;
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
        
        Title = _localizationService.GetString("CreateNewUser");
        if (BackButton != null)
            BackButton.Text = _localizationService.GetString("BackButton");
        if (HeaderLabel != null)
            HeaderLabel.Text = _localizationService.GetString("CreateNewUser");
        if (FullNameLabel != null)
            FullNameLabel.Text = _localizationService.GetString("FullName");
        if (NameEntry != null)
            NameEntry.Placeholder = _localizationService.GetString("EnterFullName");
        if (UsernameLabel != null)
            UsernameLabel.Text = _localizationService.GetString("Username");
        if (UsernameEntry != null)
            UsernameEntry.Placeholder = _localizationService.GetString("EnterUsername");
        if (PasswordLabel != null)
            PasswordLabel.Text = _localizationService.GetString("Password");
        if (PasswordEntry != null)
            PasswordEntry.Placeholder = _localizationService.GetString("EnterPassword");
        if (ConfirmPasswordLabel != null)
            ConfirmPasswordLabel.Text = _localizationService.GetString("ConfirmPassword");
        if (ConfirmPasswordEntry != null)
            ConfirmPasswordEntry.Placeholder = _localizationService.GetString("EnterConfirmPassword");
        if (SalaryLabel != null)
            SalaryLabel.Text = _localizationService.GetString("MonthlySalary");
        if (SalaryEntry != null)
            SalaryEntry.Placeholder = _localizationService.GetString("EnterMonthlySalary");
        if (RoleLabel != null)
            RoleLabel.Text = _localizationService.GetString("Role");
        if (CreateButton != null)
            CreateButton.Text = _localizationService.GetString("AddUser");
        
        // Update Role picker with localized strings
        if (RolePicker != null)
        {
            var currentSelection = RolePicker.SelectedIndex;
            RolePicker.ItemsSource = new[]
            {
                _localizationService.GetString("Admin"),
                _localizationService.GetString("Moderator"),
                _localizationService.GetString("User")
            };
            // Restore selection if valid
            if (currentSelection >= 0 && currentSelection < RolePicker.ItemsSource.Count)
            {
                RolePicker.SelectedIndex = currentSelection;
            }
            else
            {
                RolePicker.SelectedIndex = 2; // Default to User
            }
        }
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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (Navigation?.NavigationStack?.Count > 1)
        {
            await Navigation.PopAsync();
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    private void OnNameEntryCompleted(object sender, EventArgs e)
    {
        if (UsernameEntry != null)
        {
            UsernameEntry.Focus();
        }
    }

    private void OnUsernameEntryCompleted(object sender, EventArgs e)
    {
        if (PasswordEntry != null)
        {
            PasswordEntry.Focus();
        }
    }

    private void OnPasswordEntryCompleted(object sender, EventArgs e)
    {
        if (ConfirmPasswordEntry != null)
        {
            ConfirmPasswordEntry.Focus();
        }
    }

    private void OnConfirmPasswordEntryCompleted(object sender, EventArgs e)
    {
        if (SalaryEntry != null)
        {
            SalaryEntry.Focus();
        }
    }

    private void OnSalaryEntryCompleted(object sender, EventArgs e)
    {
        OnCreateClicked(sender, e);
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        var username = UsernameEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;
        var salaryText = SalaryEntry.Text?.Trim() ?? "0";
        
        // Map localized role back to English role name
        var selectedRole = RolePicker.SelectedItem?.ToString() ?? (_localizationService?.GetString("User") ?? "User");
        string role = "User"; // Default
        if (_localizationService != null)
        {
            if (selectedRole == _localizationService.GetString("Admin") || selectedRole == "Admin")
                role = "Admin";
            else if (selectedRole == _localizationService.GetString("Moderator") || selectedRole == "Moderator")
                role = "Moderator";
            else
                role = "User";
        }
        else
        {
            // Fallback if localization service is not available
            role = selectedRole == "Admin" ? "Admin" : (selectedRole == "Moderator" ? "Moderator" : "User");
        }

        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = "";

        // Validation
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError(_localizationService?.GetString("PleaseEnterName") ?? "Please enter a name");
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowError(_localizationService?.GetString("PleaseEnterUsername") ?? "Please enter a username");
            return;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowError(_localizationService?.GetString("PleaseEnterPassword") ?? "Please enter a password");
            return;
        }

        if (password.Length < 4)
        {
            ShowError(_localizationService?.GetString("PasswordMustBe4Chars") ?? "Password must be at least 4 characters");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError(_localizationService?.GetString("PasswordsDoNotMatch") ?? "Passwords do not match");
            return;
        }

        if (!decimal.TryParse(salaryText, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var salary) || salary < 0)
        {
            ShowError(_localizationService?.GetString("PleaseEnterValidSalary") ?? "Please enter a valid salary");
            return;
        }

        // Show loading
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;
        CreateButton.IsEnabled = false;

        try
        {
            // Check if username already exists
            if (await _databaseService.UsernameExistsAsync(username))
            {
                ShowError(_localizationService?.GetString("UsernameExists") ?? "Username already exists");
                return;
            }

            // Create user
            var user = new Models.User
            {
                Name = name,
                Username = username,
                Password = PasswordHelper.HashPassword(password),
                Role = role,
                IsEnabled = true,
                MonthlySalary = salary
            };

            await _databaseService.CreateUserAsync(user);

            // Show success and navigate back to admin
            var successMsg = _localizationService?.GetString("UserCreatedSuccessfully") ?? $"User '{username}' created successfully!";
            await Application.Current.MainPage.DisplayAlert(
                _localizationService?.GetString("Success") ?? "Success", 
                string.Format(successMsg, username), 
                _localizationService?.GetString("OK") ?? "OK");

            // Clear form
            NameEntry.Text = "";
            UsernameEntry.Text = "";
            PasswordEntry.Text = "";
            ConfirmPasswordEntry.Text = "";
            SalaryEntry.Text = "0";
            RolePicker.SelectedIndex = 2; // Reset to User

            // Navigate back to admin page and refresh user list
            await Shell.Current.Navigation.PopAsync();
            
            // Refresh the admin page user list if it's still open
            if (Shell.Current.Navigation.NavigationStack.LastOrDefault() is AdminPage adminPage)
            {
                if (adminPage.BindingContext is AdminViewModel adminViewModel)
                {
                    await adminViewModel.InitializeAsync();
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            CreateButton.IsEnabled = true;
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}

