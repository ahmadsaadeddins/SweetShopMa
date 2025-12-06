using SweetShopMa.Services;
using SweetShopMa.ViewModels;
using SweetShopMa.Views;
using SweetShopMa;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using SweetShopMa.Platforms.Windows;
#endif

namespace SweetShopMa;

/// <summary>
/// MauiProgram is the entry point for configuring the MAUI application.
/// This class sets up dependency injection, registers services, and configures the app.
/// 
/// WHAT IS DEPENDENCY INJECTION?
/// Dependency Injection (DI) is a design pattern where objects receive their dependencies
/// from an external source rather than creating them internally. This makes code more
/// testable, flexible, and maintainable.
/// 
/// HOW IT WORKS:
/// 1. Services are registered here with their lifetime (Singleton, Transient, Scoped)
/// 2. When a class needs a service, the DI container automatically provides it
/// 3. For example, ShopViewModel needs DatabaseService - DI provides it automatically
/// 
/// SERVICE LIFETIMES:
/// - Singleton: One instance for the entire app lifetime (shared across all uses)
/// - Transient: New instance created each time it's requested
/// - Scoped: One instance per scope (not used in this app)
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Creates and configures the MAUI application.
    /// This method is called automatically when the app starts.
    /// </summary>
    public static MauiApp CreateMauiApp()
    {
        // Create a builder to configure the app
        var builder = MauiApp.CreateBuilder();
        
        // Configure the app
        builder
            // Set the main App class (App.xaml.cs)
            .UseMauiApp<App>()
            
            // Register fonts that can be used throughout the app
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            
            // ============================================
            // REGISTER SERVICES (Dependency Injection)
            // ============================================
            
            // Core Services - Singleton (one instance for entire app)
            // These services maintain state and should be shared across the app
            
            .Services.AddSingleton<DatabaseService>()  // Database operations (CRUD)
            .AddSingleton<AuthService>()                // User authentication
            .AddSingleton<CartService>()                // Shopping cart management
            .AddSingleton<AttendanceRulesService>()     // Attendance rules
            .AddSingleton<Services.LocalizationService>(_ => Services.LocalizationService.Instance)  // Multi-language support (uses singleton instance)
            .AddSingleton<IPdfService, PdfService>()   // PDF generation service
            
            // Platform-Specific Services
            // On Windows, use Windows-specific implementations
            // On other platforms, use default implementations
#if WINDOWS
            .AddSingleton<IPrintService, WindowsPrintService>()           // Windows: HTML-based printing
            .AddSingleton<ICashDrawerService, WindowsCashDrawerService>()  // Windows: ESC/POS cash drawer
#else
            .AddSingleton<IPrintService, DefaultPrintService>()           // Other platforms: Share API fallback
            .AddSingleton<ICashDrawerService, DefaultCashDrawerService>() // Other platforms: Not supported
#endif
            
            // ViewModels - Singleton (maintain state across navigation)
            // ViewModels hold business logic and should persist their state
            .AddSingleton<ShopViewModel>()      // Main shop interface logic
            .AddSingleton<AdminViewModel>()   // Admin panel logic
            
            // Views - Different lifetimes based on usage
            
            // Singleton: MainPage is the main interface, should persist state
            .AddSingleton<Views.MainPage>()
            
            // Transient: These pages should be fresh each time they're created
            // Transient means a new instance is created each time it's requested
            .AddTransient<Views.LoginPage>()              // Login page (fresh each time for security)
            .AddTransient<Views.DeveloperSetupPage>()     // User creation form (fresh form each time)
            .AddTransient<Views.AdminPage>()               // Admin panel (can be recreated)
            .AddTransient<Views.AttendancePage>()         // Attendance page (can be recreated)
            .AddTransient<ViewModels.RestockReportViewModel>()  // Restock report ViewModel
            .AddTransient<Views.RestockReportPage>();     // Restock report page

        // ============================================
        // PLATFORM-SPECIFIC CONFIGURATION
        // ============================================
        
        // Configure lifecycle events (app startup, window creation, etc.)
        builder.ConfigureLifecycleEvents(events =>
        {
#if WINDOWS
            // Windows-specific: Maximize window on startup
            events.AddWindows(w =>
            {
                w.OnWindowCreated(window =>
                {
                    var appWindow = window.GetAppWindow();
                    if (appWindow.Presenter is OverlappedPresenter presenter)
                    {
                        // Maximize the window when app starts
                        presenter.Maximize();
                        // Allow user to resize and maximize/minimize
                        presenter.IsResizable = true;
                        presenter.IsMaximizable = true;
                    }
                    else
                    {
                        // Fallback: Use fullscreen if presenter type is different
                        appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                    }
                });
            });
#endif
        });
        
        // Build and return the configured app
        // This creates the DI container with all registered services
        return builder.Build();
    }
}
