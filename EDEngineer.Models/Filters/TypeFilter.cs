namespace EDEngineer.Models.Filters
{
    public class TypeFilter : BlueprintFilter
    {
        public string Type { get; }

        public override string Label => Type;

        public override bool AppliesTo(Blueprint blueprint)
        {
            return Type == "@Synthesis" && blueprint.Category == BlueprintCategory.Synthesis ||
                   Type == "@Technology" && blueprint.Category == BlueprintCategory.Technology ||
                   blueprint.Type == Type;
        }

        public TypeFilter(string type, string uniqueName) : base(uniqueName)
        {
            Type = type;
            Checked = true;
        }

        public static TypeFilter MagicFilter => new TypeFilter(null, "TFmagic")
        {
            Checked = true,
            Magic = true
        };
    }
}