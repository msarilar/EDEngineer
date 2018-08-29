using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.State;
using EDEngineer.Models.Utils;
using EDEngineer.Models.Utils.Collections;
using EDEngineer.Properties;
using EDEngineer.Utils.System;
using EDEngineer.Views.Popups.Graphics;
using Newtonsoft.Json;
using Duration = NodaTime.Duration;

namespace EDEngineer.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public SortedObservableDictionary<string, CommanderViewModel> Commanders { get; }  = new SortedObservableDictionary<string, CommanderViewModel>((a, b) => string.Compare(a.Key, b.Key, StringComparison.InvariantCultureIgnoreCase));

        public Languages Languages { get; }

        public GraphicSettings GraphicSettings { get; }

        public KeyValuePair<string, CommanderViewModel> CurrentCommander
        {
            get => currentCommander;
            set
            {
                if (Equals(value, currentCommander))
                {
                    return;
                }

                currentCommander = value;
                SettingsManager.SelectedCommander = value.Key;
                OnPropertyChanged();

                currentCommander.Value.RefreshShoppingList();
            }
        }

        private string logDirectory;
        public string LogDirectory
        {
            get => logDirectory;
            set
            {
                if (value == logDirectory)
                {
                    return;
                }

                File.Delete(Path.Combine(LogWatcher.ManualChangesDirectory, $"aggregation.json"));

                Settings.Default.LogDirectory = value;
                Settings.Default.Save();
                logDirectory = value;
                OnPropertyChanged();
            }
        }

        public bool ApiOn
        {
            get => apiOn;
            set { apiOn = value; OnPropertyChanged(); }
        }

        public MainWindowViewModel(Languages languages, string directory, bool fresh)
        {
            logDirectory = directory;
            entryDatas =
                JsonConvert.DeserializeObject<List<EntryData>>(Helpers.GetEntryDatasJson());
            Languages = languages;
            GraphicSettings = new GraphicSettings();
            IngredientsGrouped = SettingsManager.IngredientsGrouped;

            if (!Comparers.Contains(SettingsManager.Comparer))
            {
                SettingsManager.Comparer = Comparers.First();
            }

            LoadState(fresh);

            CurrentComparer = SettingsManager.Comparer;
        }

        private void LoadState(bool fresh)
        {
            LogWatcher?.Dispose();
            LogWatcher = new LogWatcher(LogDirectory);

            Commanders.Clear();

            var aggregation = fresh ? GetAggregation() : null;

            if (aggregation == null || !aggregation.Aggregations.Any())
            {
                var allLogs = LogWatcher.RetrieveAllLogs();

                foreach (var commander in allLogs.Keys)
                {
                    // some file contains only one line unrelated to anything, could generate Dummy Commander if we don't skip
                    if (allLogs[commander].Count <= 1)
                    {
                        continue;
                    }

                    var commanderState = new CommanderViewModel(commander, c => c.LoadLogs(allLogs[commander]), Languages, entryDatas);
                    Commanders[commander] = commanderState;
                }
            }
            else
            {
                foreach (var key in aggregation.Aggregations.Keys)
                {
                    var commanderState = new CommanderViewModel(key,
                        c => c.LoadAggregation(aggregation.Aggregations[key]), Languages, entryDatas)
                    {
                        LastUpdate = aggregation.Aggregations[key].LastTimestamp
                    };
                    Commanders[key] = commanderState;
                }

                var latestInstant = aggregation?.Aggregations.Values.Max(c => c.LastTimestamp);
                var content = LogWatcher.GetFilesContentFrom(latestInstant);
                foreach (var key in content.Keys)
                {
                    ApplyEvents(Tuple.Create(key, content[key]));
                }
            }

            if (Commanders.Count == 0) // we found absolutely nothing
            {
                Commanders[LogWatcher.DEFAULT_COMMANDER_NAME] = new CommanderViewModel(LogWatcher.DEFAULT_COMMANDER_NAME, c => {}, Languages, entryDatas);
            }

            if (Commanders.Any(k => k.Key == SettingsManager.SelectedCommander))
            {
                CurrentCommander = Commanders.First(k => k.Key == SettingsManager.SelectedCommander);
            }
            else
            {
                CurrentCommander = Commanders.First();
            }

            CurrentCommander.Value.RefreshShoppingList();
        }

        private static CommanderAggregation GetAggregation()
        {
            CommanderAggregation aggregation;
            var path = Path.Combine(LogWatcher.ManualChangesDirectory, $"aggregation.json");
            try
            {
                aggregation = File.Exists(path)
                    ? JsonConvert.DeserializeObject<CommanderAggregation>(File.ReadAllText(path))
                    : null;

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                aggregation?.Aggregations.Select(a => a.Value.LastTimestamp.ToDateTimeUtc()).ToList();
            }
            catch
            {
                aggregation = null;
            }

            return aggregation;
        }

        private bool showZeroes = true;
        private bool showOnlyForFavorites;
        private bool showOriginIcons = true;
        private bool ingredientsGrouped;
        private Subkind? materialSubkindFilter;

        private KeyValuePair<string, CommanderViewModel> currentCommander;
        private string currentComparer;
        private readonly List<EntryData> entryDatas;
        private bool apiOn;

        public bool ShowZeroes
        {
            get => showZeroes;
            set
            {
                showZeroes = value;
                OnPropertyChanged();
            }
        }

        public Subkind? MaterialSubkindFilter
        {
            get => materialSubkindFilter;
            set
            {
                materialSubkindFilter = value;
                OnPropertyChanged();
            }
        }

        public int CargoTabIndex
        {
            get => SettingsManager.CargoTabIndex;
            set {
                SettingsManager.CargoTabIndex = value;
                OnPropertyChanged();
            }
        }

        public bool ShowOnlyForFavorites
        {
            get => showOnlyForFavorites;
            set
            {
                showOnlyForFavorites = value;
                OnPropertyChanged();
            }
        }

        public bool ShowOriginIcons
        {
            get => showOriginIcons;
            set
            {
                showOriginIcons = value;
                OnPropertyChanged();
            }
        }

        public bool IngredientsGrouped
        {
            get => ingredientsGrouped;
            set
            {
                ingredientsGrouped = value;
                SettingsManager.IngredientsGrouped = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> Comparers
        {
            get
            {
                yield return StateCargo.NAME_COMPARER;
                yield return StateCargo.COUNT_COMPARER;
                yield return StateCargo.RARITY_COMPARER;
            }
        }

        public string CurrentComparer
        {
            get => currentComparer;
            set
            {
                currentComparer = value;
                SettingsManager.Comparer = value;
                OnPropertyChanged();
                foreach (var state in Commanders.Select(c => c.Value.State.Cargo))
                {
                    if (IngredientsGrouped)
                    {
                        state.ChangeComparer(currentComparer, (a, b) =>
                            a.Data.Group == null && b.Data.Group == null ? 0 :
                            a.Data.Group == null ? 1 :
                            b.Data.Group == null ? -1 :
                            string.Compare(Languages.Translate(a.Data.Group.Description()),
                                           Languages.Translate(b.Data.Group.Description()), StringComparison.InvariantCultureIgnoreCase));
                    }
                    else
                    {
                        state.ChangeComparer(currentComparer);
                    }
                }
            }
        }

        private LogWatcher LogWatcher { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UserChange(Entry entry, int i)
        {
            if (i == 0)
            {
                return;
            }

            var userChange = CurrentCommander.Value.UserChange(entry, i);

            var path = Path.Combine(LogWatcher.ManualChangesDirectory, $"manualChanges.{CurrentCommander.Key.Sanitize()}.json");
            var file = new FileInfo(path);
            if (file.Exists)
            {
                file.IsReadOnly = false;
            }

            File.AppendAllText(path, userChange.OriginalJson + Environment.NewLine);
        }

        public void ChangeAllFilters(bool newValue)
        {
            foreach (var filter in CurrentCommander.Value.Filters.AllFilters)
            {
                filter.Checked = newValue;
            }
        }

        public void ToggleHighlight(Entry entry)
        {
            CurrentCommander.Value.ToggleHighlight(entry);
        }

        public void Dispose()
        {
            foreach (var commander in Commanders)
            {
                commander.Value.Dispose();
            }
            LogWatcher?.Dispose();
        }

        public void InitiateWatch()
        {
            LogWatcher.InitiateWatch(logs =>
            {
                Application.Current.Dispatcher.Invoke(() => { ApplyEvents(logs); });
            });
        }

        public void ApplyEvents(Tuple<string, List<string>> logs)
        {
            if (logs.Item2.Count == 0)
            {
                return;
            }

            if (Commanders.ContainsKey(logs.Item1))
            {
                Commanders[logs.Item1].ApplyEventsToSate(logs.Item2);
            }
            else if (logs.Item1 != LogWatcher.DEFAULT_COMMANDER_NAME)
            {
                var commanderState = new CommanderViewModel(logs.Item1, c => c.LoadLogs(logs.Item2), Languages, entryDatas);
                Commanders[logs.Item1] = commanderState;
            }
        }

        public void SaveAggregation()
        {
            var newAggregation = new CommanderAggregation
            {
                Aggregations = Commanders.ToDictionary(c => c.Key, c => c.Value.State.Aggregate(c.Value.LastUpdate.Plus(Duration.FromMilliseconds(1))))
            };

            var path = Path.Combine(LogWatcher.ManualChangesDirectory, $"aggregation.json");
            var file = new FileInfo(path);
            if (file.Exists)
            {
                file.IsReadOnly = false;
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(newAggregation));
        }
    }
}