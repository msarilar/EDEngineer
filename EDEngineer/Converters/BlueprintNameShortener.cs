using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class BlueprintNameShortener : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (string) value;

            switch (type)
            {
                case "Electronic Countermeasure":
                    return "ECM";
                case "Hull Reinforcement Package":
                    return "Hull";
                case "Frame Shift Drive Interdictor":
                    return "FSD Interdictor";
                case "Prospector Limpet Controller":
                    return "Prospector LC";
                case "Fuel Transfer Limpet Controller":
                    return "Fuel Transfer LC";
                case "Hatch Breaker Limpet Controller":
                    return "Hatch Breaker LC";
                case "Collector Limpet Controller":
                    return "Collector LC";
                case "Auto Field-Maintenance Unit":
                    return "AFMU";
            }

            return type;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}