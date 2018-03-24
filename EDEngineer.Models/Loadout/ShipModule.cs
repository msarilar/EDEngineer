using System.Collections.Generic;
using EDEngineer.Models.Utils;

namespace EDEngineer.Models.Loadout
{
    public class ShipModule
    {
        public ShipModule(string type, string slot, string blueprintName, int? grade, string engineer, string experimentalEffect, List<ModuleModifier> modifiers)
        {
            var technicalType = type.ToLowerInvariant();
            var technicalSlot = slot.ToLowerInvariant();
            Slot = slot.ToReadable();
            BlueprintName = blueprintName.ToReadable();
            ExperimentalEffect = experimentalEffect.ToReadable();
            Type = type.ToReadable();
            Grade = grade;
            Engineer = engineer;
            Modifiers = modifiers;

            if (technicalSlot.StartsWith("slot"))
            {
                Category = ModuleCategory.OptionalInternal;
            }
            else if (technicalType.StartsWith("int_"))
            {
                Category = ModuleCategory.CoreInternal;
            }
            else if (technicalSlot.StartsWith("tiny"))
            {
                Category = ModuleCategory.Utility;
            }
            else if (technicalType.StartsWith("hpt"))
            {
                Category = ModuleCategory.Hardpoint;
            }
            else
            {
                Category = ModuleCategory.Other;
            }
        }

        public ModuleCategory Category { get; }
        public string Type { get; }
        public string Slot { get; }
        public string BlueprintName { get; }
        public string ExperimentalEffect { get; }
        public int? Grade { get; }
        public string Engineer { get; }
        public List<ModuleModifier> Modifiers { get; }
    }
}