using System;
using System.Windows;

namespace EDEngineer.Views.Graphics
{
    /// <summary>
    /// Interaction logic for GraphicSettingsWindow.xaml
    /// </summary>
    public partial class GraphicSettingsWindow
    {
        private readonly GraphicSettings settings;

        private readonly float beforeLeftRatio;
        private readonly float beforeBottomRatio;
        private readonly float beforeRightRatio;

        public GraphicSettingsWindow(GraphicSettings settings)
        {
            this.settings = settings;
            DataContext = settings;

            beforeLeftRatio = settings.LeftRatio;
            beforeRightRatio = settings.RightRatio;
            beforeBottomRatio = settings.BottomRatio;

            InitializeComponent();
        }

        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonResetClicked(object sender, RoutedEventArgs e)
        {
            settings.Reset(100, 100, 100);
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
                settings.Reset(beforeLeftRatio, beforeRightRatio, beforeBottomRatio);
            }
        }
    }
}
