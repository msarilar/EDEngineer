namespace EDEngineer.Models.Filters
{
    public class GradeFilter : BlueprintFilter
    {
        public int Grade { get; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Grade == Grade;
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