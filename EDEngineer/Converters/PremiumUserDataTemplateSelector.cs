using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EDEngineer.Models;

namespace EDEngineer.Converters
{
    public class ShoppingListBlueprintSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var elemnt = container as FrameworkElement;
            var blueprints = (KeyValuePair<string, List<Tuple<Blueprint, int>>>) item;

            switch (blueprints.Value.First().Item1.Category)
            {
                case BlueprintCategory.Synthesis:
                    return elemnt.FindResource("ModuleTemplate") as DataTemplate;
                case BlueprintCategory.Experimental:
                    return elemnt.FindResource("ModuleTemplate") as DataTemplate;
                case BlueprintCategory.Technology:
                    return elemnt.FindResource("ModuleTemplate") as DataTemplate;
                case BlueprintCategory.Unlock:
                    return elemnt.FindResource("ModuleTemplate") as DataTemplate;
                case BlueprintCategory.Module:
                default:
                    return elemnt.FindResource("ModuleTemplate") as DataTemplate;
            }
        }
    }
}