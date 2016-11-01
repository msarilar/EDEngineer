using System;
using EDEngineer.Models;

namespace EDEngineer.Filters
{
    public class IgnoredFavoriteFilter : BlueprintFilter
    {
        public string Label { get; set; }
        public Predicate<Blueprint> AppliesToDelegate { get; set; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return AppliesToDelegate(blueprint);
        }

        public IgnoredFavoriteFilter(string uniqueName) : base(uniqueName)
        {
        }
    }
}