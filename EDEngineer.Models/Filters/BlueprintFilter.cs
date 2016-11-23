using System.ComponentModel;
using System.Runtime.CompilerServices;
using EDEngineer.Models.Barda;

namespace EDEngineer.Models.Filters
{
    public abstract class BlueprintFilter : INotifyPropertyChanged
    {
        public string UniqueName { get; }
        private bool @checked;

        protected BlueprintFilter(string uniqueName)
        {
            UniqueName = uniqueName;
        }

        public bool Checked
        {
            get { return @checked; }
            set
            {
                if (value == @checked) return;
                @checked = value;
                OnPropertyChanged();
            }
        }

        public bool Magic { get; protected set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract bool AppliesTo(Blueprint blueprint);

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}