using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models;

namespace EDEngineer.Utils
{
    public class ItemNameConverter
    {
        private readonly List<EntryData> entryDatas;

        public ItemNameConverter(List<EntryData> entryDatas)
        {
            this.entryDatas = entryDatas;
        }

        public EntryData this[string key] => entryDatas.First(e => e.Name == key);

        private readonly Dictionary<Tuple<Kind, string>, string> localCache = new Dictionary<Tuple<Kind, string>, string>();

        private HashSet<string> IgnoreList { get; } = new HashSet<string>
        {
            "polymers",
            "scrap",
            "wreckagecomponents"
        };

        public string GetOrCreate(Kind kind, string key)
        {
            if (!TryGet(kind, key, out var value))
            {
                value = key;
            }

            return value;
        }

        public bool TryGet(Kind kind, string key, out string name)
        {
            if (key == null)
            {
                name = null;
                return false;
            }

            var cacheKey = Tuple.Create(kind, key);
            if (localCache.TryGetValue(cacheKey, out name))
            {
                return true;
            }

            if (IgnoreList.Contains(key))
            {
                return false;
            }

            var formattedKey = key.ToLowerInvariant();

            var entry = entryDatas.FirstOrDefault(e => e.FormattedName == formattedKey && (e.Kind & kind) == e.Kind) ??
                        entryDatas.FirstOrDefault(e => e.FormattedName.Contains(formattedKey) && (e.Kind & kind) == e.Kind);

            if (entry != null)
            {
                localCache[cacheKey] = name = entry.Name;
                return true;
            }

            return false;
        }
    }
}