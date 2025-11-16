using SweetShopMa.Services;
using SweetShopMa.Utils;

namespace SweetShopMa.Views;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;
    private readonly DatabaseService _databaseService;

    public LoginPage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        _authService = new AuthService(_databaseService);
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
            ShowError("⚠️ Please enter both username and password");
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
                ShowError("❌ Invalid username or password");
            }
        }
        catch (Exception ex)
        {
            ShowError($"⚠️ Error: {ex.Message}");
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
