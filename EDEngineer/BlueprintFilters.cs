using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using EDEngineer.Filters;
using EDEngineer.Models;

namespace EDEngineer
{
    public class BlueprintFilters : INotifyPropertyChanged
    {
        public List<GradeFilter> GradeFilters { get; set; }
        public List<EngineerFilter> EngineerFilters { get; set; }
        public List<TypeFilter> TypeFilters { get; set; }
        public List<IgnoredFavoriteFilter> IgnoredFavoriteFilters { get; set; }
        public List<CraftableFilter> CraftableFilters { get; set; }

        public BlueprintFilters(IReadOnlyCollection<Blueprint> availableBlueprints)
        {
            GradeFilters = new List<GradeFilter>(availableBlueprints.GroupBy(b => b.Grade)
                .Select(b => b.Key)
                .OrderBy(b => b)
                .Select(g => new GradeFilter($"GF{g}") { Grade = g, Checked = true }));

            EngineerFilters = new List<EngineerFilter>(availableBlueprints.SelectMany(b => b.Engineers)
                .Distinct()
                .OrderBy(b => b)
                .Select(g => new EngineerFilter($"EF{g}") { Engineer = g, Checked = true }));

            TypeFilters = new List<TypeFilter>(availableBlueprints.GroupBy(b => b.Type)
                .Select(b => b.Key)
                .OrderBy(b => b)
                .Select(g => new TypeFilter($"TF{g}") { Type = g, Checked = true }));

            IgnoredFavoriteFilters = new List<IgnoredFavoriteFilter>
            {
                new IgnoredFavoriteFilter("IFFnone")
                {
                    Checked = true,
                    Label = "None",
                    AppliesToDelegate = blueprint => !blueprint.Ignored && !blueprint.Favorite
                },
                new IgnoredFavoriteFilter("IFFfavorite")
                {
                    Checked = true,
                    Label = "Favorite",
                    AppliesToDelegate = blueprint => blueprint.Favorite
                },
                new IgnoredFavoriteFilter("IFFignored")
                {
                    Checked = false,
                    Label = "Ignored",
                    AppliesToDelegate = blueprint => blueprint.Ignored
                }
            };

            CraftableFilters = new List<CraftableFilter>
            {
                new CraftableFilter("CFcraftable")
                {
                    Checked = true,
                    Label = "Craftable",
                    AppliesToDelegate = blueprint => blueprint.CanCraftCount >= 1
                },
                new CraftableFilter("CFnoncraftable")
                {
                    Checked = true,
                    Label = "Non Craftable",
                    AppliesToDelegate = blueprint => blueprint.CanCraftCount == 0
                }
            };

            AllFilters = GradeFilters.Cast<BlueprintFilter>()
                .Union(EngineerFilters)
                .Union(TypeFilters)
                .Union(IgnoredFavoriteFilters)
                .Union(CraftableFilters).ToList();

            LoadSettings();

            InsertMagicFilters();
        }

        private void LoadSettings()
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
            var magicTypeFilters = new TypeFilter("TFmagic")
            {
                Checked = true,
                Magic = true
            };
            TypeFilters.Insert(0, magicTypeFilters);

            var magicEngineerFilters = new EngineerFilter("EFmagic")
            {
                Checked = true,
                Magic = true
            };
            EngineerFilters.Insert(0, magicEngineerFilters);

            var magicGradeFilter = new GradeFilter("GFmagic")
            {
                Checked = true,
                Magic = true
            };
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

        public void ChangeAllFilters(bool @checked)
        {
            foreach (var filter in AllFilters)
            {
                filter.Checked = @checked;
            }
        }

        private List<BlueprintFilter> AllFilters { get; }

        public void Monitor(ICollectionView view, IEnumerable<Entry> entries)
        {
            foreach (var filter in AllFilters)
            {
                filter.PropertyChanged += (o, e) =>
                {
                    view.Refresh();
                    OnPropertyChanged(nameof(EngineerFilters));
                    OnPropertyChanged(nameof(GradeFilters));
                    OnPropertyChanged(nameof(TypeFilters));
                    OnPropertyChanged(nameof(CraftableFilters));
                    OnPropertyChanged(nameof(IgnoredFavoriteFilters));
                };
            }

            foreach (var item in entries)
            {
                item.PropertyChanged += (o, e) => Application.Current.Dispatcher.Invoke(view.Refresh);
            }

            view.Filter = o =>
            {
                var blueprint = (Blueprint)o;

                var ret = GradeFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          EngineerFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          TypeFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          IgnoredFavoriteFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint)) &&
                          CraftableFilters.Where(f => f.Checked).Any(f => f.AppliesTo(blueprint));

                return ret;
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}