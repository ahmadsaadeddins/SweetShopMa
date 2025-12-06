using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using SweetShopMa;
using SweetShopMa.Services;
using SweetShopMa.Views;

namespace SweetShopMa;

/// <summary>
/// Application entry point and app-level configuration.
/// 
/// WHAT IS APP.XAML.CS?
/// App.xaml.cs is the main application class that handles:
/// - Application initialization
/// - Navigation to initial page (LoginPage)
/// - App-level localization setup
/// - RTL (Right-to-Left) layout configuration
/// 
/// APPLICATION LIFECYCLE:
/// 1. App constructor runs when app starts
/// 2. Creates AppShell (navigation structure)
/// 3. Navigates to LoginPage ("//login" route)
/// 4. Initializes localization
/// 5. Sets RTL layout if Arabic is selected
/// 
/// NAVIGATION:
/// Uses MAUI Shell navigation to go to "//login" route on startup.
/// LoginPage handles seeding initial data (users, products) when it appears.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Application constructor - called when app starts.
    /// Sets up AppShell, navigates to login page, and initializes localization.
    /// </summary>
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
                try
                {
                    await Task.Delay(Utils.AppConstants.NavigationDelayMs); // Small delay to ensure Shell is ready
                    try
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await shell.GoToAsync("//login");
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Navigation scheduling error: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Background navigation error: {ex}");
                }
            });
        }
        
        // Initialize localization and set RTL if needed
        var localizationService = LocalizationService.Instance;
        localizationService.LanguageChanged += OnLanguageChanged;
        OnLanguageChanged();

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
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

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        var message = ex?.Message ?? e.ExceptionObject?.ToString() ?? "Unhandled exception";
        Debug.WriteLine(ex?.ToString() ?? message);
        try
        {
            if (MainThread.IsMainThread && MainPage != null)
            {
                var loc = LocalizationService.Instance;
                var title = loc.GetString("Error");
                var ok = loc.GetString("OK");
                MainPage.DisplayAlert(title, message, ok);
            }
        }
        catch {}
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        Debug.WriteLine(e.Exception?.ToString());
        e.SetObserved();
        var message = e.Exception?.Message ?? "Unobserved task exception";
        // Avoid UI calls here; finalizer thread may not have main thread access
        // Log for diagnostics only
        Debug.WriteLine($"UnobservedTaskException: {message}\n{e.Exception}");
    }
}
