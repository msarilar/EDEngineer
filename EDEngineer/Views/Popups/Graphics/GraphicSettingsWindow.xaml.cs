using System;
using System.Windows;

namespace EDEngineer.Views.Popups.Graphics
{
    /// <summary>
    /// Interaction logic for GraphicSettingsWindow.xaml
    /// </summary>
    public partial class GraphicSettingsWindow
    {
        private readonly GraphicSettings settings;

        private readonly double beforeLeftRatio;
        private readonly double beforeBottomRatio;
        private readonly double beforeRightRatio;
        private readonly double beforeOpacity;

        public GraphicSettingsWindow(GraphicSettings settings)
        {
            this.settings = settings;
            DataContext = settings;

            beforeLeftRatio = settings.LeftRatio;
            beforeRightRatio = settings.RightRatio;
            beforeBottomRatio = settings.BottomRatio;
            beforeOpacity = settings.Opacity;

            InitializeComponent();
        }

        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonResetClicked(object sender, RoutedEventArgs e)
        {
            settings.Reset(100, 100, 100, 0.9d);
        }

        private void ButtonCancelClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GraphicSettingsWindowClosed(object sender, EventArgs e)
        {
            if (DialogResult == true)
            {
                settings.Save();
            }
            else
            {
                settings.Reset(beforeLeftRatio, beforeRightRatio, beforeBottomRatio, beforeOpacity);
            }
        }
    }
}
