using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using EDEngineer.Models;
using EDEngineer.Properties;
using EDEngineer.Utils.Collections;
using EDEngineer.Utils.System;

namespace EDEngineer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public SortedObservableDictionary<string, CommanderViewModel> Commanders { get; }  = new SortedObservableDictionary<string, CommanderViewModel>((a, b) => string.Compare(a.Key, b.Key, StringComparison.InvariantCultureIgnoreCase));

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

        public MainWindowViewModel()
        {
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

                var commanderState = new CommanderViewModel(commander);
                commanderState.LoadState(allLogs[commander]);
                Commanders[commander] = commanderState;
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
                    else
                    {
                        var commanderState = new CommanderViewModel(logs.Item1);
                        commanderState.LoadState(logs.Item2);
                        Commanders[logs.Item1] = commanderState;
                    }
                });
            });
        }

        private bool showZeroes = true;
        private bool showOnlyForFavorites;
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

        public LogWatcher LogWatcher { get; private set; }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UserChange(Entry entry, int i)
        {
            CurrentCommander.Value.UserChange(entry, i);
        }
    }
}