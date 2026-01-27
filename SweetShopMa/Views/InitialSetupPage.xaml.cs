using SweetShopMa.ViewModels;
using SweetShopMa.Services;

namespace SweetShopMa.Views;

public partial class InitialSetupPage : ContentPage
{
    private readonly LocalizationService _localizationService;

    public InitialSetupPage(InitialSetupViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _localizationService = localizationService;
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
