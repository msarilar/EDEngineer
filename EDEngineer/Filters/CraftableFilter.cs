using System;
using EDEngineer.Models;

namespace EDEngineer.Filters
{
    public class CraftableFilter : BlueprintFilter
    {
        public string Label { get; set; }
        public Predicate<Blueprint> AppliesToDelegate { get; set; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return AppliesToDelegate(blueprint);
        }

        public CraftableFilter(string uniqueName) : base(uniqueName)
        {
        }
    }
}