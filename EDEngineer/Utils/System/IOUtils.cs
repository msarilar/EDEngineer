using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Input;
using EDEngineer.Localization;
using Application = System.Windows.Application;

namespace EDEngineer.Utils.System
{
    public static class IOUtils
    {
        private static readonly Dictionary<char, string> specialCharactersMapping = new Dictionary<char, string>()
        {
            ['<'] = "&#60;",
            ['>'] = "&#62;",
            [':'] = "&#58;",
            ['"'] = "&ldquo;",
            ['/'] = "&#47;",
            ['\\'] = "&#92;",
            ['|'] = "&#124;",
            ['?'] = "&#63;",
            ['*'] = "&#42;",
        };

        private static readonly Dictionary<string, string> specialCharactersMappingReversed =
            specialCharactersMapping.ToDictionary(k => k.Value, k => k.Key.ToString());

        private static string SanitizeChar(char input)
        {
            string output;
            if (!specialCharactersMapping.TryGetValue(input, out output))
            {
                return input.ToString();
            }

            return output;
        }

        public static string Sanitize(this string input)
        {
            return input.Select(name => name)
                        .Aggregate(string.Empty, (acc, c) => acc + IOUtils.SanitizeChar(c));
        }

        public static string Desanitize(this string input)
        {
            specialCharactersMappingReversed.Keys.ToList().ForEach(k =>
                                                                   {
                                                                       input = input.Replace(k,
                                                                           specialCharactersMappingReversed[k]);
                                                                   });
            return input;
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
                var dialog = new FolderBrowserDialog
                {
                    Description = forcePickFolder ? 
                                translator.Translate("Select a new log directory") :
                                translator.Translate("Couldn't find the log folder for elite, you'll have to specify it")
                };

                if (forcePickFolder && !string.IsNullOrEmpty(currentLogDirectory))
                {
                    dialog.SelectedPath = currentLogDirectory;
                }

                var pickFolderResult = dialog.ShowDialog();

                if (pickFolderResult == DialogResult.OK)
                {
                    if (!Directory.GetFiles(dialog.SelectedPath).Any(f => f != null &&
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

                    logDirectory = dialog.SelectedPath;
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

        public static string GetBlueprintsJson()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EDEngineer.Resources.Data.blueprints.json"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetReleaseNotesJson()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EDEngineer.Resources.Data.releaseNotes.json"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetLocalizationJson()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EDEngineer.Resources.Data.localization.json"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetEntryDatasJson()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EDEngineer.Resources.Data.entryData.json"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetManualChangesDirectory()
        {
            string directory;

            var localDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EDEngineer");
            var roamingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EDEngineer");

            if (Directory.Exists(roamingDirectory))
            {
                directory = roamingDirectory;
            }
            else if (Directory.Exists(localDirectory) && Directory.GetFiles(localDirectory).Any(f => f != null && Path.GetFileName(f).StartsWith("manualChanges.") && f.EndsWith(".json")))
            {
                directory = localDirectory;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(roamingDirectory);
                    directory = roamingDirectory;
                }
                catch
                {
                    Directory.CreateDirectory(localDirectory);
                    directory = localDirectory;
                }
            }

            return directory;
        }
    }
}