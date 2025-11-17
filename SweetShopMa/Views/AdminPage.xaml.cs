using Microsoft.Maui.Controls;
using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class AdminPage : ContentPage
{
    private readonly AdminViewModel _viewModel;

    public AdminPage(AdminViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_viewModel.IsAuthorized)
        {
            await DisplayAlert("Access Denied", "Administrator privileges are required.", "OK");
            await Shell.Current.Navigation.PopAsync();
            return;
        }

        await _viewModel.InitializeAsync();
    }
}

