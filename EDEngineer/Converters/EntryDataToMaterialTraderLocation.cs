using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Localization;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class EntryDataToMaterialTraderLocation : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var entryData = (EntryData) value;

            switch (entryData.Subkind)
            {
                case Subkind.Manufactured:
                    return Languages.Instance.Translate(
                        "Materials Trader found at industrial economies, only trades in manufactured materials.");
                case Subkind.Raw:
                    return Languages.Instance.Translate(
                        "Materials Trader found at extraction and refinery economies, only trades in raw material found on planet surfaces and planetary rings.");
                default:
                    switch (entryData.Kind)
                    {
                        case Kind.Data:
                            return Languages.Instance.Translate(
                                "Materials Trader found at High Tech and Military economies, only trades in encoded materials.");
                        default:
                            return null;
                    }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}