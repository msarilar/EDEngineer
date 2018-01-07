using System.ComponentModel;

namespace EDEngineer.Models
{
    public enum Kind
    {
        [Description("Material")]
        Material,

        [Description("Data")]
        Data,

        [Description("Commodity")]
        Commodity
    }
}