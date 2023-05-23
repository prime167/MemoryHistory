using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MemoryUsageAvalonia;

public class Trailing0Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var v = (double)value;
        return v.ToString("0.0");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}