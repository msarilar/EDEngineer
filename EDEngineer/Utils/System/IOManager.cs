using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace EDEngineer.Utils.System
{
    public static class IOManager
    {
        private const string MANUAL_CHANGES_LOG_FILE_NAME = "manualChanges.json";
        private const string LOG_FILE_PATTERN = "Journal.*.log";
        private static FileSystemWatcher watcher;

        public static void InitiateWatch(string logDirectory, Action<IEnumerable<string>> action)
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            watcher = null;
            if (logDirectory != null && Directory.Exists(logDirectory))
            {
                watcher = new FileSystemWatcher
                {
                    Path = logDirectory,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = LOG_FILE_PATTERN,
                    EnableRaisingEvents = true
                };

                // TODO: use something like unix's tail rather than reading the entire file every time it's modified...
                watcher.Changed += (o, e) => { action(ReadLinesWithoutLock(e.FullPath)); };
            }
        }

        public static List<string> ReadLinesWithoutLock(string file)
        {
            var gameLogLines = new List<string>();
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    gameLogLines.Add(line);
                }
            }

            return gameLogLines;
        }

        public static IEnumerable<string> RetrieveAllLogs(string logDirectory)
        {
            var gameLogLines = new List<string>();
            foreach (
                var file in
                    Directory.GetFiles(logDirectory)
                        .Where(
                            f =>
                                f != null && Path.GetFileName(f).StartsWith("Journal.") &&
                                Path.GetFileName(f).EndsWith(".log")))
            {
                gameLogLines.AddRange(ReadLinesWithoutLock(file));
            }

            var manualChangeLines = File.Exists(MANUAL_CHANGES_LOG_FILE_NAME)
                ? File.ReadAllLines(MANUAL_CHANGES_LOG_FILE_NAME)
                : new string[] { };

            return gameLogLines.Concat(manualChangeLines);
        }

        public static string RetrieveLogDirectory(bool forcePickFolder, string currentLogDirectory)
        {
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
                    Description = forcePickFolder ? "Select a new log directory" : "Couln't find the log folder for elite, you'll have to specify it"
                };

                if (forcePickFolder && !string.IsNullOrEmpty(currentLogDirectory))
                {
                    dialog.SelectedPath = currentLogDirectory;
                }

                var pickFolderResult = dialog.ShowDialog();

                if (pickFolderResult == DialogResult.OK)
                {
                    if (!Directory.GetFiles(dialog.SelectedPath).Any(f => f != null && Path.GetFileName(f).StartsWith("Journal.") &&
                                                                          Path.GetFileName(f).EndsWith(".log")))
                    {
                        var result =
                            MessageBox.Show(
                                "Selected directory doesn't seem to contain any log file ; are you sure?",
                                "Warning", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning);

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
                    MessageBox.Show("You did not select a log directory, EDEngineer won't be able to track changes. You can still use the app manually though.",
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    logDirectory = @"\No folder in use ; click to change";
                }
            }

            return logDirectory;
        }

        public static bool NewVersionAvailable
        {
            get
            {
                using (var client = new HttpClient())
                {
                    var getResponse =
                        client.GetAsync("https://cdn.rawgit.com/msarilar/EDEngineer/master/EDEngineer/Version").Result;

                    if (getResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var version = getResponse.Content.ReadAsStringAsync().Result;
                        return Assembly.GetExecutingAssembly().GetName().Version < Version.Parse(version);
                    }
                }

                return false;
            }
        }

        public static string GetBlueprintsJson()
        {
            string json;
            try
            {
                using (var client = new HttpClient())
                {
                    var getResponse = client.GetAsync(
                        "https://cdn.rawgit.com/msarilar/EDEngineer/master/EDEngineer/Resources/Data/blueprints.json")
                        .Result;

                    if (getResponse.StatusCode == HttpStatusCode.OK)
                    {
                        return getResponse.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            catch
            {
                // ignored
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EDEngineer.Resources.Data.blueprints.json"))
            using (var reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            return json;
        }
    }
}