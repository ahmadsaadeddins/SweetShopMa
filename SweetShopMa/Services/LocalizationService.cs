using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Microsoft.Maui.Storage;

namespace SweetShopMa.Services;

/// <summary>
/// Manages multi-language support (localization) for the application.
/// 
/// WHAT IS LOCALIZATIONSERVICE?
/// LocalizationService provides multi-language support, allowing the app to display
/// text in different languages (currently English and Arabic).
/// 
/// KEY FEATURES:
/// - Load localized strings from resource files (.resx)
/// - Switch between languages (English/Arabic)
/// - Handle RTL (Right-to-Left) layout for Arabic
/// - Persist language preference across app restarts
/// - Singleton pattern (one instance for entire app)
/// 
/// HOW IT WORKS:
/// - Strings are stored in resource files: Strings.resx (English), Strings.ar.resx (Arabic)
/// - GetString(key) retrieves the localized string for the current language
/// - SetLanguage(code) changes the language and notifies all subscribers
/// - Language preference is saved to device storage (Preferences)
/// 
/// RTL SUPPORT:
/// When Arabic is selected, IsRTL returns true, and the app switches to
/// right-to-left layout (text flows from right to left).
/// </summary>
public class LocalizationService : INotifyPropertyChanged
{
    // ============================================
    // SINGLETON PATTERN
    // ============================================
    
    /// <summary>
    /// Singleton instance (only one instance exists for entire app).
    /// </summary>
    private static LocalizationService _instance;
    
    /// <summary>
    /// Current culture (language) - defaults to system culture.
    /// </summary>
    private CultureInfo _currentCulture;
    
    /// <summary>
    /// Resource manager for loading strings from .resx files.
    /// </summary>
    private ResourceManager _resourceManager;

    // ============================================
    // EVENTS
    // ============================================
    
    /// <summary>
    /// PropertyChanged event (for INotifyPropertyChanged interface).
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    
    /// <summary>
    /// LanguageChanged event - fired when language is changed.
    /// Views can subscribe to this to update their UI text.
    /// </summary>
    public event Action LanguageChanged;

    // ============================================
    // CONSTRUCTOR (Private - Singleton pattern)
    // ============================================
    
    /// <summary>
    /// Private constructor (Singleton pattern - only one instance can exist).
    /// Initializes resource manager and loads saved language preference.
    /// </summary>
    private LocalizationService()
    {
        // Initialize resource manager to load strings from .resx files
        _resourceManager = new ResourceManager("SweetShopMa.Resources.Strings", typeof(LocalizationService).Assembly);
        _currentCulture = CultureInfo.CurrentCulture;
        
        // Load saved language preference (defaults to "en" if not set)
        var savedLanguage = Preferences.Get("SelectedLanguage", "en");
        SetLanguage(savedLanguage);
    }

    /// <summary>
    /// Singleton instance property.
    /// Creates instance on first access (lazy initialization).
    /// </summary>
    public static LocalizationService Instance => _instance ??= new LocalizationService();

    // ============================================
    // PROPERTIES
    // ============================================
    
    /// <summary>
    /// Current culture (language) being used.
    /// When set, updates thread culture and fires LanguageChanged event.
    /// </summary>
    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (_currentCulture != value)
            {
                _currentCulture = value;
                // Set thread culture so DateTime, Number formatting uses correct culture
                CultureInfo.DefaultThreadCurrentCulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
                // Notify that culture changed
                OnPropertyChanged();
                // Fire LanguageChanged event so views can update
                LanguageChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Returns true if current language is RTL (Right-to-Left).
    /// Arabic ("ar") is RTL, English ("en") is LTR (Left-to-Right).
    /// </summary>
    public bool IsRTL => _currentCulture.TwoLetterISOLanguageName == "ar";

    // ============================================
    // METHODS
    // ============================================
    
    /// <summary>
    /// Gets a localized string by key.
    /// 
    /// HOW IT WORKS:
    /// 1. Looks up the key in the resource file for current culture
    /// 2. Returns the localized string if found
    /// 3. Returns "[key]" if not found (so missing keys are visible)
    /// 
    /// EXAMPLE:
    /// GetString("Login") returns "Login" in English, "تسجيل الدخول" in Arabic
    /// </summary>
    /// <param name="key">Key to look up (e.g., "Login", "Password")</param>
    /// <returns>Localized string or "[key]" if not found</returns>
    public string GetString(string key)
    {
        try
        {
            // Get string from resource file for current culture
            var value = _resourceManager.GetString(key, _currentCulture);
            // Return value or "[key]" if not found
            return value ?? $"[{key}]";
        }
        catch
        {
            // If error occurs, return "[key]" so missing keys are visible
            return $"[{key}]";
        }
    }

    /// <summary>
    /// Changes the current language.
    /// 
    /// HOW IT WORKS:
    /// 1. Creates CultureInfo from language code ("en" or "ar")
    /// 2. Sets CurrentCulture (which fires LanguageChanged event)
    /// 3. Saves preference to device storage
    /// 4. Falls back to English if language code is invalid
    /// 
    /// EXAMPLE:
    /// SetLanguage("ar") switches to Arabic
    /// SetLanguage("en") switches to English
    /// </summary>
    /// <param name="languageCode">Language code ("en" for English, "ar" for Arabic)</param>
    public void SetLanguage(string languageCode)
    {
        try
        {
            // Create culture from language code
            var culture = new CultureInfo(languageCode);
            // Set current culture (this fires LanguageChanged event)
            CurrentCulture = culture;
            // Save preference to device storage (persists across app restarts)
            Preferences.Set("SelectedLanguage", languageCode);
        }
        catch
        {
            // If language code is invalid, fallback to English
            CurrentCulture = new CultureInfo("en");
        }
    }

    /// <summary>
    /// Gets the current language code (two-letter ISO code).
    /// Returns "en" for English, "ar" for Arabic.
    /// </summary>
    public string CurrentLanguage => _currentCulture.TwoLetterISOLanguageName;

    // ============================================
    // INotifyPropertyChanged IMPLEMENTATION
    // ============================================
    
    /// <summary>
    /// Raises PropertyChanged event.
    /// </summary>
    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

