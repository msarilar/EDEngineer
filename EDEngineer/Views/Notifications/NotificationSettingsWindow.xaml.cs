using System;
using System.Windows;
using EDEngineer.Localization;
using EDEngineer.Utils.System;

namespace EDEngineer.Views.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationSettings.xaml
    /// </summary>
    public partial class NotificationSettingsWindow
    {
        private readonly NotificationSettingsViewModel viewModel;
        public NotificationSettingsWindow(Languages languages)
        {
            viewModel = new NotificationSettingsViewModel(languages);
            DataContext = viewModel;
            InitializeComponent();

            ThresholdToastRadioButton.IsEnabled = 
                BlueprintToastRadioButton.IsEnabled =
                CargoToastRadioButton.IsEnabled =
                Environment.OSVersion.Version >= new Version(6, 2, 9200, 0);
        }

        private void FavoriteBlueprintTestClicked(object sender, RoutedEventArgs e)
        {
            viewModel.TriggerFavoriteReady();
        }

        private void CargoAlmostFullTestClicked(object sender, RoutedEventArgs e)
        {
            viewModel.TriggerCargoAlmostFull();
        }

        private void ThresholdReachedTestClicked(object sender, RoutedEventArgs e)
        {
            viewModel.TriggerThresholdReached();
        }

        private void NotificationSettingsWindowClosed(object sender, EventArgs e)
        {
            viewModel.Dispose();
        }

        public static void InitNotifications()
        {
            if (string.IsNullOrEmpty(Properties.Settings.Default.NotificationKindCargoAlmostFull))
            {
                SettingsManager.NotificationKindBlueprintReady = NotificationKind.None;
                SettingsManager.NotificationKindThresholdReached = NotificationKind.None;
                SettingsManager.NotificationKindCargoAlmostFull = NotificationKind.None;

                new NotificationSettingsWindow(Languages.Instance).ShowDialog();
            }
            else if(Environment.OSVersion.Version < new Version(6, 2, 9200, 0))
            {
                if (SettingsManager.NotificationKindThresholdReached == NotificationKind.Toast)
                {
                    SettingsManager.NotificationKindThresholdReached = NotificationKind.None;
                }

                if (SettingsManager.NotificationKindBlueprintReady == NotificationKind.Toast)
                {
                    SettingsManager.NotificationKindBlueprintReady = NotificationKind.None;
                }

                if (SettingsManager.NotificationKindCargoAlmostFull == NotificationKind.Toast)
                {
                    SettingsManager.NotificationKindCargoAlmostFull = NotificationKind.None;
                }
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
