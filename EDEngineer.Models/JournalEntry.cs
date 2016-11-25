using EDEngineer.Models.Operations;
using Newtonsoft.Json;
using NodaTime;

namespace EDEngineer.Models
{
    public class JournalEntry
    {
        [JsonIgnore]
        public string OriginalJson { get; set; }

        [JsonIgnore]
        public Instant Timestamp { get; set; }

        [JsonProperty("Timestamp")]
        public string TimestampString => Timestamp.ToString();

        public JournalOperation JournalOperation { get; set; }

        [JsonIgnore]
        public bool Relevant => JournalOperation != null;

        protected bool Equals(JournalEntry other)
        {
            return string.Equals(OriginalJson, other.OriginalJson);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((JournalEntry) obj);
        }

        public override int GetHashCode()
        {
            return OriginalJson.GetHashCode();
        }
    }
}