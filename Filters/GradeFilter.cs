using EDEngineer.Models;

namespace EDEngineer.Filters
{
    public class GradeFilter : BlueprintFilter
    {
        public int Grade { get; set; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Grade == Grade;
        }

        public GradeFilter(string uniqueName) : base(uniqueName)
        {
        }
    }
}