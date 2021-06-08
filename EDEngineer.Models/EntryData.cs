using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EDEngineer.Models
{
    public class EntryData
    {
        private Rarity rarity;
        private Group? group;
        private int? maximumCapacity;

        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Rarity Rarity
        {
            get => Kind == Kind.Commodity ? Rarity.Commodity : Kind == Kind.OdysseyIngredient ? Rarity.Odyssey : rarity;
            set => rarity = value;
        }

        public string FormattedName { get; set; }

        [JsonConverter(typeof (StringEnumConverter))]
        public Kind Kind { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Subkind? Subkind { get; set; }

        public string KindStringForGui => Subkind?.ToString("G") ?? Kind.ToString("G");

        public int? ValueCr { get; set; }

        public int? BarterCost { get; set; }

        public int? BarterValue { get; set; }

        public string[] SettlementType { get; set; }

        public string[] BuildingType { get; set; }

        public string[] ContainerType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Group? Group
        {
            get => Kind == Kind.Commodity ? Models.Group.Commodities : group; 
            set => group = value;
        }

        public List<string> OriginDetails { get; set; }

        public int MaximumCapacity
        {
            get => maximumCapacity ?? rarity.MaximumCapacity();
            set => maximumCapacity = value;
        }

        [JsonIgnore]
        public IEnumerable<Origin> Origins => OriginDetails?.Select(detail =>
                originMapping.ContainsKey(detail) ? originMapping[detail] : GuessOrigin(detail))
            .Distinct() ?? new [] { Origin.Unknown };

        [JsonIgnore]
        public bool Unused { get; set; }

        public bool CanBeTraded => Rarity.Rank().HasValue &&
                                   Group.HasValue &&
                                   !Group.In(Models.Group.ThargoidShip, Models.Group.ThargoidSite, Models.Group.GuardianRuins, Models.Group.GuardianRuinsActive, Models.Group.Commodities);

        private static Origin GuessOrigin(string text)
        {
            if (text.Contains("Markets"))
            {
                return Origin.Market;
            }

            if (text.Contains("Needed for"))
            {
                return Origin.NeededForEngineer;
            }

            if (text.Contains("Signal source"))
            {
                return Origin.Scan;
            }

            if (text.Contains("Mining"))
            {
                return Origin.Mining;
            }

            return Origin.Unknown;
        }

        private static readonly Dictionary<string, Origin> originMapping = new Dictionary<string, Origin>
        {
            ["Mining"] = Origin.Mining,
            ["Mining (ice rings)"] = Origin.Mining,

            ["Mission reward"] = Origin.Mission,

            ["Ship salvage (combat ships)"] = Origin.Salvage,
            ["Ship salvage (military & authority ships)"] = Origin.Salvage,
            ["Ship salvage (transport ships)"] = Origin.Salvage,

            ["High wake scanning"] = Origin.Scan,
            ["Signal source"] = Origin.Scan,
            ["Signal source (high security)"] = Origin.Scan,
            ["Signal source (anarchy)"] = Origin.Scan,
            ["Signal source (low security)"] = Origin.Scan,
            ["Signal source (High grade emissions, Federation systems)"] = Origin.Scan,
            ["Signal source (High grade emissions, Empire systems)"] = Origin.Scan,
            ["Signal source (High grade emissions, Civil unrest)"] = Origin.Scan,
            ["Signal source (High grade emissions, War/Civil war)"] = Origin.Scan,
            ["Signal source (High grade emissions, Outbreak)"] = Origin.Scan,
            ["Signal source (High grade emissions, Boom)"] = Origin.Scan,
            ["Ship scanning"] = Origin.Scan,
            ["Ship scanning (combat ships)"] = Origin.Scan,
            ["Ship scanning (transport ships)"] = Origin.Scan,

            ["Surface data point"] = Origin.Surface,
            ["Surface prospecting"] = Origin.Surface,
            ["Surface POI"] = Origin.Surface,

            ["Deep space data beacon"] = Origin.Scan,

            ["Destroyed Unknown Artefact"] = Origin.Unknown,

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
            ["Markets near Eurybia (Extraction/Refinery)"] = Origin.Market,
            ["Markets (Kanwar Gateway, Xiripa)"] = Origin.Market,

            ["Needed for Marco Qwent (25)"] = Origin.NeededForEngineer,
            ["Needed for Ram Tah (50)"] = Origin.NeededForEngineer,
            ["Needed for Professor Palin (25)"] = Origin.NeededForEngineer,
            ["Needed for Tiana Fortune (50)"] = Origin.NeededForEngineer,
            ["Needed for The Sarge (50)"] = Origin.NeededForEngineer,
            ["Needed for Bill Turner (50)"] = Origin.NeededForEngineer,

            ["Ancient/Guardian ruins"] = Origin.AncientGuardianRuins,

            ["Planetary Settlement"] = Origin.PlanetarySettlement
        };
    }
}