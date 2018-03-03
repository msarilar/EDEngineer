using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class IntOrNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.TryParse((string)value, out var result) ? result : (int?)null;
        }
    }
}