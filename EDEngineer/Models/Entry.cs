using System.ComponentModel;
using System.Runtime.CompilerServices;
using EDEngineer.Properties;
using EDEngineer.Utils;

namespace EDEngineer.Models
{
    public class Entry : INotifyPropertyChanged
    {
        private int count;
        public string Name { get; set; }

        public int Count
        {
            get { return count; }
            set
            {
                if (value == count) return;
                count = value;
                OnPropertyChanged();
            }
        }

        public Rarity Rarity => ItemNameConverter.Rarities.ContainsKey(Name) ? ItemNameConverter.Rarities[Name] : Rarity.Standard;

        public override string ToString()
        {
            return Name + "(" + Count + ")";
        }


        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}