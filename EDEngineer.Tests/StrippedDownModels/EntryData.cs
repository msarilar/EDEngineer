using System.Collections.Generic;

namespace EDEngineer.Tests.StrippedDownModels
{
    public class EntryData
    {
        public string Name { get; set; }

        public string Rarity { get; set; }

        public string FormattedName { get; set; }
        public string Kind { get; set; }

        public string Subkind { get; set; }
        public List<string> OriginDetails { get; set; }

        public string Group { get; set; }
        public int? MaximumCapacity { get; set; }

        public int? ValueCr { get; set; }

        public int? BarterCost { get; set; }

        public int? BarterValue { get; set; }

        public string[] SettlementType { get; set; }

        public string[] BuildingType { get; set; }

        public string[] ContainerType { get; set; }
    }
}