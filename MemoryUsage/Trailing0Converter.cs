using System.Globalization;
using System.Windows.Data;

namespace MemoryUsage;

public class Trailing0Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var v = (double)value;
        return v.ToString("0.00");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}