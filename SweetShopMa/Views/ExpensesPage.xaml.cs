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

