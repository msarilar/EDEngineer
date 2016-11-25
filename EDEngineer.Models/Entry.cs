using System.ComponentModel;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Barda;
using Newtonsoft.Json;

namespace EDEngineer.Models
{
    public class Entry : INotifyPropertyChanged
    {
        public Entry(EntryData data)
        {
            Data = data;
        }

        public Entry()
        {
        }

        public EntryData Data { get; }
        private int count;
        private int favoriteCount;
        private int synthesisFavoriteCount;

        public int Count
        {
            get { return count; }
            set
            {
                if (value == count)
                {
                    return;
                }

                var oldValue = count;
                count = value;
                OnPropertyChanged(oldValue, count);
            }
        }

        [JsonIgnore]
        public int FavoriteCount
        {
            get { return favoriteCount; }
            set
            {
                if (value == favoriteCount)
                {
                    return;
                }

                var oldValue = favoriteCount;
                favoriteCount = value;
                OnPropertyChanged(oldValue, favoriteCount);
            }
        }

        [JsonIgnore]
        public int SynthesisFavoriteCount
        {
            get { return synthesisFavoriteCount; }
            set
            {
                if (value == synthesisFavoriteCount)
                {
                    return;
                }

                var oldValue = synthesisFavoriteCount;
                synthesisFavoriteCount = value;
                OnPropertyChanged(oldValue, synthesisFavoriteCount);
            }
        }

        public override string ToString()
        {
            return Data.Name + "(" + Count + ")";
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(int oldValue, int newValue, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs<int>(propertyName, oldValue, newValue));
        }
    }
}