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
        public Entry(EntryData data)
        {
            Data = data;
        }

        public EntryData Data { get; }
        private int count;
        private int favoriteCount;

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