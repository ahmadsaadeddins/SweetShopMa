using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Services;
using SweetShopMa.Utils;

namespace SweetShopMa.ViewModels;

public partial class InitialSetupViewModel : ObservableObject
{
    private readonly DatabaseService _db;
    private readonly IShopSettingsService _settingsService;

    [ObservableProperty]
    private int _currentStep = 1;

    // Step 1: Business Details
    [ObservableProperty] private string _businessName = "Sweet Shop";
    [ObservableProperty] private string _businessNameArabic = "متجر حلويات";
    [ObservableProperty] private string _taxNumber = "";
    [ObservableProperty] private string _currency = "EGP";
    [ObservableProperty] private string _receiptFooter = "Thank you for shopping with us!";
    [ObservableProperty] private string _receiptFooterArabic = "شكراً لتسوقكم معنا!";

    // Step 2: Location Details
    [ObservableProperty] private string _locationName = "Main Branch";
    [ObservableProperty] private string _address = "";
    [ObservableProperty] private string _phoneNumber = "";

    // Step 3: Developer Account
    [ObservableProperty] private string _devUsername = "ama";
    [ObservableProperty] private string _devPassword = "";
    [ObservableProperty] private string _devConfirmPassword = "";
    [ObservableProperty] private string _devName = "Ahmad";

    // Step 4: Admin Account
    [ObservableProperty] private string _adminUsername = "admin";
    [ObservableProperty] private string _adminPassword = "";
    [ObservableProperty] private string _adminConfirmPassword = "";
    [ObservableProperty] private string _adminName = "Administrator";

    public InitialSetupViewModel(DatabaseService db, IShopSettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
    }

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep < 4)
            CurrentStep++;
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
            CurrentStep--;
    }

    [RelayCommand]
    private async Task CompleteSetupAsync()
    {
        try
        {
            // Validation (simplified for now)
            if (string.IsNullOrWhiteSpace(DevPassword) || DevPassword != DevConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Developer passwords do not match or are empty", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(AdminPassword) || AdminPassword != AdminConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Admin passwords do not match or are empty", "OK");
                return;
            }

            // 1. Save Shop Settings
            var settings = await _settingsService.GetSettingsAsync();
            settings.BusinessName = BusinessName;
            settings.BusinessNameArabic = BusinessNameArabic;
            settings.TaxNumber = TaxNumber;
            settings.Currency = Currency;
            settings.ReceiptFooter = ReceiptFooter;
            settings.ReceiptFooterArabic = ReceiptFooterArabic;
            await _settingsService.SaveSettingsAsync(settings);

            // 2. Create Primary Location
            var location = new ShopLocation
            {
                LocationName = LocationName,
                Address = Address,
                PhoneNumber = PhoneNumber,
                IsPrimary = true,
                IsActive = true
            };
            var locationId = await _settingsService.CreateLocationAsync(location);
            
            // Set active location
            await _settingsService.SetActiveLocationAsync(locationId);

            // 3. Create Developer User
            var devUser = new User
            {
                Username = DevUsername,
                Password = PasswordHelper.HashPassword(DevPassword),
                Role = "Developer",
                Name = DevName,
                IsEnabled = true
            };
            await _db.CreateUserAsync(devUser);
            await _db.AssignUserToLocationAsync(devUser.Id, locationId, true);

            // 4. Create Admin User
            var adminUser = new User
            {
                Username = AdminUsername,
                Password = PasswordHelper.HashPassword(AdminPassword),
                Role = "Admin",
                Name = AdminName,
                IsEnabled = true
            };
            await _db.CreateUserAsync(adminUser);
            await _db.AssignUserToLocationAsync(adminUser.Id, locationId, true);

            await Shell.Current.DisplayAlert("Success", "Setup complete! Please login.", "OK");
            await Shell.Current.GoToAsync("///login");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
