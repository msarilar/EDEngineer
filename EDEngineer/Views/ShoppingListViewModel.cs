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
        private readonly List<Blueprint> blueprints;

        public ShoppingListViewModel(List<Blueprint> blueprints, ILanguage languages)
        {
            this.blueprints = blueprints;
            this.languages = languages;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Blueprint> List => this.ToList();

        public List<Tuple<Blueprint, int>> Composition
            => blueprints.Where(b => b.ShoppingListCount > 0)
                         .Select(b => Tuple.Create(b, b.ShoppingListCount)).ToList();

        public List<ShoppingListBlock> T
        {
            get
            {
                return blueprints.GroupBy(b => Tuple.Create(b.Type, b.BlueprintName))
                                 .Where(g => g.Any(b => b.ShoppingListCount > 0))
                                 .Select(g => new ShoppingListBlock(g.First().ShortString,
                                             g.First().TranslatedString,
                                             g.Select(b => Tuple.Create(b, b.ShoppingListCount)).ToList(),
                                             g.First().Category))
                                 .OrderByDescending(b => b.Composition.Count)
                                 .ToList();
            }
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