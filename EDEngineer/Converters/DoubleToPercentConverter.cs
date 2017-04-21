using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class DoubleToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter as string == "precise")
            {
                return ((double)value).ToString("0.00") + "%";
            }
            else
            {
                return ((double)value).ToString("0") + "%";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}