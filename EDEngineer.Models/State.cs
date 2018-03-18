using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Utils.Collections;

namespace EDEngineer.Models
{
    using Comparer = Func<KeyValuePair<string, Entry>, KeyValuePair<string, Entry>, int>;

    public class State : IState, INotifyPropertyChanged
    {
        public event EventHandler<Tuple<BlueprintCategory, string, List<BlueprintIngredient>>> BlueprintCrafted;
        public const string NAME_COMPARER = "Name";
        public const string COUNT_COMPARER = "Count";
        public const string RARITY_COMPARER = "Rarity";

        public LinkedList<JournalEntry> Operations { get; } = new LinkedList<JournalEntry>();

        private readonly List<EntryData> entryDatas;

        private readonly object stateLock = new object();
        public List<Blueprint> Blueprints { get; set; }

        private readonly IReadOnlyDictionary<string, Comparer> comparers;

        public History History { get; }

        public string System
        {
            get => system;
            set
            {
                if (value == system)
                {
                    return;
                }

                History.System = value;
                system = value;
                OnPropertyChanged();
            }
        }

        public State(List<EntryData> entryDatas, ILanguage languages, string comparer)
        {
            History = new History();
            System = "Sol";
            comparers = new Dictionary<string, Comparer>
            {
                [NAME_COMPARER] = (a, b) => string.Compare(languages.Translate(a.Key), languages.Translate(b.Key), StringComparison.InvariantCultureIgnoreCase),
                [COUNT_COMPARER] = (a, b) => b.Value.Count.CompareTo(a.Value.Count),
                [RARITY_COMPARER] = (a, b) => ((int) a.Value.Data.Rarity).CompareTo((int) b.Value.Data.Rarity)
            };

            Cargo = new SortedObservableCounter(comparers[comparer]);
            languages.PropertyChanged += (o, e) => Cargo.RefreshSort();
            
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

            Cargo.RefreshSort(comparer);
        }

        public SortedObservableCounter Cargo { get; }

        public void LoadBaseData()
        {
            lock (stateLock)
            {
                var toAdd = entryDatas.Where(e => !Cargo.ContainsKey(e.Name));
                foreach (var item in toAdd)
                {
                    Cargo.Add(new KeyValuePair<string, Entry>(item.Name, new Entry(item)));
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
        private string system;

        public void IncrementCargo(string name, int change)
        {
            lock (stateLock)
            {
                if (!Cargo.ContainsKey(name))
                {
                    Cargo[name] = new Entry(new EntryData
                    {
                        FormattedName = name,
                        Kind = Kind.Unknown,
                        Name = name,
                        Unused = true
                    });
                }
                Cargo.Increment(name, change);
            }

            if (!loading)
            {
                Cargo.SortInPlace();
            }

            OnPropertyChanged(name);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnBlueprintCrafted(BlueprintCategory category, string technicalName, List<BlueprintIngredient> ingredientsConsumed)
        {
            BlueprintCrafted?.Invoke(this, Tuple.Create(category, technicalName, ingredientsConsumed));
        }
    }
}