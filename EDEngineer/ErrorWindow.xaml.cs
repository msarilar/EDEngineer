using System;
using System.Windows;
using EDEngineer.Localization;

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
            Title = Languages.Instance.Translate("Unrecoverable Error");
            CloseButton.Content = Languages.Instance.Translate("Close");
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
