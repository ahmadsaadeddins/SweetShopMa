using SweetShopMa.Services;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class AttendancePage : ContentPage
{
    private readonly AdminViewModel _viewModel;
    private readonly LocalizationService _localizationService;

    public AttendancePage(AdminViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _localizationService = localizationService;
        BindingContext = _viewModel;
        
        _localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateLocalizedStrings();
        UpdateRTL();
        await _viewModel.LoadAttendanceAsync();
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("AttendanceTracker");
        if (AttendanceTrackerTitleLabel != null)
            AttendanceTrackerTitleLabel.Text = _localizationService.GetString("AttendanceTracker");
        if (EmployeePicker != null)
            EmployeePicker.Title = _localizationService.GetString("Employee");
        if (StatusPicker != null)
            StatusPicker.Title = _localizationService.GetString("Status");
        if (NotesEntry != null)
            NotesEntry.Placeholder = _localizationService.GetString("NotesOptional");
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
}

