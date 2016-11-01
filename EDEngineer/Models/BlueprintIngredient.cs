using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EDEngineer.Models
{
    public class BlueprintIngredient : INotifyPropertyChanged
    {
        private int current;
        public string Name { get; set; }
        public int Size { get; set; }

        public int Current
        {
            get { return current; }
            set
            {
                if (value == current) return;
                current = value;
                OnPropertyChanged();
            }
        }

        public Kind Kind { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return $"{Kind} : {Name} ({Current} / {Size})";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}