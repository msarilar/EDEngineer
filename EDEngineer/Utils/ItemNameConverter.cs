using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using EDEngineer.Models;

namespace EDEngineer.Utils
{
    public class ItemNameConverter
    {
        private readonly List<EntryData> entryDatas;
        private readonly IReadOnlyDictionary<string, Equipment> equipments;

        public ItemNameConverter(List<EntryData> entryDatas, IReadOnlyDictionary<string, Equipment> equipments)
        {
            this.entryDatas = entryDatas;
            this.equipments = equipments;
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

        public Equipment GetEquipment(JournalEvent @event, string name)
        {
            string key;
            if (@event == JournalEvent.UpgradeWeapon)
            {
                key = name;
            }
            else
            {
                key = name.Split("_".ToCharArray())[0];
            }

            if (!equipments.TryGetValue(key, out var equipment))
            {
                _ = MessageBox.Show($"Unknown equipment {name}. Ctrl+C while this messagebox is on focus then kindly report this on github", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return equipment;
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
                return name != null;
            }

            if (IgnoreList.Contains(key))
            {
                return false;
            }

            var formattedKey = key.ToLowerInvariant();

            var entry = entryDatas.FirstOrDefault(e => e.FormattedName == formattedKey && (e.Kind & kind) == e.Kind) ??
                        entryDatas.FirstOrDefault(e => e.FormattedName.Contains(formattedKey) && (e.Kind & kind) == e.Kind);

            localCache[cacheKey] = name = entry?.Name;

            return entry != null;
        }
    }
}