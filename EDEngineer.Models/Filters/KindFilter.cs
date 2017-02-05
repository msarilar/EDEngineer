namespace EDEngineer.Models.Filters
{
    public class KindFilter : Filter<EntryData>
    {
        public KindFilter(string kindLabel, Kind kind) : base(kindLabel)
        {
            Label = kindLabel;
            Kind = kind;
        }

        public Kind Kind { get; }
        public override string Label { get; }

        public override bool AppliesTo(EntryData data)
        {
            return data.Kind == Kind;
        }
    }
}