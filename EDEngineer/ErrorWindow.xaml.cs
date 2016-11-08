using System;
using System.Windows;

namespace EDEngineer
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow
    {
        public ErrorWindow(Exception exception)
        {
            InitializeComponent();

            ExceptionContent.Text = exception.ToString();
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
