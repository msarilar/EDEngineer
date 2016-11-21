using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EDEngineer.Models;
using Newtonsoft.Json.Linq;

namespace EDEngineer.Utils.System
{
    public class LogWatcher : IDisposable
    {
        public const string DEFAULT_COMMANDER_NAME = "Default";
        private const string LOG_FILE_PATTERN = "Journal.*.log";

        private readonly string logDirectory;
        private FileSystemWatcher watcher;
        private Timer periodicRefresher;

        private readonly Dictionary<string, long> positions = new Dictionary<string, long>();
        private readonly Dictionary<string, FileInfo> fileInfos = new Dictionary<string, FileInfo>();
        private readonly Dictionary<string, string> fileCommanders = new Dictionary<string, string>(); 

        public LogWatcher(string logDirectory)
        {
            this.logDirectory = logDirectory;
        }

        public void InitiateWatch(Action<Tuple<string, List<string>>> callback)
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
            
            watcher.Changed += (o, e) => { callback(ReadLinesWithoutLock(e.FullPath)); };
            watcher.Created += (o, e) => { callback(ReadLinesWithoutLock(e.FullPath)); };

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

        public Tuple<string, List<string>> ReadLinesWithoutLock(string file)
        {
            if (!File.Exists(file))
            {
                return Tuple.Create(DEFAULT_COMMANDER_NAME, new List<string>());
            }

            var gameLogLines = new List<string>();
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var watchForLoadGameEvent = false;

                if (positions.ContainsKey(file))
                {
                    stream.Seek(positions[file], SeekOrigin.Begin);
                }
                
                if(!fileCommanders.ContainsKey(file))
                {
                    watchForLoadGameEvent = true;
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    gameLogLines.Add(line);

                    if (watchForLoadGameEvent && line.Contains($@"""event"":""{JournalEvent.LoadGame}"""))
                    {
                        try
                        {
                            var data = JObject.Parse(line);
                            fileCommanders[file] = (string) data["Commander"];
                        }
                        catch
                        {
                            fileCommanders[file] = DEFAULT_COMMANDER_NAME;
                        }

                        watchForLoadGameEvent = false;
                    }
                }

                positions[file] = stream.Length;
            }

            string commanderName;
            if (!fileCommanders.TryGetValue(file, out commanderName))
            {
                commanderName = DEFAULT_COMMANDER_NAME;
            }

            return Tuple.Create(commanderName, gameLogLines);
        }

        public string ManualChangesDirectory { get; private set; }
        public Dictionary<string, List<string>> RetrieveAllLogs()
        {
            var gameLogLines = new Dictionary<string, List<string>>();
            foreach (
                var file in
                    Directory.GetFiles(logDirectory)
                        .Where(
                            f =>
                                f != null && Path.GetFileName(f).StartsWith("Journal.") &&
                                Path.GetFileName(f).EndsWith(".log")))
            {
                var fileContents = ReadLinesWithoutLock(file);
                if (fileContents.Item1 == DEFAULT_COMMANDER_NAME)
                {
                    continue;
                }

                if (gameLogLines.ContainsKey(fileContents.Item1))
                {
                    gameLogLines[fileContents.Item1].AddRange(fileContents.Item2);
                }
                else
                {
                    gameLogLines[fileContents.Item1] = fileContents.Item2;
                }
            }

            ManualChangesDirectory = IOUtils.GetManualChangesDirectory();

            foreach (var file in Directory.GetFiles(ManualChangesDirectory).Where(f => f != null && Path.GetFileName(f).StartsWith("manualChanges.") && f.EndsWith(".json")).ToList())
            {
                var fileName = Path.GetFileName(file);
                var manualChangesCommander = fileName.Substring("manualChanges.".Length);

                string commanderName;

                if (manualChangesCommander == "json") // manualChanges.json
                {
                    commanderName = gameLogLines.Keys.FirstOrDefault(k => k != DEFAULT_COMMANDER_NAME) ?? DEFAULT_COMMANDER_NAME;
                }
                else // manualChanges.Hg.Json
                {
                    commanderName = manualChangesCommander
                        .Substring(0, manualChangesCommander.Length - ".json".Length)
                        .Desanitize();

                }

                var content = File.ReadAllLines(file).ToList();


                if (!gameLogLines.ContainsKey(commanderName))
                {
                    gameLogLines[commanderName] = content;
                }
                else
                {
                    gameLogLines[commanderName].AddRange(content);
                }

                // migrate old manualChanges.json files to new one:
                if (manualChangesCommander == "json")
                {
                    File.Move(file, Path.Combine(ManualChangesDirectory, $"manualChanges.{commanderName}.json"));
                    File.Delete(file);
                }
            }

            return gameLogLines;
        }

        public void Dispose()
        {
            periodicRefresher?.Dispose();
            watcher?.Dispose();
        }
    }
}