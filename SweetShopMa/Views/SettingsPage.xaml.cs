using SweetShopMa.ViewModels;
using SweetShopMa.Services;

namespace SweetShopMa.Views;

public partial class SettingsPage : ContentPage
{
    private readonly LocalizationService _localizationService;
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedStrings();
    }

    private void OnLanguageChanged()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateLocalizedStrings();
            FlowDirection = _localizationService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        });
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("ShopSettings");
        BackButton.Text = _localizationService.GetString("BackButton");
        BusinessInfoLabel.Text = _localizationService.GetString("BusinessInformation");
        BusinessNameEnLabel.Text = _localizationService.GetString("BusinessNameEnglish");
        BusinessNameArLabel.Text = _localizationService.GetString("BusinessNameArabic");
        TaxNumberLabel.Text = _localizationService.GetString("TaxNumber");
        CurrencyLabel.Text = _localizationService.GetString("Currency");
        ReceiptSettingsLabel.Text = _localizationService.GetString("ReceiptSettings");
        ReceiptFooterEnLabel.Text = _localizationService.GetString("ReceiptFooterEnglish");
        ReceiptFooterArLabel.Text = _localizationService.GetString("ReceiptFooterArabic");
        SaveSettingsButton.Text = _localizationService.GetString("SaveSettings");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("..");
    }

    private void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.Text is string language)
        {
            _localizationService.SetLanguage(language == "EN" ? "en" : "ar");
        }
    }
}
