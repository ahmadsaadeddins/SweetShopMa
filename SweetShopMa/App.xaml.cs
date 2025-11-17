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
