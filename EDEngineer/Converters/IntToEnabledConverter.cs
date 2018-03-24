using System;
using System.Globalization;
using System.Windows.Data;

namespace EDEngineer.Converters
{
    public class IntToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var integer = (int?) value;
            switch (Comparison)
            {
                case Comparison.MoreThan:
                    return integer > Threshold;
                case Comparison.LessThan:
                    return integer < Threshold;
                case Comparison.DifferentThan:
                default:
                    return integer != Threshold;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public int Threshold { get; set; }

        public Comparison Comparison { get; set; }
    }

    public enum Comparison
    {
        DifferentThan,
        MoreThan,
        LessThan
    }
}