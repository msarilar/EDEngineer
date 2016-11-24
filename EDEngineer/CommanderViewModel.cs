using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using Windows.UI.Notifications;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Barda;
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

        private void LoadState(List<string> events)
        {
            UnsubscribeToasts();

            // Clear state:
            State.Cargo.ToList().ForEach(k => State.IncrementCargo(k.Value.Data.Name, -1 * k.Value.Count));
            LastUpdate = Instant.MinValue;

            ApplyEventsToSate(events);

            SubscribeToasts();
        }

        public CommanderViewModel(string commanderName, List<string> logs, Languages languages)
        {
            CommanderName = commanderName;

            var entryDatas = JsonConvert.DeserializeObject<List<EntryData>>(IOUtils.GetEntryDatasJson());
            var converter = new ItemNameConverter(entryDatas);

            State = new State(entryDatas, languages);

            journalEntryConverter = new JournalEntryConverter(converter, State.Cargo, languages);
            blueprintConverter = new BlueprintConverter(State.Cargo);
            LoadBlueprints();

            languages.PropertyChanged += (o, e) => OnPropertyChanged(nameof(Filters));

            LoadState(logs);

            var datas = State.Cargo.Select(c => c.Value.Data);
            var ingredientUsed = State.Blueprints.SelectMany(blueprint => blueprint.Ingredients);
            var ingredientUsedNames = ingredientUsed.Select(ingredient => ingredient.Entry.Data.Name).Distinct();
            var unusedIngredients = datas.Where(data => !ingredientUsedNames.Contains(data.Name));

            foreach (var data in unusedIngredients)
            {
                data.Unused = true;
            }
        }

        private void UnsubscribeToasts()
        {
            if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)) // windows 8 or more recent
            {
                foreach (var blueprint in State.Blueprints)
                {
                    blueprint.FavoriteAvailable -= BlueprintOnFavoriteAvailable;
                }

                State.PropertyChanged -= StateCargoCountChanged;
            }
        }

        private void SubscribeToasts()
        {
            if (Environment.OSVersion.Version >= new Version(6, 2, 9200, 0)) // windows 8 or more recent
            {
                foreach (var blueprint in State.Blueprints)
                {
                    blueprint.FavoriteAvailable += BlueprintOnFavoriteAvailable;
                }

                State.PropertyChanged += StateCargoCountChanged;
            }
        }

        private void StateCargoCountChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!SettingsManager.CargoAlmostFullWarningEnabled)
            {
                return;
            }

            var ratio = State.MaxMaterials - State.MaterialsCount;
            string headerText, contentText;
            var translator = Languages.Instance;
            if (ratio <= 5 && e.PropertyName == "MaterialsCount")
            {
                headerText = translator.Translate("Materials Almost Full!");
                contentText = string.Format(translator.Translate("You have only {0} slots left for your materials."), ratio);
            }
            else if ((ratio = State.MaxData - State.DataCount) <= 5 && e.PropertyName == "DataCount")
            {
                headerText = translator.Translate("Data Almost Full!");
                contentText = string.Format(translator.Translate("You have only {0} slots left for your data."), ratio);
            }
            else
            {
                return;
            }

            try
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

                var stringElements = toastXml.GetElementsByTagName("text");

                stringElements[0].AppendChild(toastXml.CreateTextNode(headerText));
                stringElements[1].AppendChild(toastXml.CreateTextNode(contentText));

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

        private void BlueprintOnFavoriteAvailable(object sender, EventArgs e)
        {
            if (!SettingsManager.BlueprintReadyToastEnabled)
            {
                return;
            }

            var blueprint = (Blueprint)sender;
            try
            {
                var translator = Languages.Instance;

                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

                var stringElements = toastXml.GetElementsByTagName("text");

                stringElements[0].AppendChild(toastXml.CreateTextNode(translator.Translate("Blueprint Ready")));
                stringElements[1].AppendChild(toastXml.CreateTextNode($"{translator.Translate(blueprint.Name)} (G{blueprint.Grade})"));
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

        public JournalEntry UserChange(Entry entry, int change)
        {
            var logEntry = new JournalEntry
            {
                JournalOperation = new ManualChangeOperation
                {
                    Count = change,
                    JournalEvent = JournalEvent.ManualUserChange,
                    Name = entry.Data.Name
                },
                Timestamp = SystemClock.Instance.Now
            };

            var json = JsonConvert.SerializeObject(logEntry, journalEntryConverter);

            logEntry.OriginalJson = json;

            MutateState(logEntry);

            return logEntry;
        }

        public void ApplyEventsToSate(IEnumerable<string> allLogs)
        {
            var entries = allLogs.Select(l => JsonConvert.DeserializeObject<JournalEntry>(l, new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { journalEntryConverter },
                Error = (o, e) => e.ErrorContext.Handled = true
            }))
                .Where(e => e?.Relevant == true)
                .OrderBy(e => e.Timestamp)
                .ToList();

            foreach (var entry in entries.Where(entry => entry.Timestamp >= LastUpdate).ToList())
            {
                MutateState(entry);
            }
        }

        private void MutateState(JournalEntry entry)
        {
            State.Operations.AddLast(entry);
            entry.JournalOperation.Mutate(State);
            LastUpdate = entry.Timestamp;
        }

        public ICollectionView FilterView(MainWindowViewModel parentViewModel, Kind kind, CollectionViewSource source)
        {
            source.Filter += (o, e) =>
            {
                var entry = ((KeyValuePair<string, Entry>)e.Item).Value;

                e.Accepted = entry.Data.Kind == kind &&
                       (parentViewModel.ShowZeroes || entry.Count > 0) &&
                       (!parentViewModel.ShowOnlyForFavorites || favoritedBlueprints.Any(b => b.Ingredients.Any(i => i.Entry == entry)));
            };

            parentViewModel.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(parentViewModel.ShowZeroes) || e.PropertyName == nameof(parentViewModel.ShowOnlyForFavorites))
                {
                    source.View.Refresh();
                }
            };

            State.Blueprints.ForEach(b => b.PropertyChanged += (o, e) =>
            {
                if (parentViewModel.ShowOnlyForFavorites && e.PropertyName == "Favorite")
                {
                    Application.Current.Dispatcher.Invoke(source.View.Refresh);
                }
            });

            return source.View;
        }

        private void LoadBlueprints()
        {
            var blueprintsJson = IOUtils.GetBlueprintsJson();

            State.Blueprints = new List<Blueprint>(JsonConvert.DeserializeObject<List<Blueprint>>(blueprintsJson, blueprintConverter));
            if (Settings.Default.Favorites == null)
            {
                Settings.Default.Favorites = new StringCollection();
            }

            if (Settings.Default.Ignored == null)
            {
                Settings.Default.Ignored = new StringCollection();
            }

            foreach (var blueprint in State.Blueprints)
            {
                if (Settings.Default.Favorites.Contains($"{CommanderName}:{blueprint}"))
                {
                    blueprint.Favorite = true;
                    favoritedBlueprints.Add(blueprint);

                    if (Settings.Default.Favorites.Contains($"{blueprint}"))
                    {
                        Settings.Default.Favorites.Remove($"{blueprint}");
                        Settings.Default.Save();
                    }
                }
                else if (Settings.Default.Favorites.Contains($"{blueprint}"))
                {
                    blueprint.Favorite = true;
                    favoritedBlueprints.Add(blueprint);
                    Settings.Default.Favorites.Remove($"{blueprint}");
                    Settings.Default.Favorites.Add($"{CommanderName}:{blueprint}");
                    Settings.Default.Save();
                }

                if (Settings.Default.Ignored.Contains($"{CommanderName}:{blueprint}"))
                {
                    blueprint.Ignored = true;

                    if (Settings.Default.Ignored.Contains($"{blueprint}"))
                    {
                        Settings.Default.Ignored.Remove($"{blueprint}");
                        Settings.Default.Save();
                    }
                }
                else if (Settings.Default.Ignored.Contains($"{blueprint}"))
                {
                    blueprint.Ignored = true;
                    Settings.Default.Ignored.Remove($"{blueprint}");
                    Settings.Default.Ignored.Add($"{CommanderName}:{blueprint}");
                    Settings.Default.Save();
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

            Filters = new BlueprintFilters(State.Blueprints);
        }

        public override string ToString()
        {
            return $"CMDR {CommanderName}";
        }
    }
}