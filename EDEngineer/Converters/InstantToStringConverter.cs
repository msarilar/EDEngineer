using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Localization;
using NodaTime;

namespace EDEngineer.Converters
{
    public class InstantToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo _)
        {
            if (value is Instant instant)
            {
                var culture = CultureInfo.GetCultureInfo(Languages.Instance.CurrentLanguage.TwoLetterISOLanguageName);
                
                return instant.ToString("dd MMM yyyy HH:mm:ss", culture);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}