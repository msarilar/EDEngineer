using EDEngineer.Models.Utils;
using System;

namespace EDEngineer.Models.Loadout
{
    public class ModuleModifier
    {
        public ModuleModifier(string label, float value, float originalValue, bool lessIsGood)
        {
            Label = label.ToReadable();
            Value = value;
            OriginalValue = originalValue;
            LessIsGood = lessIsGood;

            Change = OriginalValue == 0 ? (double?) null : Math.Round((Value - OriginalValue) / Math.Abs(OriginalValue) * 100);
        }

        public string Label { get; }
        public float Value { get; }
        public float OriginalValue { get; }
        public bool LessIsGood { get; }
        public double? Change { get; }
    }
}