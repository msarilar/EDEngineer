using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EDEngineer.Models
{
    public class EntryData
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Rarity Rarity { get; set; }

        public string FormattedName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Kind Kind { get; set; }
    }
}