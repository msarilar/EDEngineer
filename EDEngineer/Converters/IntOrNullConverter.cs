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
            int result;
            return int.TryParse((string) value, out result) ? result : (int?) null;
        }
    }
}