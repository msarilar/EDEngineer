using System;
using System.Collections.Generic;
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
                foreach (var item in entryDatas)
                {
                    Cargo.Add(new KeyValuePair<string, Entry>(item.Name, new Entry(item)));
                }
            }
        }

        public void IncrementCargo(string name, int change)
        {
            lock (stateLock)
            {
                Cargo.Increment(name, change);
            }
        }
    }
}