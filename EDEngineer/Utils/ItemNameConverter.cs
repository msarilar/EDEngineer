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

        private readonly Dictionary<string, string> localCache = new Dictionary<string, string>();

        private HashSet<string> IgnoreList { get; } = new HashSet<string>()
        {
            "polymers",
            "scrap"
        };

        public string GetOrCreate(string key)
        {
            string value;
            if (!TryGet(key, out value))
            {
                value = key;
            }

            return value;
        }

        public bool TryGet(string key, out string name)
        {
            if (key == null)
            {
                name = null;
                return false;
            }

            if (localCache.TryGetValue(key, out name))
            {
                return true;
            }

            if (IgnoreList.Contains(key))
            {
                return false;
            }

            var formattedKey = key.ToLowerInvariant();

            var entry = entryDatas.FirstOrDefault(e => e.FormattedName == formattedKey) ??
                        entryDatas.FirstOrDefault(e => e.FormattedName.Contains(formattedKey));

            if (entry != null)
            {
                localCache[key] = name = entry.Name;
                return true;
            }

            return false;
        }
    }
}