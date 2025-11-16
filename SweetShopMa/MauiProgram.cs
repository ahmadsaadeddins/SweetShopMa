using SweetShopMa.Services;
using SweetShopMa.ViewModels;
using SweetShopMa.Views;
using SweetShopMa;

namespace SweetShopMa;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            // Register Services
            .Services.AddSingleton<DatabaseService>()
            .AddSingleton<AuthService>()
            .AddSingleton<CartService>()
            .AddSingleton<ShopViewModel>()
            .AddSingleton<Views.MainPage>()
            .AddTransient<Views.LoginPage>();  // Transient so we can create new instances
        return builder.Build();
    }
}
