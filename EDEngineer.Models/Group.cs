using System.ComponentModel;

namespace EDEngineer.Models
{
    public enum Group
    {
        [Description("Emission Data")]
        EmissionData,

        [Description("Wake Scans")]
        WakeScans,

        [Description("Shield Data")]
        ShieldData,

        [Description("Encryption Files")]
        EncryptionFiles,

        [Description("Data Archives")]
        DataArchives,

        [Description("Encoded Firmware")]
        EncodedFirmware,

        [Description("Category 1")]
        Category1,

        [Description("Category 2")]
        Category2,

        [Description("Category 3")]
        Category3,

        [Description("Category 4")]
        Category4,

        [Description("Category 5")]
        Category5,

        [Description("Category 6")]
        Category6,

        [Description("Category 7")]
        Category7,

        [Description("Chemical")]
        Chemical,

        [Description("Thermic")]
        Thermic,

        [Description("Heat")]
        Heat,

        [Description("Conductive")]
        Conductive,

        [Description("Mechanical Components")]
        MechanicalComponents,

        [Description("Capacitors")]
        Capacitors,

        [Description("Shielding")]
        Shielding,

        [Description("Composite")]
        Composite,

        [Description("Crystals")]
        Crystals,

        [Description("Alloys")]
        Alloys,

        [Description("Thargoid Ship")]
        ThargoidShip,

        [Description("Thargoid Site")]
        ThargoidSite,

        [Description("Guardian Ruins")]
        GuardianRuins,

        [Description("Guardian Ruins (Active)")]
        GuardianRuinsActive,

        [Description("Commodities")]
        Commodities
    }
}
