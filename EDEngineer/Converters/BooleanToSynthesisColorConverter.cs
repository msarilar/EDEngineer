using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EDEngineer.Converters
{
    public class BooleanToSynthesisColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var synthesis = (bool)value;
            return synthesis ? SynthesisColor : NormalColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public SolidColorBrush SynthesisColor { get; set; }
        public SolidColorBrush NormalColor { get; set; }
    }
}