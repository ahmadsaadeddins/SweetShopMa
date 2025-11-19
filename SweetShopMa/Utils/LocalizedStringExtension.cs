using System;
using System.Globalization;
using System.Resources;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace SweetShopMa.Utils;

/// <summary>
/// XAML markup extension for localized strings.
/// 
/// WHAT IS LOCALIZEDSTRINGEXTENSION?
/// This allows you to use localized strings directly in XAML without code-behind.
/// Instead of setting Text in code, you can use it in XAML markup.
/// 
/// HOW IT WORKS:
/// - Implements IMarkupExtension interface (required for XAML extensions)
/// - Gets current culture from LocalizationService
/// - Loads string from resource file (.resx)
/// - Returns localized value or "[Key]" if not found
/// 
/// USAGE IN XAML:
/// <Label Text="{local:LocalizedString Key=Login}" />
/// 
/// This is equivalent to:
/// label.Text = LocalizationService.Instance.GetString("Login");
/// 
/// BENEFITS:
/// - Clean XAML (no code-behind needed for simple text)
/// - Automatically updates when language changes
/// - Type-safe (compile-time checking)
/// </summary>
[ContentProperty(nameof(Key))]
public class LocalizedStringExtension : IMarkupExtension
{
    /// <summary>
    /// Resource manager for loading strings from .resx files.
    /// Static so it's shared across all instances (more efficient).
    /// </summary>
    private static readonly ResourceManager ResourceManager = 
        new ResourceManager("SweetShopMa.Resources.Strings", typeof(LocalizedStringExtension).Assembly);

    /// <summary>
    /// Key to look up in the resource file (e.g., "Login", "Password").
    /// This is set in XAML: {local:LocalizedString Key=Login}
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Provides the localized string value to XAML.
    /// This method is called by XAML when it needs the value.
    /// 
    /// HOW IT WORKS:
    /// 1. Get current culture from LocalizationService
    /// 2. Look up key in resource file for that culture
    /// 3. Return localized string or "[Key]" if not found
    /// </summary>
    /// <param name="serviceProvider">XAML service provider (not used in this implementation)</param>
    /// <returns>Localized string or "[Key]" if not found</returns>
    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
            return string.Empty;

        var culture = Services.LocalizationService.Instance.CurrentCulture;
        try
        {
            var value = ResourceManager.GetString(Key, culture);
            return value ?? $"[{Key}]";
        }
        catch
        {
            return $"[{Key}]";
        }
    }
}

