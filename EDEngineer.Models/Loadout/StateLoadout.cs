using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

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
            if(Loadout == null || newLoadout.Modules.Sum(m => m.Modifiers.Count) > Loadout.Modules.Sum(m => m.Modifiers.Count))
            Loadout = newLoadout;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}