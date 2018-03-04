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
            get => shoppingListHighlighted;
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

        protected bool Equals(BlueprintIngredient other)
        {
            return Equals(Entry, other.Entry) && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BlueprintIngredient)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Entry != null ? Entry.GetHashCode() : 0) * 397) ^ Size;
            }
        }
    }
}