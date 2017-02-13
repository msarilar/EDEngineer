using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EDEngineer.Converters
{
    public class QuantityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var quantity = (int?)value;            
            return 
                !quantity.HasValue ? NormalColor 
                : quantity > 0 ? TooManyColor 
                : quantity < 0 ? NotEnoughColor 
                : NormalColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public SolidColorBrush TooManyColor { get; set; }
        public SolidColorBrush NormalColor { get; set; }
        public SolidColorBrush NotEnoughColor { get; set; }
    }
}