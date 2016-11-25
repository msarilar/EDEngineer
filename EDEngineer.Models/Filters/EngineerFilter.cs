using System.Linq;

namespace EDEngineer.Models.Filters
{
    public class EngineerFilter : BlueprintFilter
    {
        public string Engineer { get; }

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Engineers.Contains(Engineer);
        }

        public EngineerFilter(string engineer, string uniqueName) : base(uniqueName)
        {
            Engineer = engineer;
        }

        public static EngineerFilter MagicFilter => new EngineerFilter(null, "EFmagic")
        {
            Checked = true,
            Magic = true
        };
    }
}