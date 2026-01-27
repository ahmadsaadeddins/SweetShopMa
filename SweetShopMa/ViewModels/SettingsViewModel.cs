using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SweetShopMa.Models;
using SweetShopMa.Resources;
using SweetShopMa.Services;

namespace SweetShopMa.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IShopSettingsService _settingsService;
    private readonly SessionContext _sessionContext;

    [ObservableProperty] private string _businessName = "";
    [ObservableProperty] private string _businessNameArabic = "";
    [ObservableProperty] private string _taxNumber = "";
    [ObservableProperty] private string _currency = "";
    [ObservableProperty] private string _receiptFooter = "";
    [ObservableProperty] private string _receiptFooterArabic = "";

    [ObservableProperty] private bool _isSaving;

    public SettingsViewModel(IShopSettingsService settingsService, SessionContext sessionContext)
    {
        _settingsService = settingsService;
        _sessionContext = sessionContext;
        LoadSettingsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        BusinessName = settings.BusinessName;
        BusinessNameArabic = settings.BusinessNameArabic;
        TaxNumber = settings.TaxNumber;
        Currency = settings.Currency;
        ReceiptFooter = settings.ReceiptFooter;
        ReceiptFooterArabic = settings.ReceiptFooterArabic;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsSaving = true;
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            settings.BusinessName = BusinessName;
            settings.BusinessNameArabic = BusinessNameArabic;
            settings.TaxNumber = TaxNumber;
            settings.Currency = Currency;
            settings.ReceiptFooter = ReceiptFooter;
            settings.ReceiptFooterArabic = ReceiptFooterArabic;

            await _settingsService.SaveSettingsAsync(settings);
            await Shell.Current.DisplayAlert(Strings.Success, Strings.SettingsSavedSuccessfully, Strings.OK);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.OK);
        }
        finally
        {
            IsSaving = false;
        }
    }
}
