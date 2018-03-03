using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Models;
using EDEngineer.Models.Utils;

namespace EDEngineer.Converters
{
    public class RarityToMaximumCapacity : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rarity = (Rarity)value;

            return rarity.MaximumCapacity();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}