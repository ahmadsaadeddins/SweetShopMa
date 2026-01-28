using SweetShopMa.ViewModels;
using SweetShopMa.Services;

namespace SweetShopMa.Views;

public partial class ShopLocationsPage : ContentPage
{
    private readonly LocalizationService _localizationService;
    private readonly LocationsViewModel _viewModel;

    public ShopLocationsPage(LocationsViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        _localizationService = localizationService;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
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

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LocationsViewModel.IsEditMode))
        {
            UpdateFormTitle();
        }
    }

    private void UpdateFormTitle()
    {
        FormTitleLabel.Text = _viewModel.IsEditMode 
            ? _localizationService.GetString("EditLocation") 
            : _localizationService.GetString("AddNewLocation");
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("ShopLocationsTitle");
        BackButton.Text = _localizationService.GetString("BackButton");
        ListOfLocationsLabel.Text = _localizationService.GetString("ListOfLocations");
        UpdateFormTitle();
        LocationNameLabel.Text = _localizationService.GetString("LocationName");
        LocationNameEntry.Placeholder = _localizationService.GetString("LocationNamePlaceholder");
        AddressLabel.Text = _localizationService.GetString("Address");
        PhoneNumberLabel.Text = _localizationService.GetString("PhoneNumber");
        SetAsPrimaryLabel.Text = _localizationService.GetString("SetAsPrimaryLocation");
        SaveLocationButton.Text = _localizationService.GetString("SaveLocation");
        CancelButton.Text = _localizationService.GetString("Cancel");
    }

    protected override void OnDisappearing()
    {
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        base.OnDisappearing();
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
