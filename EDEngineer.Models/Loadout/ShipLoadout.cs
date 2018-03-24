using System.Collections.Generic;

namespace EDEngineer.Models.Loadout
{
    public class StateLoadout
    {

    }
    public class ModuleModifier
    {
        public ModuleModifier(string label, float value, float originalValue, bool lessIsGood)
        {
            Label = label;
            Value = value;
            OriginalValue = originalValue;
            LessIsGood = lessIsGood;
        }

        public string Label { get; }
        public float Value { get; }
        public float OriginalValue { get; }
        public bool LessIsGood { get; }
    }

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

    public class ShipLoadout
    {
        public ShipLoadout(string ship, string shipName, string shipIdent, int shipValue, int modulesValue, int rebuy, List<ShipModule> modules)
        {
            Ship = ship;
            ShipName = shipName;
            ShipIdent = shipIdent;
            ShipValue = shipValue;
            ModulesValue = modulesValue;
            Rebuy = rebuy;
            Modules = modules;
        }

        public string Ship { get; }
        public string ShipName { get; }
        public string ShipIdent { get; }
        public int ShipValue { get; }
        public int ModulesValue { get; }
        public int Rebuy { get; }
        public List<ShipModule> Modules { get; }
    }
}