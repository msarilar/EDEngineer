using System.Collections.Generic;

namespace EDEngineer.Models.Loadout
{
    public class ShipModule
    {
        public ShipModule(string type, string slot, Blueprint blueprint, string experimentalEffect, List<ModuleModifier> modifiers)
        {
            Type = type;
            Slot = slot;
            Blueprint = blueprint;
            ExperimentalEffect = experimentalEffect;
            Modifiers = modifiers;
        }

        public string Type { get; }
        public string Slot { get; }
        public Blueprint Blueprint { get; }
        public string ExperimentalEffect { get; }
        public List<ModuleModifier> Modifiers { get; }
    }
}