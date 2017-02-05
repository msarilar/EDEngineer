using System;
using System.Windows;

namespace EDEngineer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
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