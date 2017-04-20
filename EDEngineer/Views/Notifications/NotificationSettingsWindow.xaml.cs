using System;
using System.Windows;
using EDEngineer.Localization;

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

            ToastRadioButton.IsEnabled = Environment.OSVersion.Version >= new Version(6, 2, 9200, 0);
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
    }
}
