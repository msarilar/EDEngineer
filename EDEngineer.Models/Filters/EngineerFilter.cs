using System.Diagnostics;
using System.Linq;
using EDEngineer.Models.Utils;

namespace EDEngineer.Models.Filters
{
    [DebuggerDisplay("{Label}")]
    public class EngineerFilter : BlueprintFilter
    {
        public string Engineer { get; }

        public override string Label => Engineer;

        public override bool AppliesTo(Blueprint blueprint)
        {
            return blueprint.Category.In(BlueprintCategory.Synthesis, BlueprintCategory.Technology, BlueprintCategory.Merchant) || blueprint.Engineers.Contains(Engineer);
        }

        public EngineerFilter(string engineer, string uniqueName) : base(uniqueName)
        {
            Engineer = engineer;
            Checked = true;
        }

        public static EngineerFilter MagicFilter => new EngineerFilter(null, "EFmagic")
        {
            Checked = true,
            Magic = true
        };
    }
}