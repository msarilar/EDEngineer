using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class IngredientToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ingredient = (BlueprintIngredient) value;
            bool visible;
            if (parameter != null && (string) parameter == "Inverted")
            {
                visible = ingredient.Entry.Count < ingredient.Size;
            }
            else
            {
                visible = ingredient.Entry.Count >= ingredient.Size;
            }

            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}