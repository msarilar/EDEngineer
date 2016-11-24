using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EDEngineer.Models.Operations
{
    public abstract class JournalOperation
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public JournalEvent JournalEvent { get; set; }

        public abstract void Mutate(State state);
    }
}