using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
                        foreach (var property in typeof(Settings)
                                                 .GetProperties(
                                                     BindingFlags.DeclaredOnly |
                                                     BindingFlags.Instance |
                                                     BindingFlags.Public).Where(p => p.CanWrite && p.Name != "Version" && p.Name != "CurrentVersion"))
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

                    Settings.Default.UpgradeRequired = true;
                    File.WriteAllText(dialog.FileName, JsonConvert.SerializeObject(Settings.Default, settings));
                }
            }
        }

        private void ResetSettingsButtonClicked(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reset();
            Settings.Default.Save();
            Close();
            loadedCallback();
        }
    }
}
