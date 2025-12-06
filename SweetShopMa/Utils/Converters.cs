using System.Globalization;

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

