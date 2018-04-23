using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Utils.Collections;

namespace EDEngineer.Models.State
{
    using Comparer = Func<KeyValuePair<string, Entry>, KeyValuePair<string, Entry>, int>;

    public class StateCargo : INotifyPropertyChanged
    {
        public const string NAME_COMPARER = "Name";
        public const string COUNT_COMPARER = "Count";
        public const string RARITY_COMPARER = "Rarity";

        private readonly List<EntryData> entryDatas;

        private readonly object stateLock = new object();

        private readonly IReadOnlyDictionary<string, Comparer> comparers;

        public StateCargo(List<EntryData> entryDatas, ILanguage languages, string comparer)
        {
            comparers = new Dictionary<string, Comparer>
            {
                [NAME_COMPARER] = (a, b) => string.Compare(languages.Translate(a.Key), languages.Translate(b.Key), StringComparison.InvariantCultureIgnoreCase),
                [COUNT_COMPARER] = (a, b) => b.Value.Count.CompareTo(a.Value.Count),
                [RARITY_COMPARER] = (a, b) => ((int) a.Value.Data.Rarity).CompareTo((int) b.Value.Data.Rarity)
            };

            Ingredients = new SortedObservableCounter(comparers[comparer]);
            languages.PropertyChanged += (o, e) => Ingredients.RefreshSort();
            
            this.entryDatas = entryDatas;
            LoadBaseData();
        }

        public void ChangeComparer(string newComparer, Func<Entry, Entry, int> priorityComparison = null)
        {
            Comparer comparer;
            if (priorityComparison != null)
            {
                comparer = (a, b) =>
                           {
                               var priority = priorityComparison(a.Value, b.Value);
                               if (priority == 0)
                               {
                                   return comparers[newComparer](a, b);
                               }

                               return priority;
                           };
            }
            else
            {
                comparer = comparers[newComparer];
            }

            Ingredients.RefreshSort(comparer);
        }

        public SortedObservableCounter Ingredients { get; }

        public void LoadBaseData()
        {
            lock (stateLock)
            {
                var toAdd = entryDatas.Where(e => !Ingredients.ContainsKey(e.Name));
                foreach (var item in toAdd)
                {
                    Ingredients.Add(new KeyValuePair<string, Entry>(item.Name, new Entry(item)));
                }
            }
        }

        public void InitLoad()
        {
            loading = true;
        }

        public void CompleteLoad()
        {
            loading = false;
        }

        private bool loading;

        public void IncrementCargo(string name, int change)
        {
            lock (stateLock)
            {
                if (!Ingredients.ContainsKey(name))
                {
                    Ingredients[name] = new Entry(new EntryData
                    {
                        FormattedName = name,
                        Kind = Kind.Unknown,
                        Name = name,
                        Unused = true
                    });
                }
                Ingredients.Increment(name, change);
            }

            if (!loading)
            {
                Ingredients.SortInPlace();
            }

            OnPropertyChanged(name);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}