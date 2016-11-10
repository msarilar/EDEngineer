using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Windows.UI.Notifications;
using EDEngineer.Models;
using EDEngineer.Models.Operations;
using EDEngineer.Properties;
using EDEngineer.Utils;
using EDEngineer.Utils.System;
using Newtonsoft.Json;
using NodaTime;

namespace EDEngineer
{
    public class CommanderViewModel : INotifyPropertyChanged
    {
        public string CommanderName { get; }
        public State State { get; set; }
        public List<Blueprint> Blueprints { get; set; }
        public BlueprintFilters Filters { get; set; }

        private readonly JournalEntryConverter journalEntryConverter;
        private readonly BlueprintConverter blueprintConverter;

        private readonly HashSet<Blueprint> favoritedBlueprints = new HashSet<Blueprint>();
        private Instant lastUpdate = Instant.MinValue;
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


        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void LoadState(List<string> events)
        {
            // Clear state:
            State.Cargo.ToList().ForEach(k => State.IncrementCargo(k.Value.Data.Name, -1 * k.Value.Count));
            LastUpdate = Instant.MinValue;
            UnsubscribeToasts();

            ApplyEventsToSate(events);

            SubscribeToasts();
        }

        public CommanderViewModel(string commanderName)
        {
            CommanderName = commanderName;

            var entryDatas = JsonConvert.DeserializeObject<List<EntryData>>(IOUtils.GetEntryDatasJson());
            var converter = new ItemNameConverter(entryDatas);

            State = new State(entryDatas);

            journalEntryConverter = new JournalEntryConverter(converter, State.Cargo);
            blueprintConverter = new BlueprintConverter(State.Cargo);
            LoadBlueprints();
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
            var blueprint = (Blueprint)sender;
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

        public void ApplyEventsToSate(IEnumerable<string> allLogs)
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

        public ICollectionView FilterView(MainWindowViewModel parentViewModel, Kind kind, ICollectionView view)
        {
            view.Filter = o =>
            {
                var entry = ((KeyValuePair<string, Entry>)o).Value;

                return entry.Data.Kind == kind &&
                       (parentViewModel.ShowZeroes || entry.Count > 0) &&
                       (!parentViewModel.ShowOnlyForFavorites || favoritedBlueprints.Any(b => b.Ingredients.Any(i => i.Entry == entry)));
            };

            parentViewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(parentViewModel.ShowZeroes) || e.PropertyName == nameof(parentViewModel.ShowOnlyForFavorites))
                {
                    view.Refresh();
                }
            };

            Blueprints.ForEach(b => b.PropertyChanged += (o, e) =>
            {
                if (parentViewModel.ShowOnlyForFavorites && e.PropertyName == "Favorite")
                {
                    Application.Current.Dispatcher.Invoke(view.Refresh);
                }
            });

            return view;
        }

        private void LoadBlueprints()
        {
            var blueprintsJson = IOUtils.GetBlueprintsJson();

            Blueprints = new List<Blueprint>(JsonConvert.DeserializeObject<List<Blueprint>>(blueprintsJson, blueprintConverter));
            if (Settings.Default.Favorites == null)
            {
                Settings.Default.Favorites = new StringCollection();
            }

            if (Settings.Default.Ignored == null)
            {
                Settings.Default.Ignored = new StringCollection();
            }

            foreach (var blueprint in Blueprints)
            {
                if (Settings.Default.Favorites.Contains($"{CommanderName}:{blueprint}"))
                {
                    blueprint.Favorite = true;
                    favoritedBlueprints.Add(blueprint);
                }

                if (Settings.Default.Ignored.Contains($"{CommanderName}:{blueprint}"))
                {
                    blueprint.Ignored = true;
                }

                blueprint.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == "Favorite")
                    {
                        if (blueprint.Favorite)
                        {
                            Settings.Default.Favorites.Add($"{CommanderName}:{blueprint}");
                            favoritedBlueprints.Add(blueprint);
                        }
                        else
                        {
                            Settings.Default.Favorites.Remove($"{CommanderName}:{blueprint}");
                            favoritedBlueprints.Remove(blueprint);
                        }

                        Settings.Default.Save();
                    }
                    else if (e.PropertyName == "Ignored")
                    {
                        if (blueprint.Ignored)
                        {
                            Settings.Default.Ignored.Add($"{CommanderName}:{blueprint}");
                        }
                        else
                        {
                            Settings.Default.Ignored.Remove($"{CommanderName}:{blueprint}");
                        }

                        Settings.Default.Save();
                    }
                };
            }

            Filters = new BlueprintFilters(Blueprints);
        }

        public override string ToString()
        {
            return $"CMDR {CommanderName}";
        }
    }
}