using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace EDEngineer.Utils.UI
{
    public class PersistGroupExpandedStateBehavior : Behavior<Expander>
    {
        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
            "GroupName",
            typeof(object),
            typeof(PersistGroupExpandedStateBehavior),
            new PropertyMetadata(default(object)));

        public object GroupName
        {
            get
            {
                return GetValue(GroupNameProperty);
            }

            set
            {
                SetValue(GroupNameProperty, value);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            bool? expanded = GetExpandedState();

            if (expanded != null)
            {
                AssociatedObject.IsExpanded = expanded.Value;
            }

            AssociatedObject.Expanded += OnExpanded;
            AssociatedObject.Collapsed += OnCollapsed;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Expanded -= OnExpanded;
            AssociatedObject.Collapsed -= OnCollapsed;

            base.OnDetaching();
        }

        private bool GetExpandedState()
        {
            if (GroupName == null)
            {
                return false;
            }

            return !System.SettingsManager.IngredientGroups.IsCollapsed(GroupName.ToString());
        }

        private void OnCollapsed(object sender, RoutedEventArgs e)
        {
            SetExpanded(false);
        }

        private void OnExpanded(object sender, RoutedEventArgs e)
        {
            SetExpanded(true);
        }

        private void SetExpanded(bool expanded)
        {
            if(expanded)
            {
                System.SettingsManager.IngredientGroups.Expand(GroupName.ToString());
            }
            else
            {
                System.SettingsManager.IngredientGroups.Collapse(GroupName.ToString());
            }
        }
    }
}
