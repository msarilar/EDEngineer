using System;
using System.Windows;
using EDEngineer.Localization;

namespace EDEngineer.Views.Popups
{
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class ErrorWindow
    {
        public ErrorWindow(Exception exception, string title = "Unrecoverable Error")
        {
            InitializeComponent();

            ExceptionContent.Text = exception.ToString();
            Title = Languages.Instance.Translate(title);
            CloseButton.Content = Languages.Instance.Translate("Close");
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
