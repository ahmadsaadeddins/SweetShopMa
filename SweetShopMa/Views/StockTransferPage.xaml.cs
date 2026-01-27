using SweetShopMa.ViewModels;
using SweetShopMa.Services;

namespace SweetShopMa.Views;

public partial class StockTransferPage : ContentPage
{
    private readonly LocalizationService _localizationService;

    public StockTransferPage(StockTransferViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
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
        Title = _localizationService.GetString("StockTransferTitle");
        BackButton.Text = _localizationService.GetString("BackButton");
        PageTitleLabel.Text = _localizationService.GetString("TransferStockBetweenLocations");
        SelectProductLabel.Text = _localizationService.GetString("SelectProduct");
        FromLocationLabel.Text = _localizationService.GetString("FromLocation");
        ToLocationLabel.Text = _localizationService.GetString("ToLocation");
        QuantityLabel.Text = _localizationService.GetString("QuantityToTransfer");
        QuantityEntry.Placeholder = _localizationService.GetString("QuantityPlaceholder");
        ExecuteTransferButton.Text = _localizationService.GetString("ExecuteTransfer");
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
