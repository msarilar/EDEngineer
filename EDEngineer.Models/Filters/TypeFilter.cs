namespace EDEngineer.Models.Filters
{
    public class TypeFilter : BlueprintFilter
    {
        public string Type { get; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Type == Type;
        }

        public TypeFilter(string type, string uniqueName) : base(uniqueName)
        {
            Type = type;
        }

        public static TypeFilter MagicFilter => new TypeFilter(null, "TFmagic")
        {
            Checked = true,
            Magic = true
        };
    }
}