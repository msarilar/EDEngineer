using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using EDEngineer.Localization;
using EDEngineer.Models.Utils;
using Microsoft.WindowsAPICodePack.Dialogs;
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
                logDirectory = Properties.Settings.Default.LogDirectory;
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
                    if (!Directory.GetFiles(dialog.FileName).Any(f => f != null &&
                                                                          Path.GetFileName(f).StartsWith("Journal.") &&
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

            Properties.Settings.Default.LogDirectory = logDirectory;
            Properties.Settings.Default.Save();
            return logDirectory;
        }

        public static string RetrieveShoppingListDirectory(bool forcePickFolder, string currentShoppingListDirectory)
        {
            var translator = Languages.Instance;
            string shoppingListDirectory = null;

            if (!forcePickFolder)
            {
                shoppingListDirectory = Properties.Settings.Default.ShoppingListDirectory;
                if (string.IsNullOrEmpty(shoppingListDirectory))
                {
                    var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
                    if (userProfile != null)
                    {
                        shoppingListDirectory = Path.Combine(userProfile, IO.GetManualChangesDirectory());
                    }
                }
            }

            if (forcePickFolder || shoppingListDirectory == null || !Directory.Exists(shoppingListDirectory))
            {
                var dialog = new CommonOpenFileDialog
                {
                    Title = forcePickFolder ?
                                translator.Translate("Select a new shopping list directory") :
                                translator.Translate("Couldn't find the shopping list folder, you'll have to specify it"),
                    AllowNonFileSystemItems = false,
                    Multiselect = false,
                    IsFolderPicker = true,
                    EnsurePathExists = true
                };

                if (forcePickFolder && !string.IsNullOrEmpty(currentShoppingListDirectory))
                {
                    dialog.InitialDirectory = currentShoppingListDirectory;
                }

                var pickFolderResult = dialog.ShowDialog();

                if (pickFolderResult == CommonFileDialogResult.Ok)
                {
                    if (!Directory.GetFiles(dialog.FileName).Any(f => f != null &&
                                                                          Path.GetFileName(f).StartsWith("Journal.") &&
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
                                return currentShoppingListDirectory;
                            }

                            Application.Current.Shutdown();
                        }
                    }

                    shoppingListDirectory = dialog.FileName;
                }
                else if (forcePickFolder)
                {
                    return currentShoppingListDirectory;
                }
                else
                {
                    MessageBox.Show(translator.Translate("You did not select a shopping list directory, EDEngineer won't be able to export shopping lists."),
                        translator.Translate("Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    shoppingListDirectory = @"\" + translator.Translate("No folder in use ; click to change");
                }
            }

            Properties.Settings.Default.ShoppingListDirectory = shoppingListDirectory;
            Properties.Settings.Default.Save();
            return shoppingListDirectory;
        }

        public static string RetrieveShoppingList(string currentShoppingListDirectory)
        {
            var translator = Languages.Instance;

            if (Directory.Exists(currentShoppingListDirectory))
            {
                var dialog = new CommonOpenFileDialog
                {
                    Title = translator.Translate("Select a shopping list to import"),
                    AllowNonFileSystemItems = false,
                    Multiselect = false,
                    IsFolderPicker = false,
                    EnsurePathExists = true
                };

                var pickFileResult = dialog.ShowDialog();

                if (pickFileResult == CommonFileDialogResult.Ok)
                {
                    if (File.Exists(dialog.FileName))
                    {
                        var contents = File.ReadAllText(dialog.FileName);
                        return contents;
                    }
                }
            }

            return string.Empty;
        }

        public static void SaveShoppingList(string currentShoppingListDirectory, string contents)
        {
            var translator = Languages.Instance;

            if (Directory.Exists(currentShoppingListDirectory))
            {
                var dialog = new CommonSaveFileDialog
                {
                    Title = translator.Translate("Select a shopping list file to export over, or enter new name for a new file"),
                    EnsurePathExists = true,
                    DefaultDirectory = currentShoppingListDirectory,
                    DefaultFileName = "ShoppingList.json",
                    DefaultExtension = ".json",
                    OverwritePrompt = true
                };

                dialog.Filters.Add(new CommonFileDialogFilter("Shopping List Files (*.json)", ".json"));
                dialog.Filters.Add(new CommonFileDialogFilter("All Files (*.*)", ".*"));

                var pickFileResult = dialog.ShowDialog();

                if (pickFileResult == CommonFileDialogResult.Ok)
                {
                    File.WriteAllText(dialog.FileName, contents);
                }
            }
        }
    }
}