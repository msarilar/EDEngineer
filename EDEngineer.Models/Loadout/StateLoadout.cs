using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Operations;
using EDEngineer.Models.Utils;

namespace EDEngineer.Models.Loadout
{
    public class StateLoadout : INotifyPropertyChanged
    {
        private ShipLoadout loadout;

        public ShipLoadout Loadout
        {
            get => loadout;
            set
            {
                loadout = value;
                OnPropertyChanged();
            }
        }

        public void Update(ShipLoadout newLoadout)
        {
            Loadout = newLoadout;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ApplyCraft(EngineerOperation engineerOperation)
        {
            var matchingModule = Loadout.Modules.FirstOrDefault(m => m.TechnicalType == engineerOperation.TechnicalType &&
                                                m.TechnicalSlot == engineerOperation.TechnicalSlot);
            if (matchingModule != null)
            {
                matchingModule.Engineer = engineerOperation.Engineer;
                matchingModule.BlueprintName = engineerOperation.BlueprintName.ToReadable();
                matchingModule.Modifiers = engineerOperation.Modifiers;
                matchingModule.Grade = engineerOperation.Grade;
                matchingModule.ExperimentalEffect = engineerOperation.ExperimentalEffect?.ToReadable();
            }
        }
    }
}