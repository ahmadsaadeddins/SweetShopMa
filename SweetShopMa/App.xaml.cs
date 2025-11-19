using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa;
using SweetShopMa.Services;
using SweetShopMa.Views;

namespace SweetShopMa;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
        
        // Navigate to login page on startup
        // LoginPage will handle seeding users and products
        if (MainPage is AppShell shell)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(200); // Small delay to ensure Shell is ready
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await shell.GoToAsync("//login");
                });
            });
        }
        
        // Initialize localization and set RTL if needed
        var localizationService = LocalizationService.Instance;
        localizationService.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged();
    }

    private void OnLanguageChanged()
    {
        var localizationService = LocalizationService.Instance;
        if (MainPage != null)
        {
            MainPage.FlowDirection = localizationService.IsRTL 
                ? FlowDirection.RightToLeft 
                : FlowDirection.LeftToRight;
        }
    }
}
