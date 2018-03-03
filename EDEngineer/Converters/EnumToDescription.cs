using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Models.Utils;

namespace EDEngineer.Converters
{
    public class EnumToDescription : IValueConverter
    {
        public const string UNKNOWN = "Unknown";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var content = value as Enum;

            return content?.Description() ?? UNKNOWN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}