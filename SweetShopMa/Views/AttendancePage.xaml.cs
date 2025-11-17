using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class AttendancePage : ContentPage
{
    private readonly AdminViewModel _viewModel;

    public AttendancePage(AdminViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAttendanceAsync();
    }
}

