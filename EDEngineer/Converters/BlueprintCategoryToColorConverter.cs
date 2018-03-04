using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class BlueprintCategoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var category = (BlueprintCategory) value;
            switch(category)
            {
                case BlueprintCategory.Module:
                    return ModuleColor;
                case BlueprintCategory.Synthesis:
                    return SynthesisColor;
                case BlueprintCategory.Experimental:
                    return ExperimentalColor;
                case BlueprintCategory.Technology:
                    return TechnologyColor;
                case BlueprintCategory.Unlock:
                    return UnlockColor;
                default:
                    return ModuleColor;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public SolidColorBrush ModuleColor { get; set; }
        public SolidColorBrush SynthesisColor { get; set; }
        public SolidColorBrush UnlockColor { get; set; }
        public SolidColorBrush TechnologyColor { get; set; }
        public SolidColorBrush ExperimentalColor { get; set; }
    }
}