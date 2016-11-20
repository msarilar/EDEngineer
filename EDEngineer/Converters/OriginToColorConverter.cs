using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class OriginToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var origin = (Origin) value;
            Color color;
            switch (origin)
            {
                case Origin.Mission:
                    color = Colors.MediumVioletRed;
                    break;
                case Origin.Mining:
                    color = Colors.DeepSkyBlue;
                    break;
                case Origin.Scan:
                    color = Colors.Wheat;
                    break;
                case Origin.Salvage:
                    color = Colors.MediumPurple;
                    break;
                case Origin.Surface:
                    color = Colors.GreenYellow;
                    break;
                case Origin.Market:
                    color = Colors.OrangeRed;
                    break;
                case Origin.Unknown:
                    color = Colors.RoyalBlue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}