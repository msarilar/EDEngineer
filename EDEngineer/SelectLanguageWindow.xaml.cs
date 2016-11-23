using System.Diagnostics;
using System.Windows;
using EDEngineer.Localization;

namespace EDEngineer
{
    /// <summary>
    /// Interaction logic for SelectLanguageWindow.xaml
    /// </summary>
    public partial class SelectLanguageWindow
    {
        public SelectLanguageWindow(Languages languages)
        {
            InitializeComponent();
            DataContext = languages;
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HelpTranslateButtonClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/msarilar/EDEngineer/issues/32");
        }
    }
}
