using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using EDEngineer.Models.Utils.Json;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace EDEngineer.Models.Utils
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

            periodicRefresher?.Stop();
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

            watcher.Changed += (o, e) => { callback(ReadLinesWithoutLock(e.FullPath, -1, out var _)); };
            watcher.Created += (o, e) => { callback(ReadLinesWithoutLock(e.FullPath, -1, out var _)); };

            InitPeriodicRefresh();
        }

        private FileInfo mostRecentLogFile;
        private void InitPeriodicRefresh()
        {
            periodicRefresher = new Timer
            {
                Interval = 2000,
                AutoReset = true
            };

            mostRecentLogFile = Directory
                .GetFiles(logDirectory)
                .Where(f => f != null &&
                            Path.GetFileName(f).StartsWith("Journal.") &&
                            Path.GetFileName(f).EndsWith(".log"))
                .Select(f => fileInfos.GetOrAdd(f, k => new FileInfo(k)))
                .OrderByDescending(f => f.CreationTimeUtc)
                .FirstOrDefault();

            periodicRefresher.Elapsed +=
                (o, e) =>
                {
                    if (mostRecentLogFile != null)
                    {
                        mostRecentLogFile.IsReadOnly = false;
                        mostRecentLogFile.Refresh();
                    }
                };

            periodicRefresher.Start();
        }

        public Tuple<string, List<string>> ReadLinesWithoutLock(string file, int maxLines, out bool ended)
        {
            ended = true;
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

                if (!fileCommanders.ContainsKey(file))
                {
                    watchForLoadGameEvent = true;
                }

                string line;
                int lineNumber = 0;
                while (true)
                {
                    if((line = reader.ReadLine()) == null)
                    {
                        break;
                    }

                    if (maxLines != -1 && lineNumber >= maxLines)
                    {
                        ended = false;
                        break;
                    }

                    lineNumber++;

                    gameLogLines.Add(line);

                    if (!watchForLoadGameEvent)
                    {
                        continue;
                    }

                    if (VersionIsBeta(line))
                    {
                        fileCommanders.Remove(file);
                        return Tuple.Create(DEFAULT_COMMANDER_NAME, new List<string>());
                    }

                    if (line.Contains($@"""event"":""{JournalEvent.LoadGame}"""))
                    {
                        try
                        {
                            var data = JObject.Parse(line);
                            fileCommanders[file] = (string)data["Commander"];
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

            if (!fileCommanders.TryGetValue(file, out var commanderName))
            {
                commanderName = DEFAULT_COMMANDER_NAME;
            }

            if (mostRecentLogFile != null && file != mostRecentLogFile.FullName)
            {
                mostRecentLogFile = new FileInfo(file);
            }

            return Tuple.Create(commanderName, gameLogLines);
        }

        private static bool VersionIsBeta(string line)
        {
            var lowered = line.ToLower(CultureInfo.InvariantCulture);
            return lowered.Contains(@"""event"":""fileheader""") && lowered.Contains("beta");
        }

        public static string ManualChangesDirectory { get; } = IO.GetManualChangesDirectory();

        public Dictionary<string, List<string>> RetrieveAllLogs(int page, out bool ended)
        {
            ended = true;
            var gameLogLines = new Dictionary<string, List<string>>();
            if (logDirectory != null && Directory.Exists(logDirectory))
            {
                foreach (
                    var file in
                    Directory.GetFiles(logDirectory)
                             .Where(
                                 f =>
                                     f != null && Path.GetFileName(f).StartsWith("Journal.") &&
                                     Path.GetFileName(f).EndsWith(".log")))
                {
                    var fileContents = ReadLinesWithoutLock(file, page, out ended);
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
            }

            var commandersInGame = gameLogLines.Keys.ToHashSet();

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

                // if there's commanders found in the logs and none of them corresponds to the current commander's manualChange, then skip it
                if (commandersInGame.Any() && commandersInGame.All(x => x != commanderName))
                {
                    continue;
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
                    File.Move(file, Path.Combine(ManualChangesDirectory, $"manualChanges.{commanderName.Sanitize()}.json"));
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

        public Dictionary<string, List<string>> GetFilesContentFrom(Instant? latestInstant)
        {
            if (latestInstant.HasValue && logDirectory != null && Directory.Exists(logDirectory))
            {
                var files = Directory.GetFiles(logDirectory)
                                     .Where(
                                         f =>
                                             f != null && Path.GetFileName(f).StartsWith("Journal.") &&
                                             Path.GetFileName(f).EndsWith(".log")).Select(f => new FileInfo(f))
                                     .OrderByDescending(f => f.LastWriteTimeUtc)
                                     .ToList();

                var moreRecentFiles =
                    files.TakeWhile((f, i) => i == 0 || f.LastWriteTimeUtc > latestInstant.Value.ToDateTimeUtc() ||
                                              files[i - 1].LastWriteTimeUtc > latestInstant.Value.ToDateTimeUtc())
                         .ToList();

                if (moreRecentFiles.Any())
                {
                    return moreRecentFiles.Select(f => ReadLinesWithoutLock(f.FullName, -1, out var _)).Aggregate(
                        new Dictionary<string, List<string>>(),
                        (current, content) =>
                        {
                            if (current.ContainsKey(content.Item1))
                            {
                                current[content.Item1].AddRange(content.Item2);
                            }
                            else
                            {
                                current[content.Item1] = content.Item2;
                            }

                            return current;
                        });
                }

                if (files.Any())
                {
                    var content =
                        ReadLinesWithoutLock(files.OrderByDescending(f => f.LastWriteTimeUtc).First().FullName, -1, out var _);
                    return new Dictionary<string, List<string>>
                    {
                        [content.Item1] = content.Item2
                    };
                }
            }

            return new Dictionary<string, List<string>>();
        }
    }
}