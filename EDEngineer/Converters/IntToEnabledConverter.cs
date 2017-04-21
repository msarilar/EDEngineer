using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class IntToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var integer = (int?) value;
            return integer != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}