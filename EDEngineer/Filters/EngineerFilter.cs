using System.Linq;
using EDEngineer.Models;

namespace EDEngineer.Filters
{
    public class EngineerFilter : BlueprintFilter
    {
        public string Engineer { get; set; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Engineers.Contains(Engineer);
        }

        public EngineerFilter(string uniqueName) : base(uniqueName)
        {
        }
    }
}