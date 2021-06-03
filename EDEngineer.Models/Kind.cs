using System;
using System.ComponentModel;

namespace EDEngineer.Models
{
    [Flags]
    public enum Kind
    {
        [Description("Material")]
        Material = 1,

        [Description("Data")]
        Data = 2,

        [Description("Commodity")]
        Commodity = 4,

        [Description("Unknown")]
        Unknown = 8,

        [Description("Odyssey Ingredient")]
        OdysseyIngredient = 16
    }
}