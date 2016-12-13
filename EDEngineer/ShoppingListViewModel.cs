using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models;

namespace EDEngineer
{
    public class ShoppingListViewModel : INotifyPropertyChanged, IEnumerable<Blueprint>
    {
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

        public IEnumerator<Blueprint> GetEnumerator()
        {
            var added = blueprints
                .SelectMany(b => Enumerable.Repeat(b, b.ShoppingListCount))
                .ToList();

            if (!added.Any())
            {
                yield break;
            }

            var metaBlueprint = new Blueprint(languages, "", "Shopping List", null, new BlueprintIngredient[0],
                new string[0]);
            metaBlueprint = added
                .Aggregate(metaBlueprint, (acc, current) =>
                                          {
                                              acc.Ingredients = acc.Ingredients.Concat(current.Ingredients).ToList();
                                              return acc;
                                          });

            metaBlueprint.Ingredients = metaBlueprint.Ingredients
                                                     .GroupBy(i => i.Entry.Data.Name)
                                                     .Select(
                                                         i =>
                                                             new BlueprintIngredient(i.First().Entry,
                                                                 i.Sum(c => c.Size)))
                                                     .OrderBy(i => i.Entry.Data.Kind)
                                                     .ThenBy(i => languages.Translate(i.Entry.Data.Name))
                                                     .ToList();
            yield return metaBlueprint;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}