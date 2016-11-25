using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using EDEngineer.Localization;
using EDEngineer.Models.Filters;

namespace EDEngineer.Converters
{
    public class AllFiltersToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var translator = Languages.Instance;

            var builder = new StringBuilder();

            switch ((string) parameter)
            {
                case "Engineer":
                    builder.Append(translator.Translate("Engineer Filter"));
                    break;
                case "Grade":
                    builder.Append(translator.Translate("Grade Filter"));
                    break;
                case "Type":
                    builder.Append(translator.Translate("Type Filter"));
                    break;
                case "Craftable":
                    builder.Append(translator.Translate("Craftable Filter"));
                    break;
                case "IgnoredAndFavorite":
                    builder.Append(translator.Translate("Ignored And Favorite Filter"));
                    break;
                case "Ingredients":
                    return translator.Translate("Ingredient Filter (Reversed)");
                default:
                    throw new NotImplementedException();
            }
            var filters = (IEnumerable<BlueprintFilter>) value;

            var blueprintFilters = filters as IList<BlueprintFilter> ?? filters.ToList();
            builder.Append($" ({blueprintFilters.Count(f => !f.Magic && f.Checked)}/{blueprintFilters.Count(f => !f.Magic)})");

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}