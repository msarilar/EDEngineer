using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Notifications;
using EDEngineer.Models;
using EDEngineer.Models.Operations;
using EDEngineer.Utils;
using EDEngineer.Utils.System;
using Newtonsoft.Json;
using NodaTime;
using Application = System.Windows.Application;

namespace EDEngineer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public State State { get; set; }
        public List<Blueprint> Blueprints { get; set; }
        public BlueprintFilters Filters { get; set; }

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

                Properties.Settings.Default.LogDirectory = value;
                Properties.Settings.Default.Save();
                logDirectory = value;
                OnPropertyChanged();
            }
        }

        private readonly JournalEntryConverter journalEntryConverter;
        private readonly BlueprintConverter blueprintConverter;
        private Instant lastUpdate = Instant.MinValue;
        private readonly HashSet<Blueprint> favoritedBlueprints = new HashSet<Blueprint>();

        public MainWindowViewModel()
        {
            var entryDatas = JsonConvert.DeserializeObject<List<EntryData>>(IOUtils.GetEntryDatasJson());
            var converter = new ItemNameConverter(entryDatas);

            State = new State(entryDatas);
            journalEntryConverter = new JournalEntryConverter(converter, State.Cargo);
            blueprintConverter = new BlueprintConverter(State.Cargo);
            LoadBlueprints();
            LoadState();
        }

        public Instant LastUpdate
        {
            get { return lastUpdate; }
            set
            {
                if (value == lastUpdate)
                    return;
                lastUpdate = value;
                OnPropertyChanged();
            }
        }

        private bool showZeroes = true;
        private bool showOnlyForFavorites;

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

        internal void LoadState(bool forcePickFolder = false)
        {
            LogDirectory = IOUtils.RetrieveLogDirectory(forcePickFolder, LogDirectory);

            LogWatcher?.Dispose();

            // Clear state:
            State.Cargo.ToList().ForEach(k => State.IncrementCargo(k.Value.Data.Name, -1 * k.Value.Count));
            LastUpdate = Instant.MinValue;
            UnsubscribeToasts();

            LogWatcher = new LogWatcher(LogDirectory);

            var allLogs = LogWatcher.RetrieveAllLogs();
            ApplyEventsToSate(allLogs);

            LogWatcher.InitiateWatch(ApplyEventsToSate);

            SubscribeToasts();
        }

        private void UnsubscribeToasts()
        {
            if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)) // windows 8 or more recent
            {
                foreach (var blueprint in Blueprints)
                {
                    blueprint.FavoriteAvailable -= BlueprintOnFavoriteAvailable;
                }
            }
        }

        private void SubscribeToasts()
        {
            if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)) // windows 8 or more recent
            {
                foreach (var blueprint in Blueprints)
                {
                    blueprint.FavoriteAvailable += BlueprintOnFavoriteAvailable;
                }
            }
        }

        private void BlueprintOnFavoriteAvailable(object sender, EventArgs e)
        {
            var blueprint = (Blueprint) sender;
            try
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                var stringElements = toastXml.GetElementsByTagName("text");

                stringElements[0].AppendChild(toastXml.CreateTextNode("Blueprint Ready"));
                stringElements[1].AppendChild(toastXml.CreateTextNode($"{blueprint.Name} (G{blueprint.Grade})"));
                stringElements[2].AppendChild(toastXml.CreateTextNode($"{string.Join(", ", blueprint.Engineers)}"));

                var imagePath = "file:///" + Path.GetFullPath("Resources/Images/elite-dangerous-clean.png");

                var imageElements = toastXml.GetElementsByTagName("image");
                imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

                var toast = new ToastNotification(toastXml);

                ToastNotificationManager.CreateToastNotifier("EDEngineer").Show(toast);
            }
            catch (Exception)
            {
                // silently fail for platforms not supporting toasts
            }
        }

        private void ApplyEventsToSate(IEnumerable<string> allLogs)
        {
            var entries = allLogs.Select(l => JsonConvert.DeserializeObject<JournalEntry>(l, new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { journalEntryConverter },
                Error = (o, e) => e.ErrorContext.Handled = true
            }))
                .Where(e => e?.Relevant == true)
                .OrderBy(e => e.TimeStamp)
                .ToList();

            foreach (var entry in entries.Where(entry => entry.TimeStamp >= LastUpdate).ToList())
            {
                Application.Current.Dispatcher.Invoke(() => entry.JournalOperation.Mutate(State));
                LastUpdate = entry.TimeStamp;
            }
        }

        private void LoadBlueprints()
        {
            var blueprintsJson = IOUtils.GetBlueprintsJson();

            Blueprints = new List<Blueprint>(JsonConvert.DeserializeObject<List<Blueprint>>(blueprintsJson, blueprintConverter));
            if (Properties.Settings.Default.Favorites == null)
            {
                Properties.Settings.Default.Favorites = new StringCollection();
            }

            if (Properties.Settings.Default.Ignored == null)
            {
                Properties.Settings.Default.Ignored = new StringCollection();
            }

            foreach (var blueprint in Blueprints)
            {
                if (Properties.Settings.Default.Favorites.Contains(blueprint.ToString()))
                {
                    blueprint.Favorite = true;
                    favoritedBlueprints.Add(blueprint);
                }

                if (Properties.Settings.Default.Ignored.Contains(blueprint.ToString()))
                {
                    blueprint.Ignored = true;
                }

                blueprint.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == "Favorite")
                    {
                        if (blueprint.Favorite)
                        {
                            Properties.Settings.Default.Favorites.Add(blueprint.ToString());
                            favoritedBlueprints.Add(blueprint);
                        }
                        else
                        {
                            Properties.Settings.Default.Favorites.Remove(blueprint.ToString());
                            favoritedBlueprints.Remove(blueprint);
                        }

                        Properties.Settings.Default.Save();
                    }
                    else if (e.PropertyName == "Ignored")
                    {
                        if (blueprint.Ignored)
                        {
                            Properties.Settings.Default.Ignored.Add(blueprint.ToString());
                        }
                        else
                        {
                            Properties.Settings.Default.Ignored.Remove(blueprint.ToString());
                        }

                        Properties.Settings.Default.Save();
                    }
                };
            }

            Filters = new BlueprintFilters(Blueprints);
        }

        public void UserChange(Entry entry, int change)
        {
            if (change == 0)
            {
                return;
            }

            var logEntry = new JournalEntry
            {
                JournalOperation = new ManualChangeOperation
                {
                    Count = change,
                    JournalEvent = JournalEvent.ManualUserChange,
                    Name = entry.Data.Name
                },
                TimeStamp = SystemClock.Instance.Now
            };

            var json = JsonConvert.SerializeObject(logEntry, journalEntryConverter);

            logEntry.OriginalJson = json;

            logEntry.JournalOperation.Mutate(State);

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EDEngineer", "manualChanges.json");
            File.AppendAllText(path, json + Environment.NewLine);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICollectionView FilterView(Kind kind, ICollectionView view)
        {
            view.Filter = o =>
            {
                var entry = ((KeyValuePair<string, Entry>)o).Value;

                return entry.Data.Kind == kind && 
                        (ShowZeroes || entry.Count > 0) && 
                        (!ShowOnlyForFavorites || favoritedBlueprints.Any(b => b.Ingredients.Any(i => i.Entry == entry)));
            };

            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(ShowZeroes) || e.PropertyName == nameof(ShowOnlyForFavorites))
                {
                    view.Refresh();
                }
            };

            Blueprints.ForEach(b => b.PropertyChanged += (o, e) =>
            {
                if (ShowOnlyForFavorites && e.PropertyName == "Favorite")
                {
                    Application.Current.Dispatcher.Invoke(view.Refresh);
                }
            });

            return view;
        }
    }
}