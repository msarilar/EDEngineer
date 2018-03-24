using EDEngineer.Models.Utils;
using System;

namespace EDEngineer.Models.Loadout
{
    public class ModuleModifier
    {
        public ModuleModifier(string label, float value, float? originalValue, bool lessIsGood)
        {
            Label = label.ToReadable();
            Value = value;
            OriginalValue = originalValue == 0 ? null : originalValue;
            LessIsGood = lessIsGood;

            Change = OriginalValue == 0 || OriginalValue == null ? (double?) null : Math.Round((Value - OriginalValue.Value) / Math.Abs(OriginalValue.Value) * 100);
        }

        public string Label { get; }
        public float Value { get; }
        public float? OriginalValue { get; }
        public bool LessIsGood { get; }
        public double? Change { get; }
    }
}