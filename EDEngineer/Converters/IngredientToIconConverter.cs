using System;
using System.Globalization;
using System.Windows.Data;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class IngredientToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }

            var ingredient = (EntryData)value;

            if(ingredient.Kind == Kind.OdysseyIngredient)
            {
                switch (ingredient.Group)
                {
                    case Group.Chemicals:
                        return "/Resources/Images/odyssey-chemicals.png";
                    case Group.Tech:
                        return "/Resources/Images/odyssey-tech.png";
                    case Group.Circuits:
                        return "/Resources/Images/odyssey-circuits.png";
                    case Group.Item:
                        return "/Resources/Images/odyssey-item.png";
                    case Group.Data:
                        return "/Resources/Images/odyssey-data.png";
                    case Group.Consumable:
                        return "/Resources/Images/odyssey-item.png";
                    default:
                        throw new ArgumentException("Invalid group for Odyssey Ingredient " + ingredient.Group);
                }
            }
            else
            {
                return RarityToIconConverter.ConvertRarityToIcon(ingredient.Rarity);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}