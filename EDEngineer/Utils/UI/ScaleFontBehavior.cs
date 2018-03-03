using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using EDEngineer.Utils.System;

namespace EDEngineer.Utils.UI
{
    using DependencyObjectAccessor = Tuple<string, Action<string>, double, Action<double>>;

    public class ScaleFontBehavior : Behavior<Grid>
    {
        private readonly Dictionary<string, double> fontSizes = new Dictionary<string, double>(); 

        public double FontRatio
        {
            get => (double)GetValue(FontRatioProperty);
            set => SetValue(FontRatioProperty, value);
        }
        public static readonly DependencyProperty FontRatioProperty = DependencyProperty.Register("FontRatio", typeof(double), typeof(ScaleFontBehavior), new PropertyMetadata(20d));
        private readonly PostponeScheduler scheduler;
        private bool disposed;

        public ScaleFontBehavior()
        {
            scheduler = new PostponeScheduler(CalculateFontSize, 100);
        }

        protected override void OnAttached()
        {
            AssociatedObject.SizeChanged += (s, e) => { if(!disposed) scheduler.Schedule(); };
            AssociatedObject.LayoutUpdated += (s, e) => { if (!disposed) scheduler.Schedule(); };
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            scheduler.Schedule();
        }

        protected override void OnDetaching()
        {
            disposed = true;
            scheduler.Dispose();
        }

        private void CalculateFontSize()
        {
            var textBlocks = FindVisualChildren<TextBlock>(AssociatedObject)
                .Select(b => new DependencyObjectAccessor(b.Name, s => b.Name = s, b.FontSize, s => b.FontSize = s));
            var textBoxes = FindVisualChildren<TextBox>(AssociatedObject)
                .Select(b => new DependencyObjectAccessor(b.Name, s => b.Name = s, b.FontSize, s => b.FontSize = s));

            foreach (var tb in textBlocks.Union(textBoxes))
            {
                if (!fontSizes.TryGetValue(tb.Item1, out var fontSize))
                {
                    string name;
                    if (tb.Item1 == string.Empty)
                    {
                        name = "_" + Guid.NewGuid().ToString().Replace("-", "");
                        tb.Item2(name);
                    }
                    else
                    {
                        name = tb.Item1;
                    }

                    fontSizes[name] = tb.Item3;
                    fontSize = tb.Item3;
                }

                tb.Item4(fontSize * FontRatio / 100d);
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
                    if (o is T item)
                    {
                        children.Add(item);
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
