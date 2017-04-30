using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace EDEngineer.Models
{
    public class BlueprintIngredient : INotifyPropertyChanged
    {
        private bool shoppingListHighlighted;
        public Entry Entry { get; }

        public BlueprintIngredient(Entry entry, int size)
        {
            Entry = entry;
            Size = size;
        }

        public int Size { get; }

        [JsonIgnore]
        public bool ShoppingListHighlighted
        {
            get { return shoppingListHighlighted; }
            set
            {
                if (value == shoppingListHighlighted)
                    return;
                shoppingListHighlighted = value;
                OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            return $"{Entry.Data.Kind} : {Entry.Data.Name} ({Entry.Count} / {Size})";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}