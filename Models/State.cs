using System;
using System.Collections.Generic;
using EDEngineer.Utils;

namespace EDEngineer.Models
{
    public class State
    {
        private static readonly Func<KeyValuePair<string, Entry>, KeyValuePair<string, Entry>, int> comparer =
            (a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal);

        private readonly object stateLock = new object();

        public State()
        {
            LoadBaseData();
        }

        public SortedObservableCounter Cargo { get; set; } = new SortedObservableCounter(comparer);
        public SortedObservableCounter Materials { get; set; } = new SortedObservableCounter(comparer);
        public SortedObservableCounter Data { get; set; } = new SortedObservableCounter(comparer);

        public event EventHandler<StateChangeArgs> StateChanged;

        public void LoadBaseData()
        {
            foreach (var name in ItemNameConverter.CommodityNames)
            {
                IncrementCargo(name, 0);
            }

            foreach (var name in ItemNameConverter.MaterialNames)
            {
                IncrementMaterials(name, 0);
            }

            foreach (var name in ItemNameConverter.DataNames)
            {
                IncrementData(name, 0);
            }
        }

        public void IncrementCargo(string name, int change)
        {
            lock (stateLock)
            {
                Cargo.Increment(name, change);
                StateChanged?.Invoke(this,
                    new StateChangeArgs(Kind.Commodity, name, Cargo.ContainsKey(name) ? Cargo[name].Count : 0));
            }
        }

        public void IncrementMaterials(string name, int change)
        {
            lock (stateLock)
            {
                Materials.Increment(name, change);
                StateChanged?.Invoke(this,
                    new StateChangeArgs(Kind.Material, name, Materials.ContainsKey(name) ? Materials[name].Count : 0));
            }
        }

        public void IncrementData(string name, int change)
        {
            lock (stateLock)
            {
                Data.Increment(name, change);
                StateChanged?.Invoke(this,
                    new StateChangeArgs(Kind.Data, name, Data.ContainsKey(name) ? Data[name].Count : 0));
            }
        }
    }
}