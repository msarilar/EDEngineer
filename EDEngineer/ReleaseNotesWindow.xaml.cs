using System;
using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;

namespace EDEngineer
{
    /// <summary>
    /// Interaction logic for ReleaseNotesWindow.xaml
    /// </summary>
    public partial class ReleaseNotesWindow
    {
        public ReleaseNotesWindow(IEnumerable<Tuple<string, string>> list, string title)
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
