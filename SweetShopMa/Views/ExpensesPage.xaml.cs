using SweetShopMa.Services;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class ExpensesPage : ContentPage
{
    private readonly AdminViewModel _viewModel;
    private readonly LocalizationService _localizationService;

    public ExpensesPage(AdminViewModel viewModel, LocalizationService localizationService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _localizationService = localizationService;
        BindingContext = _viewModel;

        _localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void OnLanguageChanged()
    {
        UpdateLocalizedStrings();
        UpdateRTL();
    }

    private void UpdateLocalizedStrings()
    {
        Title = _localizationService.GetString("EmployeeExpenses");
        if (PageHeaderLabel != null)
            PageHeaderLabel.Text = _localizationService.GetString("EmployeeExpenses");
        if (BackButton != null)
            BackButton.Text = _localizationService.GetString("BackButton");
        if (EmployeePicker != null)
            EmployeePicker.Title = _localizationService.GetString("Employee");
        if (AmountEntry != null)
            AmountEntry.Placeholder = _localizationService.GetString("Amount");
        if (CategoryEntry != null)
            CategoryEntry.Placeholder = _localizationService.GetString("Category");
        if (NotesEntry != null)
            NotesEntry.Placeholder = _localizationService.GetString("Notes");
        if (AddExpenseButton != null)
            AddExpenseButton.Text = _localizationService.GetString("AddExpense");
        // Note: Collector items (Delete button) are handled via DataTemplate which doesn't support easy dynamic name-based update,
        // but it will be refreshed when the view re-renders or if bound via ViewModel.
        // However, for single-page apps, often we need to trigger a refresh of the CollectionView.
    }

    private void UpdateRTL()
    {
        FlowDirection = _localizationService.IsRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
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

    private void OnLanguageButtonClicked(object sender, EventArgs e)
    {
        var currentLang = _localizationService.CurrentLanguage;
        var newLang = currentLang == "en" ? "ar" : "en";
        _localizationService.SetLanguage(newLang);
    }
}

