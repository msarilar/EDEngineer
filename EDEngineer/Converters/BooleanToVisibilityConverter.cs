using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bind = (bool)value;
            bool visible;
            if (parameter != null && (string)parameter == "Inverted")
            {
                visible = !bind;
            }
            else
            {
                visible = bind;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}