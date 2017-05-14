using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace EDEngineer.Utils.UI
{
    public class ScaleFontBehavior : Behavior<Grid>
    {
        private readonly Dictionary<string, double> fontSizes = new Dictionary<string, double>(); 

        public double FontRatio { get { return (double)GetValue(FontRatioProperty); } set { SetValue(FontRatioProperty, value); } }
        public static readonly DependencyProperty FontRatioProperty = DependencyProperty.Register("FontRatio", typeof(double), typeof(ScaleFontBehavior), new PropertyMetadata(20d));

        protected override void OnAttached()
        {
            AssociatedObject.SizeChanged += (s, e) => { CalculateFontSize(); };
            AssociatedObject.LayoutUpdated += (s, e) => { CalculateFontSize(); };
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            CalculateFontSize();
        }

        private void CalculateFontSize()
        {
            var tbs = FindVisualChildren<TextBlock>(AssociatedObject);
            foreach (var tb in tbs)
            {
                double fontSize;
                if (!fontSizes.TryGetValue(tb.Name, out fontSize))
                {
                    if (tb.Name == string.Empty)
                    {
                        tb.Name = "_" + Guid.NewGuid().ToString().Replace("-", "");
                    }

                    fontSizes[tb.Name] = tb.FontSize;
                    fontSize = tb.FontSize;
                }

                tb.FontSize = fontSize * FontRatio / 100d;
            }
        }

        public static List<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            var children = new List<T>();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var o = VisualTreeHelper.GetChild(obj, i);
                if (o != null)
                {
                    if (o is T)
                    {
                        children.Add((T)o);
                    }

                    children.AddRange(FindVisualChildren<T>(o));
                }
            }
            return children;
        }

        public static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            var current = initial;

            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }
}
