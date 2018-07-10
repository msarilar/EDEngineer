using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using EDEngineer.Models.State;

namespace EDEngineer.Converters
{
    public class HistoryToTop5Converter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(values[0] is StateHistory) || !(values[1] is string))
            {
                return new[] { "?" };
            }

            var history = (StateHistory)values[0];
            var name = (string) values[1];

            if (!history.Loots.TryGetValue(name, out var locations))
            {
                return new[] { "?" };
            }

            return locations.OrderByDescending(kv => kv.Value)
                     .Take(5)
                     .Select(kv => $"{kv.Key} ({kv.Value})");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}