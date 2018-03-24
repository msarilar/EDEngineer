using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EDEngineer.Models.Loadout;

namespace EDEngineer.Converters
{
    public class ModuleModifierToColorConverter : IValueConverter
    {
        private readonly SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private readonly SolidColorBrush green = new SolidColorBrush(Colors.Green);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return green;
            }

            var modifier = (ModuleModifier)value;
            var diff = modifier.Value - modifier.OriginalValue;

            if (modifier.LessIsGood)
            {
                diff *= -1;
            }

            return diff < 0 ? red : green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}