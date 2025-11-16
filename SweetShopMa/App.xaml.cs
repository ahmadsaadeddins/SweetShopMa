using SweetShopMa;

namespace SweetShopMa;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
    }
}
