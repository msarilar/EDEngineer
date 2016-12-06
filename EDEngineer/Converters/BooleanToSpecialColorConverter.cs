using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EDEngineer.Converters
{
    public class BooleanToSpecialColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var synthesis = (bool)value;
            return synthesis ? SpecialColor : NormalColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public SolidColorBrush SpecialColor { get; set; }
        public SolidColorBrush NormalColor { get; set; }
    }
}