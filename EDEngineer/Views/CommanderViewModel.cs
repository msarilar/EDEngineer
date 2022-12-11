using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Operations;
using EDEngineer.Models.State;
using EDEngineer.Models.Utils;
using EDEngineer.Properties;
using EDEngineer.Utils;
using EDEngineer.Utils.System;
using EDEngineer.Views.Notifications;
using Newtonsoft.Json;
using NodaTime;

namespace EDEngineer.Views
{
    public class CommanderViewModel : INotifyPropertyChanged, IDisposable
    {
        public string CommanderName { get; }
        public State State { get; }
        public BlueprintFilters Filters { get; private set; }
        public ObservableCollection<Entry> HighlightedEntryData { get; } = new ObservableCollection<Entry>();

        public ShoppingListViewModel ShoppingList { get; private set; }

        private readonly JournalEntryConverter journalEntryConverter;
        public JsonSerializerSettings JsonSettings { get; }

        private readonly HashSet<Blueprint> favoritedBlueprints = new HashSet<Blueprint>();
        private Instant lastUpdate = Instant.MinValue;
        private readonly CommanderNotifications commanderNotifications;

        public Instant LastUpdate
        {
            get => lastUpdate;
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

        public void LoadLogs(IEnumerable<string> events)
        {
            commanderNotifications?.UnsubscribeNotifications();
            State.Cargo.InitLoad();
            // Clear state:

            State.Cargo.Ingredients.ToList().ForEach(k => State.Cargo.IncrementCargo(k.Value.Data.Name, -1 * k.Value.Count));
            LastUpdate = Instant.MinValue;

            ApplyEventsToSate(events);
            commanderNotifications?.SubscribeNotifications();

            State.Cargo.Ingredients.RefreshSort();
            State.Cargo.CompleteLoad();
        }

        public CommanderViewModel(string commanderName, Action<CommanderViewModel> loadAction, Languages languages, List<EntryData> entryDatas, List<Equipment> equipments)
        {
            CommanderName = commanderName;

            var equipmentByName = equipments.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
            var converter = new ItemNameConverter(entryDatas, equipmentByName);

            State = new State(new StateCargo(entryDatas, equipments, languages, SettingsManager.Comparer));

            commanderNotifications = new CommanderNotifications(State);
            var blueprintConverter = new BlueprintConverter(State.Cargo.Ingredients);

            var blueprintsJson = Helpers.GetBlueprintsJson();
            var blueprints =
                JsonConvert.DeserializeObject<List<Blueprint>>(blueprintsJson, blueprintConverter)
                           .Where(b => b.Ingredients.Any())
                           .ToList();

            journalEntryConverter = new JournalEntryConverter(converter, State.Cargo.Ingredients, languages, blueprints);
            JsonSettings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { journalEntryConverter },
                Error = (o, e) => e.ErrorContext.Handled = true
            };
            LoadBlueprints(languages, blueprints);
            languages.PropertyChanged += (o, e) => OnPropertyChanged(nameof(Filters));

            loadAction(this);

            State.BlueprintCrafted += (o, e) =>
                                      {
                                          TryRemoveFromShoppingListByIngredients(e.Category, e.TechnicalType,
                                              e.IngredientsConsumed);
                                          State.ApplyCraft(e);
                                      };
            ShoppingList.SynchronizeWithLogs = SettingsManager.SyncShoppingList;

            languages.PropertyChanged += (o, e) =>
                                         {
                                             OnPropertyChanged(nameof(ShoppingList));
                                             OnPropertyChanged(nameof(ShoppingListItem));
                                         };

            var datas = State.Cargo.Ingredients.Select(c => c.Value.Data);
            var ingredientUsed = State.Blueprints.SelectMany(blueprint => blueprint.Ingredients);
            var ingredientUsedNames = ingredientUsed.Select(ingredient => ingredient.Entry.Data.Name).Distinct();
            var unusedIngredients = datas.Where(data => !ingredientUsedNames.Contains(data.Name));

            foreach (var data in unusedIngredients)
            {
                data.Unused = true;
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
                Timestamp = SystemClock.Instance.GetCurrentInstant()
            };

            var json = JsonConvert.SerializeObject(logEntry, journalEntryConverter);

            logEntry.OriginalJson = json;

            MutateState(logEntry);

            return logEntry;
        }

        public void ApplyEventsToSate(IEnumerable<string> allLogs)
        {
            var entries = allLogs.Select(l => JsonConvert.DeserializeObject<JournalEntry>(l, JsonSettings))
                .Where(e => e?.Relevant == true);

            foreach (var entry in entries.Where(entry => entry.Timestamp >= LastUpdate).ToList())
            {
                MutateState(entry);
            }
        }

        private void MutateState(JournalEntry entry)
        {
            State.Operations.AddLast(entry);
            entry.JournalOperation.Mutate(State);
            LastUpdate = Instant.Max(LastUpdate, entry.Timestamp);
        }

        public ICollectionView FilterView(MainWindowViewModel parentViewModel, Kind kind, CollectionViewSource source)
        {
            source.Filter += (o, e) =>
            {
                var entry = ((KeyValuePair<string, Entry>)e.Item).Value;

                e.Accepted = ((entry.Data.Kind & kind) == entry.Data.Kind || entry.Data.Kind == Kind.Unknown) &&
                       (parentViewModel.MaterialSubkindFilter == null || entry.Data.Kind == Kind.Data || parentViewModel.MaterialSubkindFilter == entry.Data.Subkind) &&
                       (parentViewModel.ShowZeroes || entry.Count != 0) &&
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

            if (parentViewModel.IngredientsGrouped)
            {
                source.View.GroupDescriptions.Add(new PropertyGroupDescription("Value.Data.Group"));
            }

            return source.View;
        }

        private void LoadBlueprints(ILanguage languages, IEnumerable<Blueprint> blueprints)
        {
            State.Blueprints = new List<Blueprint>(blueprints);
            if (Settings.Default.Favorites == null)
            {
                Settings.Default.Favorites = new StringCollection();
            }

            if (Settings.Default.Ignored == null)
            {
                Settings.Default.Ignored = new StringCollection();
            }

            if (Settings.Default.ShoppingList == null)
            {
                Settings.Default.ShoppingList = new StringCollection();
            }

            if (Settings.Default.ShowAllGrades == null)
            {
                Settings.Default.ShowAllGrades = new StringCollection();
            }

            if (Settings.Default.CollapsedIngredientGroups == null)
            {
                Settings.Default.CollapsedIngredientGroups = new StringCollection();
            }

            foreach (var blueprint in State.Blueprints)
            {
                var text = $"{CommanderName}:{blueprint}";

                if (Settings.Default.Favorites.Contains(text))
                {
                    blueprint.Favorite = true;
                    favoritedBlueprints.Add(blueprint);

                    if (Settings.Default.Favorites.Contains($"{blueprint}"))
                    {
                        Settings.Default.Favorites.Remove($"{blueprint}");
                    }
                }
                else if (Settings.Default.Favorites.Contains($"{blueprint}"))
                {
                    blueprint.Favorite = true;
                    favoritedBlueprints.Add(blueprint);
                    Settings.Default.Favorites.Remove($"{blueprint}");
                    Settings.Default.Favorites.Add(text);
                }

                if (Settings.Default.Ignored.Contains(text))
                {
                    blueprint.Ignored = true;

                    if (Settings.Default.Ignored.Contains($"{blueprint}"))
                    {
                        Settings.Default.Ignored.Remove($"{blueprint}");
                    }
                }
                else if (Settings.Default.Ignored.Contains($"{blueprint}"))
                {
                    blueprint.Ignored = true;
                    Settings.Default.Ignored.Remove($"{blueprint}");
                    Settings.Default.Ignored.Add(text);
                }

                blueprint.ShoppingListCount = Settings.Default.ShoppingList.Cast<string>().Count(l => l == text);

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
                    else if (e.PropertyName == "ShoppingListCount")
                    {
                        while (Settings.Default.ShoppingList.Contains(text))
                        {
                            Settings.Default.ShoppingList.Remove(text);
                        }

                        for (var i = 0; i < blueprint.ShoppingListCount; i++)
                        {
                            Settings.Default.ShoppingList.Add(text);
                        }

                        Settings.Default.Save();
                    }
                };
            }

            Settings.Default.Save();
            Filters = new BlueprintFilters(languages, State.Blueprints);

            ShoppingList = new ShoppingListViewModel(State.Cargo, State.Blueprints, languages);
        }

        public void TryRemoveFromShoppingListByIngredients(BlueprintCategory category, string technicalModuleName, List<BlueprintIngredient> blueprintIngredients)
        {
            if (!ShoppingList.SynchronizeWithLogs)
            {
                return;
            }

            var blueprints = ShoppingList
                .Composition
                .Select(x => x.Item1)
                .ToList();

            if (category == BlueprintCategory.Module)
            {
                var blueprint = blueprints.FirstOrDefault(b => b.Category == BlueprintCategory.Module &&
                                                               b.TechnicalType.IsIn(technicalModuleName) &&
                                                               b.HasSameIngredients(blueprintIngredients));
                if (blueprint == null)
                {
                    var experimentals = blueprints.Where(b => b.Category == BlueprintCategory.Experimental).ToList();
                    foreach (var experimental in experimentals)
                    {
                        foreach (var module in blueprints.Where(
                            b => b.Category == BlueprintCategory.Module && b.TechnicalType.IsIn(technicalModuleName)))
                        {
                            var mergedIngredients = experimental.Ingredients.Concat(module.Ingredients).ToList();
                            if (mergedIngredients.Count == blueprintIngredients.Count &&
                                mergedIngredients.All(blueprintIngredients.Contains))
                            {
                                ShoppingListChange(experimental, -1);
                                ShoppingListChange(module, -1);

                                return;
                            }
                        }
                    }

                    blueprint = blueprints.FirstOrDefault(b => b.HasSameIngredients(blueprintIngredients));
                }

                if (blueprint != null)
                {
                    ShoppingListChange(blueprint, -1);
                }
            }
            else
            {
                var blueprint = blueprints.FirstOrDefault(b =>
                {
                    if (b.Category != category)
                    {
                        return false;
                    }

                    switch (b.Category)
                    {
                        case BlueprintCategory.Synthesis:
                        case BlueprintCategory.Technology:
                            return b.HasSameIngredients(blueprintIngredients);
                        default:
                            return false;
                    }
                });

                if (blueprint != null)
                {
                    ShoppingListChange(blueprint, -1);
                }
            }
        }

        public void ShoppingListChange(Blueprint blueprint, int i)
        {
            if (i != 0 && blueprint.ShoppingListCount + i >= 0)
            {
                blueprint.ShoppingListCount += i;

                OnPropertyChanged(nameof(ShoppingList));
                OnPropertyChanged(nameof(ShoppingListItem));
            }
        }

        public void ImportShoppingList()
        {
            if (Helpers.TryRetrieveShoppingList(out var shoppingListItems))
            {
                var blueprints = State.Blueprints;

                if (shoppingListItems != null && shoppingListItems.Count > 0)
                {
                    // Configure the message box to be displayed
                    var messageBoxText = "Do you want to clear the shopping list before import?";
                    var caption = "Shopping List Import";
                    var button = MessageBoxButton.YesNoCancel;
                    var icon = MessageBoxImage.Warning;

                    // Display message box
                    var result = MessageBox.Show(messageBoxText, caption, button, icon);

                    // Process message box results
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            ClearShoppingList();
                            break;
                        case MessageBoxResult.No:
                            // User pressed No, so just load into shopping list without clearing
                            break;
                        case MessageBoxResult.Cancel:
                            // User pressed Cancel button so skip out of Import
                            return;
                    }

                    LoadShoppingListItems(shoppingListItems, blueprints);
                    Settings.Default.Save();
                }

                RefreshShoppingList();

            }
        }

        private void LoadShoppingListItems(StringCollection shoppingListItems, List<Blueprint> blueprints)
        {
            var blueprintsByString = blueprints.ToDictionary(b => b.ToString(), b => b);
            foreach (var item in shoppingListItems)
            {
                if (item == null)
                {
                    continue;
                }

                var itemName = item.Split(':');

                if (blueprintsByString.TryGetValue(itemName[1], out var blueprint))
                {
                    ShoppingListChange(blueprint, 1);
                }
            }
        }

        public void ExportShoppingList()
        {
            try
            {
                Helpers.SaveShoppingList(CommanderName);
            }
            catch(Exception e)
            {
                MessageBox.Show("Shopping list could not be saved." + Environment.NewLine + Environment.NewLine + e, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClearShoppingList()
        {
            foreach (var tuple in ShoppingList.Composition.ToList())
            {
                ShoppingListChange(tuple.Item1, tuple.Item2 * -1);
            }
        }

        public int ShoppingListItem => 0;

        public override string ToString()
        {
            return $"CMDR {CommanderName}";
        }

        public void Dispose()
        {
            commanderNotifications?.Dispose();
        }

        public void ToggleHighlight(Entry entry)
        {
            entry.Highlighted = !entry.Highlighted;

            if (entry.Highlighted)
            {
                HighlightedEntryData.Add(entry);
            }
            else
            {
                HighlightedEntryData.Remove(entry);
            }

            Settings.Default.Save();
        }

        public void HighlightShoppingListIngredient(List<BlueprintIngredient> ingredients, Blueprint blueprint, bool highlighted)
        {
            if (ingredients == null)
            {
                return;
            }

            foreach (
                var ingredient in
                    blueprint.Ingredients.Join(ingredients,
                        ingredient => ingredient.Entry.Data.Name,
                        ingredient => ingredient.Entry.Data.Name,
                        (_, ingredient) => ingredient))
            {
                ingredient.ShoppingListHighlighted = highlighted;
            }

            blueprint.ShoppingListHighlighted = highlighted;
        }

        public void HighlightShoppingListBlueprint(List<Tuple<Blueprint, int>> blueprints, BlueprintIngredient ingredient, bool highlighted)
        {
            foreach (var blueprint in blueprints.Select(i => i.Item1).Where(b => b.Ingredients.Any(i => i.Entry.Data.Name == ingredient.Entry.Data.Name)))
            {
                blueprint.ShoppingListHighlighted = highlighted;
            }

            ingredient.ShoppingListHighlighted = highlighted;
        }

        public void RefreshShoppingList()
        {
            // relevant when live reloading a commander, because WPF didn't bind upon creating the object:
            OnPropertyChanged(nameof(ShoppingList));
            OnPropertyChanged(nameof(ShoppingListItem));
        }

        public void RefreshShoppingList( StringCollection shoppingList)
        {
            var blueprints = State.Blueprints;
            LoadShoppingListItems(shoppingList, blueprints);
            Settings.Default.Save();
        }

        public void ShowAllGradeChanges(ShoppingListBlock shoppingListBlock)
        {
            shoppingListBlock.ShowAllGrades = !shoppingListBlock.ShowAllGrades;
            if (shoppingListBlock.ShowAllGrades)
            {
                SettingsManager.AddToAllGrades(shoppingListBlock.Label);
            }
            else
            {
                SettingsManager.RemoveFromAllGrades(shoppingListBlock.Label);
            }

            OnPropertyChanged(nameof(ShoppingList));
            OnPropertyChanged(nameof(ShoppingListItem));
        }

        public void LoadAggregation(StateAggregation aggregation)
        {
            State.ApplyAggregation(aggregation);
        }
    }
}