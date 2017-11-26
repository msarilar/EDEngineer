using System;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace EDEngineer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            CheckForUpdates();

            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                if (Current.MainWindow != null)
                {
                    Current.MainWindow.Visibility = Visibility.Hidden;
                }
                
                new Views.Popups.ErrorWindow((Exception)e.ExceptionObject).ShowDialog();
                Current.MainWindow?.Close();
            };

            Current.DispatcherUnhandledException += (o, e) =>
            {
                new Views.Popups.ErrorWindow(e.Exception).ShowDialog();
                Current.MainWindow?.Close();
            };
        }

        public static async Task CheckForUpdates()
        {
            using (var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/msarilar/EDEngineer"))
            {
                await mgr.UpdateApp();
            }
        }
    }
}