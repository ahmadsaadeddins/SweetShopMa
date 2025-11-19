using System;
using System.Threading.Tasks;
using SweetShopMa.Services;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class RestockReportPage : ContentPage
{
    private readonly LocalizationService _localizationService;

    public RestockReportPage(RestockReportViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _localizationService = localizationService;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RestockReportViewModel viewModel)
        {
            await viewModel.LoadRestockRecordsAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _localizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("RestockReport");
        if (BackButton != null)
            BackButton.Text = _localizationService.GetString("BackButton");
        if (TitleLabel != null)
            TitleLabel.Text = _localizationService.GetString("RestockReport");
        if (EmptyViewLabel != null)
            EmptyViewLabel.Text = _localizationService.GetString("NoRestockRecordsFound");
        
        // Update CollectionView item labels
        if (BindingContext is RestockReportViewModel viewModel)
        {
            viewModel.RefreshLocalizedStrings();
        }
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

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        if (Navigation?.NavigationStack?.Count > 1)
        {
            await Navigation.PopAsync();
            return;
        }

        await Shell.Current.GoToAsync("..");
    }
}

