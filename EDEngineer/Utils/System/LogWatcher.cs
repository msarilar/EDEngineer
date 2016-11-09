using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EDEngineer.Utils.System
{
    public class LogWatcher : IDisposable
    {
        private const string MANUAL_CHANGES_LOG_FILE_NAME = "manualChanges.json";
        private const string LOG_FILE_PATTERN = "Journal.*.log";

        private readonly string logDirectory;
        private FileSystemWatcher watcher;
        private Timer periodicRefresher;

        private readonly Dictionary<string, long> positions = new Dictionary<string, long>();
        private readonly Dictionary<string, FileInfo> fileInfos = new Dictionary<string, FileInfo>();

        public LogWatcher(string logDirectory)
        {
            this.logDirectory = logDirectory;
        }

        public void InitiateWatch(Action<IEnumerable<string>> action)
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
                watcher = null;
            }

            periodicRefresher?.Dispose();

            if (!Directory.Exists(logDirectory))
            {
                return;
            }

            watcher = new FileSystemWatcher
            {
                Path = logDirectory + Path.DirectorySeparatorChar,
                Filter = LOG_FILE_PATTERN,
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime
            };
            
            watcher.Changed += (o, e) => { action(ReadLinesWithoutLock(e.FullPath)); };
            watcher.Created += (o, e) => { action(ReadLinesWithoutLock(e.FullPath)); };

            InitPeriodicRefresh();
        }

        private void InitPeriodicRefresh()
        {
            periodicRefresher = new Timer()
            {
                Interval = 2000
            };

            periodicRefresher.Tick +=
                (o, e) =>
                {
                    var fileNames = Directory.GetFiles(logDirectory).Where(f => f != null &&
                                                                            Path.GetFileName(f).StartsWith("Journal.") &&
                                                                            Path.GetFileName(f).EndsWith(".log"));

                    // Elite Dangerous streams to file so no notification is given to the file system. We need to refresh the data to trigger the FileSystemWatcher:
                    foreach (var fileName in fileNames)
                    {
                        FileInfo fileInfo;
                        if (!fileInfos.TryGetValue(fileName, out fileInfo))
                        {
                            fileInfo = new FileInfo(fileName);
                            fileInfos[fileName] = fileInfo;
                        }

                        fileInfo.Refresh();
                    }
                };

            periodicRefresher.Start();
        }

        public List<string> ReadLinesWithoutLock(string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            var gameLogLines = new List<string>();
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                if (positions.ContainsKey(file))
                {
                    stream.Seek(positions[file], SeekOrigin.Begin);
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    gameLogLines.Add(line);
                }

                positions[file] = stream.Length;
            }

            return gameLogLines;
        }

        public IEnumerable<string> RetrieveAllLogs()
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

        public void Dispose()
        {
            periodicRefresher?.Dispose();
            watcher?.Dispose();
        }
    }
}