using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class IngredientToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var current = (int)values[0];
            var target = (int)values[1];
            bool visible;
            if (parameter != null && (string)parameter == "Inverted")
            {
                visible = current < target;
            }
            else
            {
                visible = current >= target;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}