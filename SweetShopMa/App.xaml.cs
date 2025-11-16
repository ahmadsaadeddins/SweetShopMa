using SweetShopMa;
using SweetShopMa.Views;

namespace SweetShopMa;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
