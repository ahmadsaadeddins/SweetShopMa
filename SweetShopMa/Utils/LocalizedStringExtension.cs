using System;
using System.Globalization;
using System.Resources;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace SweetShopMa.Utils;

[ContentProperty(nameof(Key))]
public class LocalizedStringExtension : IMarkupExtension
{
    private static readonly ResourceManager ResourceManager = 
        new ResourceManager("SweetShopMa.Resources.Strings", typeof(LocalizedStringExtension).Assembly);

    public string Key { get; set; }

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

