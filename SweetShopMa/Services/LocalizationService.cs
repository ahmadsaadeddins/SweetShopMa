using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Microsoft.Maui.Storage;

namespace SweetShopMa.Services;

public class LocalizationService : INotifyPropertyChanged
{
    private static LocalizationService _instance;
    private CultureInfo _currentCulture;
    private ResourceManager _resourceManager;

    public event PropertyChangedEventHandler PropertyChanged;
    public event Action LanguageChanged;

    private LocalizationService()
    {
        _resourceManager = new ResourceManager("SweetShopMa.Resources.Strings", typeof(LocalizationService).Assembly);
        _currentCulture = CultureInfo.CurrentCulture;
        
        // Try to load saved language preference
        var savedLanguage = Preferences.Get("SelectedLanguage", "en");
        SetLanguage(savedLanguage);
    }

    public static LocalizationService Instance => _instance ??= new LocalizationService();

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        private set
        {
            if (_currentCulture != value)
            {
                _currentCulture = value;
                CultureInfo.DefaultThreadCurrentCulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
                OnPropertyChanged();
                LanguageChanged?.Invoke();
            }
        }
    }

    public bool IsRTL => _currentCulture.TwoLetterISOLanguageName == "ar";

    public string GetString(string key)
    {
        try
        {
            var value = _resourceManager.GetString(key, _currentCulture);
            return value ?? $"[{key}]";
        }
        catch
        {
            return $"[{key}]";
        }
    }

    public void SetLanguage(string languageCode)
    {
        try
        {
            var culture = new CultureInfo(languageCode);
            CurrentCulture = culture;
            Preferences.Set("SelectedLanguage", languageCode);
        }
        catch
        {
            // Fallback to English if language code is invalid
            CurrentCulture = new CultureInfo("en");
        }
    }

    public string CurrentLanguage => _currentCulture.TwoLetterISOLanguageName;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

