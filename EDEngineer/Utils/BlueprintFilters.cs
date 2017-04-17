using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using EDEngineer.Models;
using EDEngineer.Models.Filters;
using EDEngineer.Models.Utils;

namespace EDEngineer.Utils
{
    public class BlueprintFilters : INotifyPropertyChanged
    {
        private string searchText;
        public List<GradeFilter> GradeFilters { get; }
        public List<EngineerFilter> EngineerFilters { get; }
        public List<TypeFilter> TypeFilters { get; }
        public List<IgnoredFavoriteFilter> IgnoredFavoriteFilters { get; }
        public List<CraftableFilter> CraftableFilters { get; }

        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    OnPropertyChanged();
                }
            }
        }

        public BlueprintFilters(ILanguage language, IReadOnlyCollection<Blueprint> availableBlueprints)
        {
            GradeFilters = new List<GradeFilter>(availableBlueprints.GroupBy(b => b.Grade)
                .Select(b => b.Key)
                .OrderBy(b => b)
                .Select(g => new GradeFilter(g, $"GF{g}") {  Checked = true }));

            EngineerFilters = new List<EngineerFilter>(availableBlueprints.SelectMany(b => b.Engineers)
                .Distinct()
                .OrderBy(language.Translate)
                .Select(g => new EngineerFilter(g, $"EF{g}") { Checked = true }));

            TypeFilters = new List<TypeFilter>(availableBlueprints.GroupBy(b => b.Type)
                .Select(b => b.Key)
                .OrderBy(language.Translate)
                .Select(g => new TypeFilter(g, $"TF{g}") { Checked = true }));

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
                /* COMMODITY REMOVED
                new CraftableFilter("Missing Commodities", blueprint => blueprint.JustMissingCommodities, "CFmissingcommodities")
                {
                    Checked = true
                }*/
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

        public void Monitor(CollectionViewSource source, IEnumerable<Entry> entries, ObservableCollection<Entry> highlightedEntryData)
        {
            PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(SearchText))
                {
                    source.View.Refresh();
                }
            };

            foreach (var filter in AllFilters)
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

            highlightedEntryData.CollectionChanged += (o, e) =>
            {
                source.View.Refresh();
            };

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
            
            source.Filter += (o,e) =>
            {
                var blueprint = (Blueprint)e.Item;

                var satisfySearchText = string.IsNullOrWhiteSpace(SearchText) ||
                                        SearchText.Split(' ').All(t => 
                                        blueprint.SearchableContent.IndexOf(t.Trim(),
                                            StringComparison.InvariantCultureIgnoreCase) >= 0);

                var satisfyHighlightedFilters = !highlightedEntryData.Any() ||
                                                highlightedEntryData.Intersect(
                                                    blueprint.Ingredients.Select(i => i.Entry)).Any();

                var ret = satisfySearchText &&
                          satisfyHighlightedFilters &&
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