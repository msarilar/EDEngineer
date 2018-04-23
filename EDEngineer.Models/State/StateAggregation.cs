using System.Collections.Generic;
using EDEngineer.Models.Loadout;
using NodaTime;

namespace EDEngineer.Models.State
{
    public class StateAggregation
    {
        public Dictionary<string, int> Cargo { get; set; }
        public Dictionary<string, Dictionary<string, int>> History { get; set; }
        public string System { get; set; }
        public ShipLoadout Loadout { get; set; }
        public Instant LastTimestamp { get; set; }
    }
}