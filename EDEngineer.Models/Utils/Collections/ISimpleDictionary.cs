using System.Collections.Generic;

namespace EDEngineer.Models.Utils.Collections
{
    public interface ISimpleDictionary<TKey, TValue>
    {
        TValue this[TKey key]
        {
            get;
            set;
        }

        IEnumerable<TKey> Keys { get; } 
        IEnumerable<TValue> Values { get; }

        bool ContainsKey(TKey key);
    }
}