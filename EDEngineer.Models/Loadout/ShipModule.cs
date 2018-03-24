using System.Collections.Generic;

namespace EDEngineer.Models.Loadout
{
    public class ShipModule
    {
        public ShipModule(string type, string slot, string blueprintName, int? grade, string engineer, string experimentalEffect, List<ModuleModifier> modifiers)
        {
            Type = type.ToLowerInvariant();
            Slot = slot.ToLowerInvariant();
            BlueprintName = blueprintName?.ToLowerInvariant();
            Grade = grade;
            Engineer = engineer;
            ExperimentalEffect = experimentalEffect?.ToLowerInvariant();
            Modifiers = modifiers;

            if (Slot.StartsWith("slot"))
            {
                Category = ModuleCategory.OptionalInternal;
            }
            else if (Type.StartsWith("int_"))
            {
                Category = ModuleCategory.CoreInternal;
            }
            else if (Slot.StartsWith("tiny"))
            {
                Category = ModuleCategory.Utility;
            }
            else if (Type.StartsWith("hpt"))
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