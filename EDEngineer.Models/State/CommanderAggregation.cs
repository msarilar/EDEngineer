using System;
using System.Collections.Generic;

namespace EDEngineer.Models.State
{
    public class CommanderAggregation
    {
        public Dictionary<string, StateAggregation> Aggregations { get; set; }
    }
}