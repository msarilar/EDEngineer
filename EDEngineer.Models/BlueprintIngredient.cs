namespace EDEngineer.Models
{
    public class BlueprintIngredient
    {
        public Entry Entry { get; }

        public BlueprintIngredient(Entry entry, int size)
        {
            Entry = entry;
            Size = size;
        }

        public int Size { get; }

        public override string ToString()
        {
            return $"{Entry.Data.Kind} : {Entry.Data.Name} ({Entry.Count} / {Size})";
        }
    }
}