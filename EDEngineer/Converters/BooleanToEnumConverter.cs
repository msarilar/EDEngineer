using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class BooleanToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (parameterString == null || value == null)
            {
                return false;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return false;
            }

            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterString = parameter as string;
            if (parameterString == null)
            {
                return null;
            }

            return Enum.Parse(targetType, parameterString);
        }
    }
}