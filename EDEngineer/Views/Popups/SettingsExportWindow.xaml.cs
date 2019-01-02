using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using EDEngineer.Models.Utils;
using EDEngineer.Properties;
using Newtonsoft.Json;
using MessageBox = System.Windows.Forms.MessageBox;

namespace EDEngineer.Views.Popups
{
    /// <summary>
    /// Interaction logic for SettingsExportWindow.xaml
    /// </summary>
    public partial class SettingsExportWindow
    {
        private readonly Action loadedCallback;

        public SettingsExportWindow(Action loadedCallback)
        {
            this.loadedCallback = loadedCallback;
            InitializeComponent();
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadSettingsButtonClicked(object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var newSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(dialog.FileName));
                        foreach (var property in typeof(Settings).GetProperties().Where(p => p.CanWrite))
                        {
                            try
                            {
                                property.SetValue(Settings.Default, property.GetValue(newSettings));
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        Settings.Default.Save();
                        Close();
                        loadedCallback();
                    }
                    catch
                    {
                        MessageBox.Show(
                            "Settings could not be loaded, please make sure EDEngineer is up to date on both instances (source and target)");
                    }
                }
            }
        }

        private void ExportSettingsButtonClicked(object sender, RoutedEventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new WritablePropertiesOnlyResolver(),
                        Formatting = Formatting.Indented
                    };

                    File.WriteAllText(dialog.FileName, JsonConvert.SerializeObject(Settings.Default, settings));
                }
            }
        }
    }
}
