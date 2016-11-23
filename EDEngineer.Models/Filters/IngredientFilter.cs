using System.Linq;

namespace EDEngineer.Models.Filters
{
    public class IngredientFilter : BlueprintFilter
    {
        public Entry Entry { get; }

        public IngredientFilter(Entry entry, string uniqueName) : base(uniqueName)
        {
            Entry = entry;
        }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Ingredients.Any(i => i.Entry == Entry);
        }
    }
}