using SweetShopMa;
using SweetShopMa.Views;

namespace SweetShopMa;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();

        // Start at login page
        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute("shop", typeof(MainPage));
    }
}
