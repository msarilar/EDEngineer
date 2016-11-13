using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.Localization;
using EDEngineer.Utils.Collections;

namespace EDEngineer.Models
{
    public class State
    {
        private readonly List<EntryData> entryDatas;

        private readonly object stateLock = new object();

        public State(List<EntryData> entryDatas, Languages languages)
        {
            Cargo = new SortedObservableCounter((a, b) => string.Compare(languages.Translate(a.Key), languages.Translate(b.Key), StringComparison.InvariantCultureIgnoreCase));
            languages.PropertyChanged += (o, e) => Cargo.RefreshSort();
            
            this.entryDatas = entryDatas;
            LoadBaseData();
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

        public void IncrementCargo(string name, int change)
        {
            lock (stateLock)
            {
                Cargo.Increment(name, change);
            }
        }
    }
}