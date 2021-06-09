using System.Collections.Generic;

namespace EDEngineer.Models.State
{
    public class StateHistory
    {
        private string _system;
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
            }
        }
        public string Settlement { get; set; }
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
        }

        public Dictionary<string, Dictionary<string, int>> Loots { get; } = new Dictionary<string, Dictionary<string, int>>();
    }
}