using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class FontRatioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ratio = (float) value;
            var fontSize = float.Parse((string) parameter, CultureInfo.InvariantCulture);

            return ratio * fontSize / 100f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}