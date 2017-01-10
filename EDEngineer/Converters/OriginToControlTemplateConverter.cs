using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class OriginToControlTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var origin = (Origin)value;
            switch (origin)
            {
                case Origin.Mission:
                    return Application.Current.FindResource("OriginMission");
                case Origin.Mining:
                    return Application.Current.FindResource("OriginMining");
                case Origin.Scan:
                    return Application.Current.FindResource("OriginScan");
                case Origin.Salvage:
                    return Application.Current.FindResource("OriginSalvage");
                case Origin.Surface:
                    return Application.Current.FindResource("OriginSurface");
                case Origin.Market:
                    return Application.Current.FindResource("OriginMarket");
                case Origin.Unknown:
                    return Application.Current.FindResource("OriginUnknown");
                case Origin.NeededForEngineer:
                    return Application.Current.FindResource("NeededForEngineer");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}