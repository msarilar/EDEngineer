using System.Diagnostics;

namespace EDEngineer.Models.Filters
{
    [DebuggerDisplay("{Category}")]
    public class CategoryFilter : BlueprintFilter
    {
        public BlueprintCategory Category { get; }

        public override string Label => Category.ToString("G");

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Category == Category;
        }

        public CategoryFilter(BlueprintCategory category, string uniqueName) : base(uniqueName)
        {
            Category = category;
            Checked = true;
        }
    }
}