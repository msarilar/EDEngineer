using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EDEngineer.Converters
{
    public class ThresholdToBackgroundColorConverter : IMultiValueConverter
    {
        public SolidColorBrush ThresholdReachedColor { get; set; }
        public SolidColorBrush NormalColor { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var count = (int) values[0];
            var threshold = (int?) values[1];

            return count >= threshold ? ThresholdReachedColor : NormalColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}