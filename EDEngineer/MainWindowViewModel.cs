using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Barda;
using EDEngineer.Models.Barda.Collections;
using EDEngineer.Properties;
using EDEngineer.Utils.System;

namespace EDEngineer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public SortedObservableDictionary<string, CommanderViewModel> Commanders { get; }  = new SortedObservableDictionary<string, CommanderViewModel>((a, b) => string.Compare(a.Key, b.Key, StringComparison.InvariantCultureIgnoreCase));

        public Languages Languages { get; private set; }

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
            LoadState();
        }

        public void LoadState(bool forcePickFolder = false)
        {
            LogDirectory = IOUtils.RetrieveLogDirectory(forcePickFolder, LogDirectory);
            LogWatcher?.Dispose();
            LogWatcher = new LogWatcher(logDirectory);

            var allLogs = LogWatcher.RetrieveAllLogs();
            Commanders.Clear();

            foreach (var commander in allLogs.Keys)
            {
                // some file contains only one line unrelated to anything, could generate Dummy Commander if we don't skip
                if (allLogs[commander].Count <= 1)
                {
                    continue;
                }

                var commanderState = new CommanderViewModel(commander, allLogs[commander], Languages);
                Commanders[commander] = commanderState;
            }

            if (Commanders.Count == 0) // we found absolutely nothing
            {
                Commanders[LogWatcher.DEFAULT_COMMANDER_NAME] = new CommanderViewModel(LogWatcher.DEFAULT_COMMANDER_NAME, new List<string>(), Languages);
            }

            CurrentCommander = Commanders.First();

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
                        var commanderState = new CommanderViewModel(logs.Item1, logs.Item2, Languages);
                        Commanders[logs.Item1] = commanderState;
                    }
                });
            });
        }

        private bool showZeroes = true;
        private bool showOnlyForFavorites;
        private bool showOriginIcons = true;

        private KeyValuePair<string, CommanderViewModel> currentCommander;

        public bool ShowZeroes
        {
            get { return showZeroes; }
            set
            {
                showZeroes = value;
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
    }
}