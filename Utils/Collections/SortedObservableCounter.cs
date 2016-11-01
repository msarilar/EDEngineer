using System;
using System.Collections.Generic;
using EDEngineer.Models;

namespace EDEngineer.Utils.Collections
{
    public class SortedObservableCounter : SortedObservableDictionary<string, Entry>
    {
        public SortedObservableCounter(Func<KeyValuePair<string, Entry>, KeyValuePair<string, Entry>, int> comparer)
            : base(comparer)
        {
        }

        public void Increment(string key, int value)
        {
            if (ContainsKey(key))
            {
                this[key].Count += value;
            }
            else
            {
                this[key] = new Entry
                {
                    Count = value,
                    Name = key
                };
            }
        }
    }
}