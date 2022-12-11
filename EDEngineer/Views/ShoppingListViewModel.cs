using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models;
using EDEngineer.Models.MaterialTrading;
using EDEngineer.Models.State;
using EDEngineer.Models.Utils;
using EDEngineer.Utils.System;

namespace EDEngineer.Views
{
    public class ShoppingListViewModel : INotifyPropertyChanged, IShoppingList
    {
        private bool synchronizeWithLogs;
        private readonly StateCargo stateCargo;
        private readonly ILanguage languages;
        private readonly List<IGrouping<Tuple<string, string>, Blueprint>> blueprints;
        public List<Blueprint> list = null;

        public ShoppingListViewModel(StateCargo stateCargo, List<Blueprint> blueprints, ILanguage languages)
        {
            this.blueprints = blueprints.GroupBy(b => Tuple.Create(b.Type, b.BlueprintName)).ToList();
            this.stateCargo = stateCargo;
            this.languages = languages;
            list = this.ToList();

            foreach (var blueprint in blueprints)
            {
                blueprint.PropertyChanged += (o, e) =>
                                             {
                                                 if (e.PropertyName == "ShoppingListCount")
                                                 {
                                                     list = this.ToList();
                                                 }
                                             };
            }

            foreach (var ingredientsValue in stateCargo.Ingredients)
            {
                ingredientsValue.Value.PropertyChanged += (o, e) =>
                                                          {
                                                              if (e.PropertyName == "Count")
                                                              {
                                                                  OnPropertyChanged(nameof(MaterialTrades));
                                                              }
                                                          };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Blueprint> List => list;
        public ILanguage Languages => this.languages;

        public List<Tuple<Blueprint, int>> Composition
            => blueprints.SelectMany(b => b).Where(b => b.ShoppingListCount > 0)
                         .Select(b => Tuple.Create(b, b.ShoppingListCount)).ToList();

        public Dictionary<Tuple<EntryData, bool, int>, List<MaterialTrade>> MaterialTrades =>
            MissingIngredients.Map(i => MaterialTrader.FindPossibleTrades(stateCargo, i, Deduction))
                              ?.GroupBy(i => i.Needed)
                              .OrderBy(i => languages.Translate(i.Key.Data.Name))
                              .ToDictionary(i => Tuple.Create(i.Key.Data, i.First().WillBeEnough, i.First().Missing), i => i.OrderBy(j => j.Consumption).Take(4).ToList()); 

        public List<ShoppingListBlock> CompositionForGui
        {
            get
            {
                var allGrades = SettingsManager.ShowAllGrades;
                var list = blueprints
                                 .Where(g => g.Any(b => b.ShoppingListCount > 0))
                                 .Select(g => new ShoppingListBlock(g.First().ShortString,
                                             g.First().TranslatedString,
                                             g.Select(b => Tuple.Create(b, b.ShoppingListCount)).ToList(),
                                             g.First().Category,
                                             allGrades.Contains(g.First().ShortString)))
                                 .ToList();

                var result = SmartSort(list);

                return result;
            }
        }

        private static List<ShoppingListBlock> SmartSort(List<ShoppingListBlock> list)
        {
            var result = new List<ShoppingListBlock>();
            for (var i = 0; i < list.Count; i++)
            {
                var found = false;
                var current = list[i];
                if (current.Composition.Count > 3)
                {
                    result.Add(current);
                    found = true;
                }
                else if (current.Composition.Count > 1)
                {
                    result.Add(current);
                    // find the first element of size 1 and add it if it exists:
                    for (var j = i + 1; j < list.Count; j++)
                    {
                        var next = list[j];
                        if (next.Composition.Count <= 1)
                        {
                            var temp = list[i + 1];
                            list[i + 1] = next;
                            list[j] = temp;
                            result.Add(next);
                            i++;
                            found = true;
                            break;
                        }
                    }
                }
                else
                {
                    result.Add(current);
                    // find next item of intermediary size and add it if it exists:
                    for (var j = i + 1; j < list.Count; j++)
                    {
                        var next = list[j];
                        if (next.Composition.Count <= 3 && next.Composition.Count > 1)
                        {
                            found = true;

                            var temp = list[i + 1];
                            list[i + 1] = next;
                            list[j] = temp;

                            result.Add(next);

                            i++;
                            break;
                        }
                    }

                    if (found)
                    {
                        continue;
                    }

                    // since no intermediary size item could be found, let's find the next 2 one size items:
                    int? first = null;
                    for (var j = i + 1; j < list.Count; j++)
                    {
                        var next = list[j];
                        if (next.Composition.Count == 1)
                        {
                            if (first == null)
                            {
                                first = j;
                            }
                            else
                            {
                                // swap first
                                var temp = list[i + 1];
                                list[i + 1] = list[first.Value];
                                list[first.Value] = temp;

                                // swap second
                                temp = list[i + 2];
                                list[i + 2] = list[j];
                                list[j] = temp;

                                result.Add(list[i + 1]);
                                result.Add(list[i + 2]);
                                i += 2;

                                found = true;
                                break;
                            }
                        }
                    }
                }

                // if no complete line could be made, find the biggest remaining element and swap with current one:
                if (!found && i < list.Count - 1)
                {
                    var max = i + 1;
                    for (var j = i + 1; j < list.Count; j++)
                    {
                        if (list[max].Composition.Count < list[j].Composition.Count)
                        {
                            max = j;
                        }
                    }

                    var temp = list[max];
                    list[max] = list[i];
                    list[i] = temp;

                    result.RemoveAt(i);
                    result.Add(temp);
                }
            }

            return result;
        }

        public bool SynchronizeWithLogs
        {
            get => synchronizeWithLogs;
            set
            {
                synchronizeWithLogs = value;
                SettingsManager.SyncShoppingList = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<EntryData, int> Deduction =>
            list.FirstOrDefault()
                ?.Ingredients
                .ToDictionary(i => i.Entry.Data, i => i.Size);

        public Dictionary<Entry, int> MissingIngredients =>
            list.FirstOrDefault()
                ?.Ingredients
                .Where(i => i.Size - i.Entry.Count > 0)
                .ToDictionary(i => i.Entry, i => i.Size - i.Entry.Count);

        public IEnumerator<Blueprint> GetEnumerator()
        {
            var ingredients = new Dictionary<Entry, int>();
            foreach (var b in blueprints.SelectMany(b => b).Where(b => b.ShoppingListCount > 0))
            {
                foreach (var ingredient in b.Ingredients)
                {
                    if (!ingredients.ContainsKey(ingredient.Entry))
                    {
                        ingredients[ingredient.Entry] = 0;
                    }
                    ingredients[ingredient.Entry] += ingredient.Size * b.ShoppingListCount;
                }
            }

            if (!ingredients.Any())
            {
                yield break;
            }

            var composition = new List<BlueprintIngredient>();
            foreach (var i in ingredients)
            {
                composition.Add(new BlueprintIngredient(i.Key, i.Value));
            }

            composition = composition.OrderBy(i => i.Entry.Count - i.Size > 0 ? 1 : 0)
                                     .ThenByDescending(i => i.Entry.Data.Subkind)
                                     .ThenBy(i => i.Entry.Data.Kind)
                                     .ThenBy(i => languages.Translate(i.Entry.Data.Name))
                                     .ToList();

            var metaBlueprint = new Blueprint(languages,
                "",
                "Shopping List",
                null,
                composition,
                new string[0],
                Enumerable.Empty<BlueprintEffect>().ToList(),
                null);

            yield return metaBlueprint;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}