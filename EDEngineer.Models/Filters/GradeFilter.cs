using System.Diagnostics;

namespace EDEngineer.Models.Filters
{
    [DebuggerDisplay("{Label}")]
    public class GradeFilter : BlueprintFilter
    {
        public int Grade { get; }

        public override string Label => Grade.ToString();

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Grade == null || blueprint.Grade == Grade;
        }

        public GradeFilter(int grade, string uniqueName) : base(uniqueName)
        {
            Grade = grade;
        }

        public static GradeFilter MagicFilter => new GradeFilter(-1, "GFmagic")
        {
            Checked = true,
            Magic = true
        };
    }
}