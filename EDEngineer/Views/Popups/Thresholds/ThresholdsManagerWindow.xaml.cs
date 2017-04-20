using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Utils.Collections;
using EDEngineer.Utils.System;

namespace EDEngineer.Views.Popups.Thresholds
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ThresholdsManagerWindow
    {
        private readonly ThresholdsManagerViewModel viewModel;

        public ThresholdsManagerWindow(Languages languages, ISimpleDictionary<string, Entry> thresholds, string commander)
        {
            viewModel = new ThresholdsManagerViewModel(languages, thresholds);
            DataContext = viewModel;

            InitializeComponent();

            Title = $"{Languages.Instance.Translate("Configure Thresholds")} - Cmdr {commander}";
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        public static void ShowThresholds(Languages languages, ISimpleDictionary<string, Entry> thresholds, string commander)
        {
            if (new ThresholdsManagerWindow(languages, thresholds, commander).ShowDialog() == true)
            {
                SettingsManager.Thresholds = thresholds.Values.ToDictionary(e => e.Data.Name, e => e.Threshold);
            }
            else
            {
                foreach (var item in SettingsManager.Thresholds)
                {
                    thresholds[item.Key].Threshold = item.Value;
                }
            }
        }

        public static void InitThresholds(ISimpleDictionary<string, Entry> thresholds)
        {
            if (SettingsManager.Thresholds == null)
            {
                SettingsManager.Thresholds = thresholds.Values.ToDictionary(e => e.Data.Name, e => e.Threshold);
            }
            else
            {
                foreach (var item in SettingsManager.Thresholds)
                {
                    thresholds[item.Key].Threshold = item.Value;
                }
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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

        private void ApplyButtonClicked(object sender, RoutedEventArgs e)
        {
            foreach (var item in ThresholdsGrid.SelectedItems.Cast<KeyValuePair<string, Entry>>())
            {
                item.Value.Threshold = viewModel.ValueToApply;
            }
        }
    }
}
