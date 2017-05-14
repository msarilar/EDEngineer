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
        public GraphicSettingsWindow(GraphicSettings settings)
        {
            this.settings = settings;
            DataContext = settings;
            InitializeComponent();
        }

        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GraphicSettingsWindowClosed(object sender, EventArgs e)
        {
            settings.Sync();
        }

        private void ButtonResetClicked(object sender, RoutedEventArgs e)
        {
            settings.Reset();
        }
    }
}
