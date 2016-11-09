using System;
using System.Globalization;
using System.Windows.Data;
using NodaTime;

namespace EDEngineer.Converters
{
    public class InstantToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}