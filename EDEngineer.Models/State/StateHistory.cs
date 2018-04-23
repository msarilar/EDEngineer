using System.Collections.Generic;

namespace EDEngineer.Models.State
{
    public class StateHistory
    {
        public StateHistory()
        {
            System = "Sol";
        }

        public string System { get; set; }
        public void IncrementCargo(string name, int change)
        {
            if (change <= 0)
            {
                return;
            }

            if (!Loots.ContainsKey(name))
            {
                Loots[name] = new Dictionary<string, int>();
            }

            if (!Loots[name].ContainsKey(System))
            {
                Loots[name][System] = change;
            }
            else
            {
                Loots[name][System] += change;
            }
        }

        public Dictionary<string, Dictionary<string, int>> Loots { get; } = new Dictionary<string, Dictionary<string, int>>();
    }
}