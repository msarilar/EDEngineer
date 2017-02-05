namespace EDEngineer.Models.Filters
{
    public class RarityFilter : Filter<EntryData>
    {
        public RarityFilter(string rarityLabel, Rarity rarity) : base(rarityLabel)
        {
            Label = rarityLabel;
            Rarity = rarity;
        }

        public Rarity Rarity { get; }
        public override string Label { get; }

        public override bool AppliesTo(EntryData data)
        {
            return data.Rarity == Rarity;
        }
    }
}