using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Models.Filters;

namespace EDEngineer.Converters
{
    public class FilterToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((BlueprintFilter) value).Magic)
            {
                return "All";
            }

            var typeFilter = value as TypeFilter;
            if (typeFilter != null)
            {
                return typeFilter.Type;
            }

            var gradeFilter = value as GradeFilter;
            if (gradeFilter != null)
            {
                return gradeFilter.Grade;
            }

            var engineerFilter = value as EngineerFilter;
            if (engineerFilter != null)
            {
                return engineerFilter.Engineer;
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}