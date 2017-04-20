using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Views.Notifications;
using EDEngineer.Views.Popups;

namespace EDEngineer.Converters
{
    public class NotificationKindToBooleanConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (NotificationKind) value != NotificationKind.None;
        }
    }
}