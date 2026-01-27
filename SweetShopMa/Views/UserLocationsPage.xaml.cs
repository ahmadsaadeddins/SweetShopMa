using SweetShopMa.ViewModels;
using SweetShopMa.Services;

namespace SweetShopMa.Views;

public partial class UserLocationsPage : ContentPage
{
    private readonly LocalizationService _localizationService;

    public UserLocationsPage(UserLocationsViewModel viewModel, LocalizationService localizationService)
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
        Title = _localizationService.GetString("UserAssignmentsTitle");
        BackButton.Text = _localizationService.GetString("BackButton");
        UserLocationAssignmentsLabel.Text = _localizationService.GetString("UserLocationAssignments");
        SelectUserLabel.Text = _localizationService.GetString("SelectUserToViewAssignments");
        NoLocationsLabel.Text = _localizationService.GetString("NoLocationsAssigned");
        AssignNewLocationLabel.Text = _localizationService.GetString("AssignNewLocation");
        SelectLocationLabel.Text = _localizationService.GetString("SelectLocation");
        SetAsPrimaryLabel.Text = _localizationService.GetString("SetAsUsersPrimaryLocation");
        AssignUserButton.Text = _localizationService.GetString("AssignUser");
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
