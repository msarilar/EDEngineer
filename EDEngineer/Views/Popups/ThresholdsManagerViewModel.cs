using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using EDEngineer.Localization;
using EDEngineer.Models;
using EDEngineer.Models.Filters;
using EDEngineer.Models.Utils.Collections;
using EDEngineer.Models.Utils.Json;
using EDEngineer.Utils;

namespace EDEngineer.Views.Popups
{
    public class ThresholdsManagerViewModel : INotifyPropertyChanged
    {
        public Languages Languages { get; }

        public ISimpleDictionary<string, Entry> Thresholds { get; }

        public List<RarityFilter> RarityFilters { get; }

        public List<KindFilter> KindFilters { get; }

        public string SearchText
        {
            get { return searchText; }
            set
            {
                if (searchText == value)
                {
                    return;
                }
                searchText = value;
                OnPropertyChanged();
            }
        }

        private string searchText;

        public ThresholdsManagerViewModel(Languages languages, ISimpleDictionary<string, Entry> thresholds)
        {
            Languages = languages;
            Thresholds = thresholds;

            var list = thresholds.Values.ToList();

            var kinds = list.Select(e => e.Data.Kind).Distinct().Where(k => k != Kind.Commodity).ToList();
            KindFilters = kinds.Select(kind => new KindFilter(kind.GetLabel(), kind) { Checked = true }).ToList();

            var rarities = list.Select(e => e.Data.Rarity).Distinct().Where(r => r.HasValue).ToList();
            RarityFilters = rarities.Select(rarity => new RarityFilter(rarity.GetLabel(), rarity.Value) { Checked = true }).ToList();

            var allFilters = RarityFilters.Cast<Filter<EntryData>>().Union(KindFilters).ToList();

            var view = CollectionViewSource.GetDefaultView(Thresholds);
            filteredCache = ComputeFilteredCache();
            view.Filter += o =>
            {
                var item = (KeyValuePair<string, Entry>)o;
                return filteredCache.Contains(item.Value);
            };

            foreach (var filter in allFilters)
            {
                filter.PropertyChanged += (o, e) =>
                                          {
                                              filteredCache = ComputeFilteredCache();
                                              view.Refresh();
                                          };
            }

            PropertyChanged += (o, e) =>
            {
                filteredCache = ComputeFilteredCache();
                view.Refresh();
            };
        }

        private HashSet<Entry> filteredCache; 
        private HashSet<Entry> ComputeFilteredCache()
        {
            return Thresholds.Values.Where(item => item.Data.Kind != Kind.Commodity &&
                                            (string.IsNullOrEmpty(SearchText) || Languages.Translate(item.Data.Name).IndexOf(SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0) &&
                                            KindFilters.Where(f => f.Checked).Any(f => f.AppliesTo(item.Data)) &&
                                            RarityFilters.Where(f => f.Checked).Any(f => f.AppliesTo(item.Data)))
                      .ToHashSet();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
