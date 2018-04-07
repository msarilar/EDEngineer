using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Utils;

namespace EDEngineer.Models.Loadout
{
    public class ShipModule : INotifyPropertyChanged
    {
        private string blueprintName;
        private string experimentalEffect;
        private int? grade;
        private string engineer;
        private List<ModuleModifier> modifiers;
        public string TechnicalType { get; }
        public string TechnicalSlot { get; }

        public ShipModule(string type, string slot, string blueprintName, int? grade, string engineer,
                          string experimentalEffect, List<ModuleModifier> modifiers)
        {
            TechnicalType = type.ToLowerInvariant();
            TechnicalSlot = slot.ToLowerInvariant();

            Slot = slot.ToReadable().Replace("Slot", "Slot ").Trim();
            BlueprintName = blueprintName.ToReadable();
            ExperimentalEffect = experimentalEffect.ToReadable();
            Type = type.ToReadable().Replace("Hpt", "").Replace("Int", "").Replace("hpt", "").Replace("int", "").Trim();
            Grade = grade;
            Engineer = engineer;
            Modifiers = modifiers;

            if (TechnicalSlot.StartsWith("slot"))
            {
                Category = ModuleCategory.OptionalInternal;
            }
            else if (TechnicalType.StartsWith("int_"))
            {
                Category = ModuleCategory.CoreInternal;
            }
            else if (TechnicalSlot.StartsWith("tiny"))
            {
                Category = ModuleCategory.Utility;
            }
            else if (TechnicalType.StartsWith("hpt"))
            {
                Category = ModuleCategory.Hardpoint;
            }
            else if (TechnicalType.Contains("armour"))
            {
                Category = ModuleCategory.CoreInternal;
            }
            else
            {
                Category = ModuleCategory.Other;
            }
        }

        public ModuleCategory Category { get; }
        public string Type { get; }
        public string Slot { get; }

        public string BlueprintName
        {
            get => blueprintName;
            set
            {
                blueprintName = value;
                OnPropertyChanged();
            }
        }

        public string ExperimentalEffect
        {
            get => experimentalEffect;
            set
            {
                experimentalEffect = value;
                OnPropertyChanged();
            }
        }

        public int? Grade
        {
            get => grade;
            set
            {
                grade = value;
                OnPropertyChanged();
            }
        }

        public string Engineer
        {
            get => engineer;
            set
            {
                engineer = value;
                OnPropertyChanged();
            }
        }

        public List<ModuleModifier> Modifiers
        {
            get => modifiers;
            set
            {
                modifiers = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}