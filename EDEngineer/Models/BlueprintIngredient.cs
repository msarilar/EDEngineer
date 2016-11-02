using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EDEngineer.Models
{
    public class BlueprintIngredient : INotifyPropertyChanged
    {
        public Entry Entry { get; }

        public BlueprintIngredient(Entry entry, int size)
        {
            Entry = entry;
            Size = size;
        }

        public int Size { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return $"{Entry.Kind} : {Entry.Name} ({Entry.Count} / {Size})";
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}