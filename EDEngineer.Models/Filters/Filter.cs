using System.ComponentModel;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Utils;

namespace EDEngineer.Models.Filters
{
    public abstract class Filter<T> : INotifyPropertyChanged, ILabelledFilter
    {
        public string UniqueName { get; }
        private bool @checked;

        protected Filter(string uniqueName)
        {
            UniqueName = uniqueName;
        }

        public bool Checked
        {
            get => @checked;
            set
            {
                if (value == @checked) return;
                @checked = value;
                OnPropertyChanged();
            }
        }

        public abstract string Label { get; }

        public bool Magic { get; protected set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract bool AppliesTo(T blueprint);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}