using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class ApiRunningToIndicatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return "/Resources/Images/api-on.png";
            }
            else
            {
                return "/Resources/Images/api-off.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}