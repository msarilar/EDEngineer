using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static Timer periodicTouch;

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
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = false
                };

                // TODO: use something like unix's tail rather than reading the entire file every time it's modified...
                watcher.Changed += (o, e) => { action(ReadLinesWithoutLock(e.FullPath)); };
                watcher.Created += (o, e) => { action(ReadLinesWithoutLock(e.FullPath)); };
            }

            periodicTouch = new Timer()
            {
                Interval = 2000
            };

            periodicTouch.Tick +=
                (o, e) =>
                {
                    var file = Directory.GetFiles(logDirectory).Where(f => Path.GetFileName(f).StartsWith("Journal.") &&
                                                                           Path.GetFileName(f).EndsWith(".log"))
                        .OrderByDescending(f => f)
                        .First();

                    /*
                     * EDEngineer updates *as soon* as the log files are updated by passively listening to file changes via the Windows API. It's almost immediate!
                     * Elite Dangerous updates the log files periodically
                     * EDEngineer *explicitly* reads the log files upon startup
                     * Switching modes make EDEngineer reboot, so it makes the app read the files again
                     * Elite Dangerous **seems to force a log update when it detects somebody explicitly read its logs!** I don't know why they do it like that, probably performance reasons
                     */

                    //File.SetLastAccessTimeUtc doesn't seem to work
                    ReadLinesWithoutLock(file);
                };

            periodicTouch.Start();
        }

        public static void StopWatch()
        {
            periodicTouch?.Dispose();
            watcher?.Dispose();
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

            var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EDEngineer");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var path = Path.Combine(directory, MANUAL_CHANGES_LOG_FILE_NAME);

            var manualChangeLines = File.Exists(path)
                ? File.ReadAllLines(path)
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

        public static string GetEntryDatasJson()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EDEngineer.Resources.Data.entryData.json"))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}