using System.ComponentModel;

namespace EDEngineer.Models.Loadout
{
    public enum ModuleCategory
    {
        [Description("Hardpoints")]
        Hardpoint,
        [Description("Utilities")]
        Utility,
        [Description("Core Internal")]
        CoreInternal,
        [Description("Optional Internal")]
        OptionalInternal,
        [Description("Others/Livery")]
        Other
    }
}