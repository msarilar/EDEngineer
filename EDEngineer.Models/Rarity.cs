using System.ComponentModel;

namespace EDEngineer.Models
{
    public enum Rarity
    {
        [Description("None")]
        None,

        [Description("Very Common")]
        VeryCommon,

        [Description("Common")]
        Common,

        [Description("Standard")]
        Standard,

        [Description("Rare")]
        Rare,

        [Description("Very Rare")]
        VeryRare,

        [Description("Commodity")]
        Commodity,

        [Description("Odyssey")]
        Odyssey
    }
}