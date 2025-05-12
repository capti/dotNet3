using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace dot.Converters
{
    public class AddValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string offsetString)
            {
                if (double.TryParse(offsetString, out double offset))
                {
                    return doubleValue + offset;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 