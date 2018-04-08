using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Utils;
using EDEngineer.Models.Utils.Collections;
using EDEngineer.Properties;
using EDEngineer.Utils.System;
using EDEngineer.Views.Popups.Graphics;
using Newtonsoft.Json;

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

                Settings.Default.LogDirectory = value;
                Settings.Default.Save();
                logDirectory = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel(Languages languages)
        {
            entryDatas =
                JsonConvert.DeserializeObject<List<EntryData>>(IOUtils.GetEntryDatasJson());
            Languages = languages;
            GraphicSettings = new GraphicSettings();
            IngredientsGrouped = SettingsManager.IngredientsGrouped;

            if (!Comparers.Contains(SettingsManager.Comparer))
            {
                SettingsManager.Comparer = Comparers.First();
            }

            LoadState();

            CurrentComparer = SettingsManager.Comparer;
        }

        public void LoadState(bool forcePickFolder = false)
        {
            LogDirectory = IOUtils.RetrieveLogDirectory(forcePickFolder, LogDirectory);
            LogWatcher?.Dispose();
            LogWatcher = new LogWatcher(LogDirectory);

            var allLogs = LogWatcher.RetrieveAllLogs();
            Commanders.Clear();

            foreach (var commander in allLogs.Keys)
            {
                // some file contains only one line unrelated to anything, could generate Dummy Commander if we don't skip
                if (allLogs[commander].Count <= 1)
                {
                    continue;
                }

                var commanderState = new CommanderViewModel(commander, allLogs[commander], Languages, entryDatas);
                Commanders[commander] = commanderState;
            }

            if (Commanders.Count == 0) // we found absolutely nothing
            {
                Commanders[LogWatcher.DEFAULT_COMMANDER_NAME] = new CommanderViewModel(LogWatcher.DEFAULT_COMMANDER_NAME, new List<string>(), Languages, entryDatas);
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

        private bool showZeroes = true;
        private bool showOnlyForFavorites;
        private bool showOriginIcons = true;
        private bool ingredientsGrouped;
        private Subkind? materialSubkindFilter;

        private KeyValuePair<string, CommanderViewModel> currentCommander;
        private string currentComparer;
        private readonly List<EntryData> entryDatas;

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
                Application.Current.Dispatcher.Invoke(() =>
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
                        var commanderState = new CommanderViewModel(logs.Item1, logs.Item2, Languages, entryDatas);
                        Commanders[logs.Item1] = commanderState;
                    }
                });
            });
        }
    }
}