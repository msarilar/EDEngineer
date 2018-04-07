using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class KindToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var kind = (Kind) value;

            return Inclusive == (kind == SpecialKind) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public Kind SpecialKind { get; set; }
        public bool Inclusive { get; set; }
    }
}