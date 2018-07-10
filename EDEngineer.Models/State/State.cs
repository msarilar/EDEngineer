using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.Loadout;
using EDEngineer.Models.Operations;
using NodaTime;

namespace EDEngineer.Models.State
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

        public void UpdateLoadout(ShipLoadout loadout)
        {
            Loadout.Update(loadout);
        }

        public void SetSystem(string system)
        {
            History.System = system;
        }

        public void ApplyCraft(EngineerOperation engineerOperation)
        {
            Loadout.ApplyCraft(engineerOperation);
        }

        public StateAggregation Aggregate(Instant lastTimestamp)
        {
            return new StateAggregation
            {
                Cargo = Cargo.Ingredients.ToDictionary(i => i.Key, i => i.Value.Count),
                History = History.Loots,
                System = History.System,
                Loadout = Loadout.Loadout,
                LastTimestamp = lastTimestamp
            };
        }

        public void ApplyAggregation(StateAggregation dump)
        {
            Cargo.InitLoad();
            foreach (var key in dump.Cargo.Keys)
            {
                Cargo.IncrementCargo(key, dump.Cargo[key]);
            }

            Cargo.Ingredients.RefreshSort();
            Cargo.CompleteLoad();

            foreach (var key in dump.History.Keys)
            {
                History.Loots[key] = dump.History[key];
            }

            History.System = dump.System;

            Loadout.Loadout = dump.Loadout;
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