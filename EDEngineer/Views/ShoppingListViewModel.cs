using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models;
using EDEngineer.Utils.System;

namespace EDEngineer.Views
{
    public class ShoppingListViewModel : INotifyPropertyChanged, IEnumerable<Blueprint>
    {
        private bool synchronizeWithLogs;
        private readonly ILanguage languages;
        private readonly List<IGrouping<Tuple<string, string>, Blueprint>> blueprints;

        public ShoppingListViewModel(List<Blueprint> blueprints, ILanguage languages)
        {
            this.blueprints = blueprints.GroupBy(b => Tuple.Create(b.Type, b.BlueprintName)).ToList();
            this.languages = languages;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Blueprint> List => this.ToList();

        public List<Tuple<Blueprint, int>> Composition
            => blueprints.SelectMany(b => b).Where(b => b.ShoppingListCount > 0)
                         .Select(b => Tuple.Create(b, b.ShoppingListCount)).ToList();

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
                var current = list[i];
                if (current.Composition.Count > 3)
                {
                    result.Add(current);
                }
                else if (current.Composition.Count > 1)
                {
                    result.Add(current);
                    // find the first element of size 1 and add it if it exists:
                    for (var j = i + 1; j < list.Count; j++)
                    {
                        var next = list[j];
                        if (next.Composition.Count == 1)
                        {
                            var temp = list[i + 1];
                            list[i + 1] = next;
                            list[j] = temp;
                            result.Add(next);
                            i++;
                            break;
                        }
                    }
                }
                else
                {
                    result.Add(current);
                    var found = false;
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
                                break;
                            }
                        }
                    }
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

        public IEnumerator<Blueprint> GetEnumerator()
        {
            var ingredients = blueprints
                .SelectMany(b => b)
                .SelectMany(b => Enumerable.Repeat(b, b.ShoppingListCount))
                .SelectMany(b => b.Ingredients)
                .ToList();

            if (!ingredients.Any())
            {
                yield break;
            }

            var composition = ingredients.GroupBy(i => i.Entry.Data.Name)
                                         .Select(
                                             i =>
                                                 new BlueprintIngredient(i.First().Entry,
                                                     i.Sum(c => c.Size)))
                                         .OrderBy(i => i.Entry.Data.Kind)
                                         .ThenBy(i => languages.Translate(i.Entry.Data.Name))
                                         .ToList();

            var metaBlueprint = new Blueprint(languages,
                "",
                "Shopping List",
                null,
                composition,
                new string[0]);

            yield return metaBlueprint;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}