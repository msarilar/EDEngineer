using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class ShoppingListBlockToAllGradesVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }
            var block = (ShoppingListBlock) value;
            return block.Composition.Count + block.HiddenBlueprints.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}