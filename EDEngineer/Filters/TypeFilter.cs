using EDEngineer.Models;

namespace EDEngineer.Filters
{
    public class TypeFilter : BlueprintFilter
    {
        public string Type { get; set; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Type == Type;
        }

        public TypeFilter(string uniqueName) : base(uniqueName)
        {
        }
    }
}