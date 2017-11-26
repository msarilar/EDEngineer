using System;
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
            using (var mgr = new UpdateManager("https://raw.githubusercontent.com/msarilar/EDEngineer/side-work/EDEngineer/releases/"))
            {
                mgr.UpdateApp().Wait();
            }

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
    }
}