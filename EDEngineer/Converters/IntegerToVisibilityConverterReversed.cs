using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class IntegerToVisibilityConverterReversed : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var threshold = parameter == null ? 0 : int.Parse((string)parameter);
            return (int)value > threshold ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}