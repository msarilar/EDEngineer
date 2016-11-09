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
                Current.MainWindow.Visibility = Visibility.Hidden;
                new ErrorWindow((Exception)e.ExceptionObject).ShowDialog();
                Current.MainWindow.Close();
            };

            Current.DispatcherUnhandledException += (o, e) =>
            {
                Current.MainWindow.Visibility = Visibility.Hidden;
                new ErrorWindow(e.Exception).ShowDialog();
                Current.MainWindow.Close();
            };
        }
    }
}