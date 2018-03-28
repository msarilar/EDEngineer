using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class CategoryToCommodityWarningVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is BlueprintCategory) || (BlueprintCategory) value != BlueprintCategory.Technology)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}