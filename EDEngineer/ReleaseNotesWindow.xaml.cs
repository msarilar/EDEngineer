using System.Collections.Generic;
using System.Windows;
using EDEngineer.Models;

namespace EDEngineer
{
    /// <summary>
    /// Interaction logic for ReleaseNotesWindow.xaml
    /// </summary>
    public partial class ReleaseNotesWindow
    {
        public ReleaseNotesWindow(IEnumerable<ReleaseNote> list, string title)
        {
            InitializeComponent();
            Title = title;
            ReleaseNotesTabs.ItemsSource = list;
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
