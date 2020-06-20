using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EDEngineer.Localization;
using EDEngineer.Models.Utils;
using EDEngineer.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using Application = System.Windows.Application;

namespace EDEngineer.Utils.System
{
    public static class Helpers
    {
        static Helpers()
        {
            blueprintsJson = ReadResource("blueprints");
            releaseNotesJson = ReadResource("releaseNotes");
            localizationJson = ReadResource("localization");
            entryDatasJson = ReadResource("entryData");
        }

        public static string ReadResource(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"EDEngineer.Resources.Data.{resource}.json"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static readonly string blueprintsJson;
        private static readonly string releaseNotesJson;
        private static readonly string localizationJson;
        private static readonly string entryDatasJson;

        public static string GetBlueprintsJson()
        {
            return blueprintsJson;
        }

        public static string GetReleaseNotesJson()
        {
            return releaseNotesJson;
        }

        public static string GetLocalizationJson()
        {
            return localizationJson;
        }

        public static string GetEntryDatasJson()
        {
            return entryDatasJson;
        }

        public static string RetrieveLogDirectory(bool forcePickFolder, string currentLogDirectory)
        {
            var translator = Languages.Instance;
            string logDirectory = null;

            if (!forcePickFolder)
            {
                logDirectory = Settings.Default.LogDirectory;
                if (string.IsNullOrEmpty(logDirectory))
                {
                    var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
                    if (userProfile != null)
                    {
                        logDirectory = Path.Combine(userProfile, @"saved games\Frontier Developments\Elite Dangerous");
                    }
                }
            }

            if (forcePickFolder || logDirectory == null || !Directory.Exists(logDirectory))
            {
                var dialog = new CommonOpenFileDialog
                {
                    Title = forcePickFolder ?
                                translator.Translate("Select a new log directory") :
                                translator.Translate("Couldn't find the log folder for elite, you'll have to specify it"),
                    AllowNonFileSystemItems = false,
                    Multiselect = false,
                    IsFolderPicker = true,
                    EnsurePathExists = true
                };

                if (forcePickFolder && !string.IsNullOrEmpty(currentLogDirectory))
                {
                    dialog.InitialDirectory = currentLogDirectory;
                }

                var pickFolderResult = dialog.ShowDialog();

                if (pickFolderResult == CommonFileDialogResult.Ok)
                {
                    if (!Directory.GetFiles(dialog.FileName).Any(f => Path.GetFileName(f).StartsWith("Journal.") &&
                                                                      Path.GetFileName(f).EndsWith(".log")))
                    {
                        var result =
                            MessageBox.Show(
                                translator.Translate("Selected directory doesn't seem to contain any log file ; are you sure?"),
                                translator.Translate("Warning"), MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning);

                        if (result == DialogResult.Retry)
                        {
                            RetrieveLogDirectory(forcePickFolder, null);
                        }

                        if (result == DialogResult.Abort)
                        {
                            if (forcePickFolder)
                            {
                                return currentLogDirectory;
                            }

                            Application.Current.Shutdown();
                        }
                    }

                    logDirectory = dialog.FileName;
                }
                else if (forcePickFolder)
                {
                    return currentLogDirectory;
                }
                else
                {
                    MessageBox.Show(translator.Translate("You did not select a log directory, EDEngineer won't be able to track changes. You can still use the app manually though."),
                        translator.Translate("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    logDirectory = @"\" + translator.Translate("No folder in use ; click to change");
                }
            }

            Settings.Default.LogDirectory = logDirectory;
            Settings.Default.Save();
            return logDirectory;
        }

        public static bool TryRetrieveShoppingList(out StringCollection result)
        {
            var translator = Languages.Instance;

            var dialog = new CommonOpenFileDialog
            {
                Title = translator.Translate("Select a shopping list to import"),
                AllowNonFileSystemItems = false,
                Multiselect = false,
                IsFolderPicker = false,
                EnsurePathExists = true,
                DefaultExtension = ".shoppingList",
                DefaultDirectory = IO.GetManualChangesDirectory()
            };

            dialog.Filters.Add(new CommonFileDialogFilter("Shopping List Files (*.shoppingList)", ".shoppingList"));
            dialog.Filters.Add(new CommonFileDialogFilter("Json Files (*.json)", ".json"));

            var pickFileResult = dialog.ShowDialog();

            if (pickFileResult == CommonFileDialogResult.Ok)
            {
                if (File.Exists(dialog.FileName))
                {
                    var contents = File.ReadAllText(dialog.FileName);
                    result = JsonConvert.DeserializeObject<StringCollection>(contents);
                    return true;
                }
            }

            result = null;
            return false;
        }

        public static void SaveShoppingList()
        {
            var contents = JsonConvert.SerializeObject(Settings.Default.ShoppingList);

            var translator = Languages.Instance;

            var dialog = new CommonSaveFileDialog
            {
                Title = translator.Translate("Select a shopping list file to export over, or enter new name for a new file"),
                EnsurePathExists = true,
                DefaultDirectory = IO.GetManualChangesDirectory(),
                DefaultFileName = "engineering.shoppingList",
                DefaultExtension = ".shoppingList",
                OverwritePrompt = true
            };

            dialog.Filters.Add(new CommonFileDialogFilter("Shopping List Files (*.shoppingList)", ".shoppingList"));

            var pickFileResult = dialog.ShowDialog();

            if (pickFileResult == CommonFileDialogResult.Ok)
            {
                File.WriteAllText(dialog.FileName, contents);
            }
        }
    }
}