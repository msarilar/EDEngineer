using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Utils;
using EDEngineer.Utils.Collections;

namespace EDEngineer.Models
{
    public class State
    {
        private readonly List<EntryData> entryDatas;

        private static readonly Func<KeyValuePair<string, Entry>, KeyValuePair<string, Entry>, int> comparer =
            (a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal);

        private readonly object stateLock = new object();

        public State(List<EntryData> entryDatas)
        {
            this.entryDatas = entryDatas;
            LoadBaseData();
        }

        public SortedObservableCounter Cargo { get; set; } = new SortedObservableCounter(comparer);

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

        public void IncrementCargo(string name, int change)
        {
            lock (stateLock)
            {
                if (Cargo.ContainsKey(name))
                {
                    Cargo.Add(new KeyValuePair<string, Entry>(name, new Entry(entryDatas.First(e => e.Name == name))));
                }

                Cargo.Increment(name, change);
            }
        }
    }
}