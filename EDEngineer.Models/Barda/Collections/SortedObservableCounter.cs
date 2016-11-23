using System;
using System.Collections.Generic;

namespace EDEngineer.Models.Barda.Collections
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
                throw new ArgumentOutOfRangeException(nameof(key), $"Unknown key : {key}");
            }
        }
    }
}