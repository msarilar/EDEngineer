using System;
using System.Collections.Generic;
using EDEngineer.Models.Loadout;
using EDEngineer.Models.Operations;

namespace EDEngineer.Models
{
    public class State
    {
        public State(StateCargo stateCargo)
        {
            Cargo = stateCargo;
            History = new StateHistory();
            Loadout = new StateLoadout();
        }

        public void IncrementCargo(string name, int change)
        {
            Cargo.IncrementCargo(name, change);
            History.IncrementCargo(name, change);
        }

        public LinkedList<JournalEntry> Operations { get; } = new LinkedList<JournalEntry>();
        public List<Blueprint> Blueprints { get; set; }

        public StateCargo Cargo { get; }
        public StateHistory History { get; }
        public StateLoadout Loadout { get; }

        public void OnBlueprintCrafted(EngineerOperation operation)
        {
            BlueprintCrafted?.Invoke(this, operation);
        }

        public event EventHandler<EngineerOperation> BlueprintCrafted;
    }
}