using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Models;
using EDEngineer.Models.Barda;
using EDEngineer.Models.Filters;
using EDEngineer.Utils;

namespace EDEngineer
{
    public class BlueprintFilters : INotifyPropertyChanged
    {
        public List<GradeFilter> GradeFilters { get; }
        public List<EngineerFilter> EngineerFilters { get; }
        public List<TypeFilter> TypeFilters { get; }
        public List<IgnoredFavoriteFilter> IgnoredFavoriteFilters { get; }
        public List<CraftableFilter> CraftableFilters { get; }
        private List<IngredientFilter> IngredientFilters { get; }
        public IEnumerable<IGrouping<Kind, IngredientFilter>> GroupedIngredientFilters => IngredientFilters.GroupBy(f => f.Entry.Data.Kind); 

        public BlueprintFilters(IReadOnlyCollection<Blueprint> availableBlueprints)
        {
            GradeFilters = new List<GradeFilter>(availableBlueprints.GroupBy(b => b.Grade)
                .Select(b => b.Key)
                .OrderBy(b => b)
                .Select(g => new GradeFilter(g, $"GF{g}") {  Checked = true }));

            EngineerFilters = new List<EngineerFilter>(availableBlueprints.SelectMany(b => b.Engineers)
                .Distinct()
                .OrderBy(b => b)
                .Select(g => new EngineerFilter(g, $"EF{g}") { Checked = true }));

            TypeFilters = new List<TypeFilter>(availableBlueprints.GroupBy(b => b.Type)
                .Select(b => b.Key)
                .OrderBy(b => b)
                .Select(g => new TypeFilter(g, $"TF{g}") { Checked = true }));

            IngredientFilters = new List<IngredientFilter>(availableBlueprints.SelectMany(b => b.Ingredients)
                .Select(ingredient => ingredient.Entry)
                .Distinct()
                .OrderBy(b => b.Data.Kind)
                .ThenBy(b => b.Data.Name)
                .Select(g => new IngredientFilter(g, $"IF{g.Data.Kind}-{g.Data.Name}") { Checked = false }));
            
            IgnoredFavoriteFilters = new List<IgnoredFavoriteFilter>
            {
                new IgnoredFavoriteFilter("Neither", blueprint => !blueprint.Ignored && !blueprint.Favorite, "IFFnone")
                {
                    Checked = true,
                },
                new IgnoredFavoriteFilter("Favorite", blueprint => blueprint.Favorite, "IFFfavorite")
                {
                    Checked = true,
                },
                new IgnoredFavoriteFilter("Ignored", blueprint => blueprint.Ignored, "IFFignored")
                {
                    Checked = false,
                }
            };

            CraftableFilters = new List<CraftableFilter>
            {
                new CraftableFilter("Craftable", blueprint => blueprint.CanCraftCount >= 1, "CFcraftable")
                {
                    Checked = true
                },
                new CraftableFilter("Non Craftable", blueprint => blueprint.CanCraftCount == 0, "CFnoncraftable")
                {
                    Checked = true
                },
                new CraftableFilter("Missing Commodities", blueprint => blueprint.JustMissingCommodities, "CFmissingcommodities")
                {
                    Checked = true
                }
            };

            AllFilters = GradeFilters.Cast<BlueprintFilter>()
                .Union(EngineerFilters)
                .Union(TypeFilters)
                .Union(IgnoredFavoriteFilters)
                .Union(CraftableFilters).ToList();

            LoadSavedFilters();

            InsertMagicFilters();
        }

        private void LoadSavedFilters()
        {
            if (Properties.Settings.Default.FiltersChecked == null)
            {
                Properties.Settings.Default.FiltersChecked = new StringCollection();
                foreach (var filter in AllFilters.Where(filter => filter.Checked))
                {
                    Properties.Settings.Default.FiltersChecked.Add(filter.UniqueName);
                }
            }
            else
            {
                foreach (var filter in AllFilters)
                {
                    filter.Checked = Properties.Settings.Default.FiltersChecked.Contains(filter.UniqueName);

                    filter.PropertyChanged += (o, e) =>
                    {
                        if (filter.Checked)
                        {
                            Properties.Settings.Default.FiltersChecked.Add(filter.UniqueName);
                        }
                        else
                        {
                            Properties.Settings.Default.FiltersChecked.Remove(filter.UniqueName);
                        }

                        Properties.Settings.Default.Save();
                    };
                }
            }

            Properties.Settings.Default.Save();
        }

        private void InsertMagicFilters()
        {
            var magicTypeFilters = TypeFilter.MagicFilter;
            TypeFilters.Insert(0, magicTypeFilters);

            var magicEngineerFilters = EngineerFilter.MagicFilter;
            EngineerFilters.Insert(0, magicEngineerFilters);

            var magicGradeFilter = GradeFilter.MagicFilter;
            GradeFilters.Insert(0, magicGradeFilter);

            magicGradeFilter.PropertyChanged += (o, e) =>
            {
                foreach (var filter in GradeFilters)
                {
                    filter.Checked = magicGradeFilter.Checked;
                }
            };

            magicEngineerFilters.PropertyChanged += (o, e) =>
            {
                foreach (var filter in EngineerFilters)
                {
                    filter.Checked = magicEngineerFilters.Checked;
                }
            };

            magicTypeFilters.PropertyChanged += (o, e) =>
            {
                foreach (var filter in TypeFilters)
                {
                    filter.Checked = magicTypeFilters.Checked;
                }
            };
        }

        public List<BlueprintFilter> AllFilters { get; }

        public void Monitor(CollectionViewSource source, IEnumerable<Entry> entries)
        {
            foreach (var filter in AllFilters.Concat(IngredientFilters))
            {
                filter.PropertyChanged += (o, e) =>
                {
                    source.View.Refresh();
                    OnPropertyChanged(nameof(EngineerFilters));
                    OnPropertyChanged(nameof(GradeFilters));
                    OnPropertyChanged(nameof(TypeFilters));
                    OnPropertyChanged(nameof(CraftableFilters));
                    OnPropertyChanged(nameof(IgnoredFavoriteFilters));
                };
            }

            foreach (var item in entries)
            {
                item.PropertyChanged += (o, e) =>
                {
                    var extended = e as PropertyChangedExtendedEventArgs<int>;

                    if (e.PropertyName == "Count" || extended?.OldValue * extended?.NewValue == 0)
                    {
                        Application.Current.Dispatcher.Invoke(source.View.Refresh);
                    }
                };
            }

            foreach (var blueprint in (IEnumerable<Blueprint>) source.Source)
            {
                blueprint.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == "Favorite" || e.PropertyName == "Ignored")
                    {
                        source.View.Refresh();
                    }
                };
            }
            
            source.Filter+= (o,e) =>
            {
                var blueprint = (Blueprint)e.Item;
                var checkedIngredients = IngredientFilters.Where(f => f.Checked).ToList();
                var satisfyIngredientFilters =  !checkedIngredients.Any() || checkedIngredients.Any(i => blueprint.Ingredients.Any(b => b.Entry == i.Entry));

                var ret = satisfyIngredientFilters &&
                          GradeFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          EngineerFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          TypeFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          IgnoredFavoriteFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          CraftableFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint));

                e.Accepted = ret;
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}