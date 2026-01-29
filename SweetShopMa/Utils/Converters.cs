using System.Globalization;
using SweetShopMa.Services;

namespace SweetShopMa.Utils;

/// <summary>
/// Value converters for XAML bindings
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EqualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class GreaterThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        if (double.TryParse(value.ToString(), out double v) && double.TryParse(parameter.ToString(), out double p))
        {
            return v > p;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LessThanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        if (double.TryParse(value.ToString(), out double v) && double.TryParse(parameter.ToString(), out double p))
        {
            return v < p;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return value;
    }
}

/// <summary>
/// Currency converter for XAML binding that uses dynamic shop settings.
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal amount)
        {
            var currency = parameter as string ?? "EGP";
            var isArabic = Services.LocalizationService.Instance.IsArabic;
            return CurrencyHelper.FormatCurrency(amount, currency, isArabic);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToFlowDirectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRTL && isRTL)
            return FlowDirection.RightToLeft;
        return FlowDirection.LeftToRight;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FlowDirection flowDirection)
            return flowDirection == FlowDirection.RightToLeft;
        return false;
    }
}

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.ToString()?.Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Currency formatting utilities.
/// </summary>
public static class CurrencyHelper
{
    /// <summary>
    /// Formats an amount with the appropriate currency symbol and decimal places.
    /// Defaults to EGP for Egyptian Pound if currency is not specified.
    /// </summary>
    /// <param name="amount">The amount to format</param>
    /// <param name="currency">The currency code (defaults to "EGP")</param>
    /// <param name="isArabic">Whether to use Arabic formatting</param>
    /// <returns>Formatted currency string</returns>
    public static string FormatCurrency(decimal amount, string currency = "EGP", bool isArabic = false)
    {
        if (string.IsNullOrEmpty(currency))
            currency = "EGP";
        
        var numericPart = $"{amount:F2}";
        
        return isArabic ? $"{numericPart} {GetArabicCurrencySymbol(currency)}" : $"{GetCurrencySymbol(currency)} {numericPart}";
    }
    
    /// <summary>
    /// Gets the currency symbol for a given currency code.
    /// </summary>
    /// <param name="currency">The currency code</param>
    /// <returns>Currency symbol</returns>
    public static string GetCurrencySymbol(string currency)
    {
        return currency?.ToUpper() switch
        {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "EGP" => "ج.م.",
            _ => currency ?? "$" // Use code as fallback
        };
    }
    
    /// <summary>
    /// Gets the Arabic currency symbol for a given currency code.
    /// </summary>
    /// <param name="currency">The currency code</param>
    /// <returns>Arabic currency symbol</returns>
    public static string GetArabicCurrencySymbol(string currency)
    {
        return currency?.ToUpper() switch
        {
            "USD" => "دولار",
            "EUR" => "يورو",
            "GBP" => "جنيه",
            "EGP" => "ج.م.",
            _ => currency ?? "دولار" // Use code as fallback
        };
    }
}

