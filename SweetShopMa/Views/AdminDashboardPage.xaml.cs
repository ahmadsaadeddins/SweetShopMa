using SweetShopMa.ViewModels;

namespace SweetShopMa.Views;

public partial class AdminDashboardPage : ContentPage
{
    public AdminDashboardPage(AdminViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

