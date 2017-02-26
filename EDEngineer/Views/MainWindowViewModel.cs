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
using Newtonsoft.Json;

namespace EDEngineer.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public SortedObservableDictionary<string, CommanderViewModel> Commanders { get; }  = new SortedObservableDictionary<string, CommanderViewModel>((a, b) => string.Compare(a.Key, b.Key, StringComparison.InvariantCultureIgnoreCase));

        public Languages Languages { get; }

        public KeyValuePair<string, CommanderViewModel> CurrentCommander
        {
            get { return currentCommander; }
            set
            {
                if (object.Equals(value, currentCommander))
                {
                    return;
                }

                currentCommander = value;
                SettingsManager.SelectedCommander = value.Key;
                OnPropertyChanged();
            }
        }

        private string logDirectory;
        public string LogDirectory
        {
            get { return logDirectory; }
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
            Languages = languages;
            CurrentComparer = SettingsManager.Comparer;
            LoadState();
        }

        public void LoadState(bool forcePickFolder = false)
        {
            LogDirectory = IOUtils.RetrieveLogDirectory(forcePickFolder, LogDirectory);
            LogWatcher?.Dispose();
            LogWatcher = new LogWatcher(LogDirectory);

            var allLogs = LogWatcher.RetrieveAllLogs();
            Commanders.Clear();

            var entryDatas =
                JsonConvert.DeserializeObject<List<EntryData>>(IOUtils.GetEntryDatasJson());

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
                    else if(logs.Item1 != LogWatcher.DEFAULT_COMMANDER_NAME)
                    {
                        var commanderState = new CommanderViewModel(logs.Item1, logs.Item2, Languages, entryDatas);
                        Commanders[logs.Item1] = commanderState;
                    }
                });
            });
        }

        private bool showZeroes = true;
        private bool showOnlyForFavorites;
        private bool showOriginIcons = true;

        private KeyValuePair<string, CommanderViewModel> currentCommander;
        private string currentComparer;

        public bool ShowZeroes
        {
            get { return showZeroes; }
            set
            {
                showZeroes = value;
                OnPropertyChanged();
            }
        }

        public int CargoTabIndex
        {
            get { return SettingsManager.CargoTabIndex; }
            set {
                SettingsManager.CargoTabIndex = value;
                OnPropertyChanged();
            }
        }

        public bool ShowOnlyForFavorites
        {
            get { return showOnlyForFavorites; }
            set
            {
                showOnlyForFavorites = value;
                OnPropertyChanged();
            }
        }

        public bool ShowOriginIcons
        {
            get { return showOriginIcons; }
            set
            {
                showOriginIcons = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<string> Comparers
        {
            get
            {
                yield return "Name";
                yield return "Count";
            }
        }

        public string CurrentComparer
        {
            get { return currentComparer; }
            set
            {
                currentComparer = value;
                SettingsManager.Comparer = value;
                OnPropertyChanged();
                foreach (var state in Commanders.Select(c => c.Value.State))
                {
                    state.ChangeComparer(currentComparer);
                }
            }
        }

        public LogWatcher LogWatcher { get; private set; }


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

            foreach (var ingredientFilter in CurrentCommander.Value.Filters.GroupedIngredientFilters.SelectMany(g => g))
            {
                ingredientFilter.Checked = false;
            }
        }

        public void ToggleHighlight(KeyValuePair<string, Entry> dataContext)
        {
            dataContext.Value.Highlighted = !dataContext.Value.Highlighted;

            if (dataContext.Value.Highlighted)
            {
                Settings.Default.EntriesHighlighted.Add(dataContext.Value.Data.Name);
            }
            else
            {
                Settings.Default.EntriesHighlighted.Remove(dataContext.Value.Data.Name);
            }

            Settings.Default.Save();
        }
    }
}