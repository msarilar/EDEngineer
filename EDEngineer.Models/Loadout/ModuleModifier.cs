namespace EDEngineer.Models.Loadout
{
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
}