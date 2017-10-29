using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class SubkindToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            switch ((Subkind)value)
            {
                case Subkind.Raw:
                    return "/Resources/Images/raw.png";
                case Subkind.Manufactured:
                    return "/Resources/Images/manufactured.png";
                default:
                    throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}