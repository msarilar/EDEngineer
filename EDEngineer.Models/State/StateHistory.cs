using EDEngineer.Models.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EDEngineer.Models.State
{
    public class StateHistory : INotifyPropertyChanged
    {
        private string _system;
        private string _settlement;
        public StateHistory()
        {
            System = "Sol";
        }


        public string System { 
            get => _system; 
            set {
                if(_system != value)
                {
                    Settlement = string.Empty;
                }

                _system = value;
                OnPropertyChanged();

            }
        }
        public string Settlement
        {
            get => _settlement;
            set
            {
                _settlement = value;
                OnPropertyChanged();
            }
        }

        public void IncrementCargo(string name, int change, bool reward)
        {
            if (change <= 0)
            {
                return;
            }

            if (!Loots.ContainsKey(name))
            {
                Loots[name] = new Dictionary<string, int>();
            }

            string location = System;
            if (reward)
            {
                location = "Mission reward";
            }
            else
            {
                if (!string.IsNullOrEmpty(Settlement))
                {
                    location = $"{Settlement} ({System})";
                }
            }
            
            if (!Loots[name].ContainsKey(location))
            {
                Loots[name][location] = change;
            }
            else
            {
                Loots[name][location] += change;
            }
            OnPropertyChanged();
        }

        public Dictionary<string, Dictionary<string, int>> Loots { get; } = new Dictionary<string, Dictionary<string, int>>();

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}