using SweetShopMa.Services;
using SweetShopMa.ViewModels;
using SweetShopMa.Views;
using SweetShopMa;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
#endif

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
            .AddSingleton<Services.LocalizationService>(_ => Services.LocalizationService.Instance)
            .AddSingleton<ShopViewModel>()
            .AddSingleton<AdminViewModel>()
            .AddSingleton<Views.MainPage>()
            .AddTransient<Views.LoginPage>()  // Transient so we can create new instances
            .AddTransient<Views.AdminPage>()
            .AddTransient<Views.AttendancePage>();

        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            events.AddWindows(w =>
            {
                w.OnWindowCreated(window =>
                {
                    var appWindow = window.GetAppWindow();
                    if (appWindow.Presenter is OverlappedPresenter presenter)
                    {
                        presenter.Maximize();
                        presenter.IsResizable = true;
                        presenter.IsMaximizable = true;
                    }
                    else
                    {
                        appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                    }
                });
            });
#endif
        });
        return builder.Build();
    }
}
