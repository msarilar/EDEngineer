using System;
using System.Collections.Generic;
using System.Linq;

namespace EDEngineer.Models.Utils.Collections
{
    public class SortedObservableDictionary<TKey, TValue> : SortedObservableCollection<KeyValuePair<TKey, TValue>>, ISimpleDictionary<TKey, TValue>
    {
        public SortedObservableDictionary(Func<KeyValuePair<TKey, TValue>, KeyValuePair<TKey, TValue>, int> comparer)
            : base(comparer)
        {
        }

        public TValue this[TKey key]
        {
            get { return this.First(i => Equals(i.Key, key)).Value; }
            set
            {
                var index = IndexOf(this.FirstOrDefault(i => Equals(i.Key, key)));

                if (index == -1)
                {
                    Add(new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    this[index] = new KeyValuePair<TKey, TValue>(key, value);
                }
            }
        }

        public IEnumerable<TKey> Keys => this.Select(kv => kv.Key);
        public IEnumerable<TValue> Values => this.Select(kv => kv.Value);

        public bool ContainsKey(TKey key)
        {
            return this.Any(i => Equals(i.Key, key));
        }
    }
}