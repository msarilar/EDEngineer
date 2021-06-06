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
        public List<CategoryFilter> CategoryFilters { get; }

        private bool _isMagicUpdate;

        private TypeFilter _magicTypeFilter;
        private EngineerFilter _magicEngineerFilter;
        private GradeFilter _magicGradeFilter;

        public string SearchText
        {
            get => searchText;
            set
            {
                if (searchText != value)
                {
                    searchText = value;
                    trimmedSearchText = value?.Trim().ToLowerInvariant();
                    OnPropertyChanged();
                }
            }
        }

        private string trimmedSearchText;

        public BlueprintFilters(ILanguage language, IReadOnlyCollection<Blueprint> availableBlueprints)
        {
            GradeFilters = new List<GradeFilter>(availableBlueprints.GroupBy(b => b.Grade)
                .Select(b => b.Key)
                .Where(b => b.HasValue)
                .OrderBy(b => b)
                .Select(g => new GradeFilter(g.GetValueOrDefault(), $"GF{g}") { Checked = true }));

            EngineerFilters = new List<EngineerFilter>(availableBlueprints.SelectMany(b => b.Engineers)
                .Distinct()
                .Except("@Synthesis", "@Technology", "@Merchant")
                .OrderBy(language.Translate)
                .Select(e => new EngineerFilter(e, $"EF{e}") { Checked = true }));

            TypeFilters = new List<TypeFilter>(availableBlueprints
                                               .Where(b => b.Category != BlueprintCategory.Synthesis &&
                                                           b.Category != BlueprintCategory.Technology)
                                               .GroupBy(b => b.Type)
                                               .Select(b => b.Key)
                                               .OrderBy(language.Translate)
                                               .Select(t => new TypeFilter(t, $"TF{t}") { Checked = true }));

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
                },
                new IgnoredFavoriteFilter("Shopping List", blueprint => blueprint.ShoppingListCount > 0, "IFFshoppingList")
                {
                    Checked = true,
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
                }
            };

            CategoryFilters = Enum.GetValues(typeof(BlueprintCategory))
                                  .Cast<BlueprintCategory>()
                                  .Select(c => new CategoryFilter(c, $"BCF{c}"))
                                  .ToList();

            InsertMagicFilters();
            AllFilters = GradeFilters.Cast<BlueprintFilter>()
                .Union(EngineerFilters)
                .Union(TypeFilters)
                .Union(IgnoredFavoriteFilters)
                .Union(CraftableFilters)
                .Union(CategoryFilters).ToList();

            LoadSavedFilters();

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

                        if (!_isMagicUpdate)
                        {
                            Properties.Settings.Default.Save();
                        }
                    };
                }
            }

            Properties.Settings.Default.Save();
        }

        private void InsertMagicFilters()
        {
            _magicTypeFilter = TypeFilter.MagicFilter;
            TypeFilters.Insert(0, _magicTypeFilter);

            _magicEngineerFilter = EngineerFilter.MagicFilter;
            EngineerFilters.Insert(0, _magicEngineerFilter);

            _magicGradeFilter = GradeFilter.MagicFilter;
            GradeFilters.Insert(0, _magicGradeFilter);

            var synthesisTypeFilter = new TypeFilter("@Synthesis", "TFSynthesis");
            TypeFilters.Add(synthesisTypeFilter);

            var technologyTypeFilter = new TypeFilter("@Technology", "TFTechnology");
            TypeFilters.Add(technologyTypeFilter);
        }

        private void UpdateFiltersFromMagicFilter(CollectionViewSource source, bool isChecked, IEnumerable<BlueprintFilter> filters)
        {
            if (!_isMagicUpdate)
            {
                _isMagicUpdate = true;
                foreach (var filter in filters)
                {
                    filter.Checked = isChecked;
                }
                _isMagicUpdate = false;

                //Update view once done
                Properties.Settings.Default.Save();

                source.View.Refresh();
                OnPropertyChanged(nameof(EngineerFilters));
                OnPropertyChanged(nameof(GradeFilters));
                OnPropertyChanged(nameof(TypeFilters));
                OnPropertyChanged(nameof(CraftableFilters));
                OnPropertyChanged(nameof(IgnoredFavoriteFilters));
            }
        }

        private void UpdateMagicFilterFromFilters(BlueprintFilter updatedFilter)
        {
            IEnumerable<BlueprintFilter> group = null;
            BlueprintFilter magicFilter = null;

            switch (updatedFilter.GetType().Name)
            {
                case nameof(GradeFilter):
                    group = GradeFilters;
                    magicFilter = _magicGradeFilter;
                    break;
                case nameof(TypeFilter):
                    group = TypeFilters;
                    magicFilter = _magicTypeFilter;
                    break;
                case nameof(EngineerFilter):
                    group = EngineerFilters;
                    magicFilter = _magicEngineerFilter;
                    break;
                default:
                    break;
            }

            if (group != null && magicFilter != null)
            {
                foreach (var filter in group)
                {
                    if (filter != magicFilter)
                    {
                        if (filter.Checked && !magicFilter.Checked)
                        {
                            _isMagicUpdate = true;
                            magicFilter.Checked = true;
                            _isMagicUpdate = false;
                        }

                        if (filter.Checked && magicFilter.Checked)
                        {
                            return;
                        }
                    }
                }

                magicFilter.Checked = false;
            }
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

            _magicGradeFilter.PropertyChanged += (o, e) => UpdateFiltersFromMagicFilter(source, _magicGradeFilter.Checked, GradeFilters);
            _magicEngineerFilter.PropertyChanged += (o, e) => UpdateFiltersFromMagicFilter(source, _magicEngineerFilter.Checked, EngineerFilters);
            _magicTypeFilter.PropertyChanged += (o, e) => UpdateFiltersFromMagicFilter(source, _magicTypeFilter.Checked, TypeFilters);

            foreach (var filter in AllFilters)
            {
                filter.PropertyChanged += (o, e) =>
                {
                    if (!_isMagicUpdate)
                    {
                        source.View.Refresh();
                        OnPropertyChanged(nameof(EngineerFilters));
                        OnPropertyChanged(nameof(GradeFilters));
                        OnPropertyChanged(nameof(TypeFilters));
                        OnPropertyChanged(nameof(CraftableFilters));
                        OnPropertyChanged(nameof(IgnoredFavoriteFilters));
                        UpdateMagicFilterFromFilters((BlueprintFilter)o);
                    }
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

            foreach (var blueprint in (IEnumerable<Blueprint>)source.Source)
            {
                blueprint.PropertyChanged += (o, e) =>
                {
                    if (e.PropertyName == "Favorite" || e.PropertyName == "Ignored")
                    {
                        source.View.Refresh();
                    }
                };
            }

            source.Filter += (o, e) =>
            {
                var blueprint = (Blueprint)e.Item;

                var satisfySearchText = string.IsNullOrWhiteSpace(trimmedSearchText) ||
                                        trimmedSearchText.Split(' ').All(t =>
                                        blueprint.SearchableContent.IndexOf(t.Trim(),
                                            StringComparison.Ordinal) >= 0);

                var satisfyHighlightedFilters = !highlightedEntryData.Any() ||
                                                highlightedEntryData.Intersect(
                                                    blueprint.Ingredients.Select(i => i.Entry)).Any();

                var ret = satisfySearchText &&
                          satisfyHighlightedFilters &&
                          GradeFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          EngineerFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          TypeFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          IgnoredFavoriteFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          CraftableFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          (!CategoryFilters.Where(f => f.Checked).Any() || CategoryFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)));

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