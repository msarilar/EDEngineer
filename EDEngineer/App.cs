using System;
using System.Windows;
using System.Windows.Threading;

namespace EDEngineer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private bool closed = false;
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
                Current.Shutdown();
            };

            Current.DispatcherUnhandledException += (o, e) =>
            {
                e.Handled = true;
                MessageBox.Show(e.Exception.ToString());
                new Views.Popups.ErrorWindow(e.Exception).ShowDialog();
                Current.MainWindow?.Close();
                Current.Shutdown();
            };

            Dispatcher.UnhandledException += (o, e) =>
            {
                e.Handled = true;
                new Views.Popups.ErrorWindow(e.Exception).ShowDialog();
                Current.MainWindow?.Close();
                Current.Shutdown();
            };
        }
    }
}