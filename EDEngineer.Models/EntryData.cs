using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EDEngineer.Models
{
    public class EntryData
    {
        public string Name { get; set; }

        [JsonConverter(typeof (StringEnumConverter))]
        public Rarity? Rarity { get; set; }

        public string FormattedName { get; set; }

        [JsonConverter(typeof (StringEnumConverter))]
        public Kind Kind { get; set; }

        public List<string> OriginDetails { get; set; }

        [JsonIgnore]
        public IEnumerable<Origin> Origins => OriginDetails?.Select(detail => originMapping[detail]).Distinct() ?? new [] { Origin.Unknown };

        [JsonIgnore]
        public bool Unused { get; set; }

        private static readonly Dictionary<string, Origin> originMapping = new Dictionary<string, Origin>()
        {
            ["Surface prospecting"] = Origin.Surface,
            ["Mining"] = Origin.Mining,
            ["Mission reward"] = Origin.Mission,
            ["Mining (ice rings)"] = Origin.Mining,
            ["Ship salvage (transport ships)"] = Origin.Salvage,
            ["Signal source (high security)"] = Origin.Scan,
            ["Surface POI"] = Origin.Surface,
            ["Signal source"] = Origin.Scan,
            ["Ship salvage (combat ships)"] = Origin.Salvage,
            ["Signal source (anarchy)"] = Origin.Scan,
            ["Signal source (low security)"] = Origin.Scan,
            ["Signal source (High grade emissions)"] = Origin.Scan,
            ["Ship salvage (military & authority ships)"] = Origin.Salvage,
            ["Destroyed Unknown Artefact"] = Origin.Unknown,
            ["Ship scanning (combat ships)"] = Origin.Scan,
            ["Deep space data beacon"] = Origin.Scan,
            ["Ship scanning (transport ships)"] = Origin.Scan,
            ["High wake scanning"] = Origin.Scan,
            ["Surface data point"] = Origin.Surface,
            ["Ship scanning"] = Origin.Scan,
            ["Markets"] = Origin.Market,
            ["Markets near Akhenaten (High Tech/Refinery)"] = Origin.Market,
            ["Markets near Stafkarl (Industrial/Refinery)"] = Origin.Market,
            ["Markets near Run (Industrial/Refinery)"] = Origin.Market,
            ["Markets near Lei Jing (Industrial/Refinery)"] = Origin.Market,
            ["Markets near Myrbat (Industrial/Refinery)"] = Origin.Market,
            ["Markets near 70 Tauri (Industrial/Extraction)"] = Origin.Market,
            ["Markets near Leesti (Industrial/Refinery)"] = Origin.Market,
            ["Markets near Lakota (Industrial/Extraction)"] = Origin.Market,
            ["Markets near Cilbien Zu (Industrial/Extraction)"] = Origin.Market,
            ["Markets near Heget (Industrial/Extraction)"] = Origin.Market,
            ["Markets near Eurybia (Extraction/Refinery)"] = Origin.Market
        };
    }
}