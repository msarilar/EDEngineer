using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using EDEngineer.Properties;
using EDEngineer.Utils;

namespace EDEngineer.Models
{
    public class Entry : INotifyPropertyChanged
    {
        public Entry(string name)
        {
            Name = name;
            if (ItemNameConverter.CommodityNames.Contains(name))
            {
                Kind = Kind.Commodity;
            }
            else if (ItemNameConverter.MaterialNames.Contains(name))
            {
                Kind = Kind.Material;
            }
            else if (ItemNameConverter.DataNames.Contains(name))
            {
                Kind = Kind.Data;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private int count;
        public string Name { get; }
        public Kind Kind { get; }

        public int Count
        {
            get { return count; }
            set
            {
                if (value == count) return;
                var oldValue = count;
                count = value;
                OnPropertyChanged(oldValue, count);
            }
        }

        public Rarity Rarity => ItemNameConverter.Rarities.ContainsKey(Name) ? ItemNameConverter.Rarities[Name] : Rarity.Standard;

        public override string ToString()
        {
            return Name + "(" + Count + ")";
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(int oldValue, int newValue, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<int>(propertyName, oldValue, newValue));
        }
    }
}