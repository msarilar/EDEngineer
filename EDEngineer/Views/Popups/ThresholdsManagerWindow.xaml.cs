using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Utils.Collections;

namespace EDEngineer.Views.Popups
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ThresholdsManagerWindow
    {
        public ThresholdsManagerWindow(Languages languages, ISimpleDictionary<string, Entry> thresholds, string commander)
        {
            var viewModel = new ThresholdsManagerViewModel(languages, thresholds);
            DataContext = viewModel;

            InitializeComponent();

            CancelButton.Content = Languages.Instance.Translate("Cancel");
            OkButton.Content = Languages.Instance.Translate("OK");
            Title = $"{Languages.Instance.Translate("Configure Thresholds")} - Cmdr {commander}";
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void ShowThresholds(Languages languages, ISimpleDictionary<string, Entry> thresholds, string commander)
        {
            new ThresholdsManagerWindow(languages, thresholds, commander).Show();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ThresholdsGridCellSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                var grid = (DataGrid)sender;
                grid.BeginEdit(e);
            }
        }
    }
}
