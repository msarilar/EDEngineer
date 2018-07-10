using System;
using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.Loadout;
using EDEngineer.Models.Operations;
using EDEngineer.Models.Utils.Json;
using NodaTime;

namespace EDEngineer.Models.State
{
    public interface IState
    {
        void IncrementCargo(string name, int change);
        void IncrementCargoWithHistory(string name, int change);
        void UpdateLoadout(ShipLoadout loadout);
        void SetSystem(string system);
        void ApplyDump(List<MaterialOperation> dumpOperations, HashSet<Kind> resetFilter);
        void OnBlueprintCrafted(EngineerOperation engineerOperation);
        bool Contains(string name);
    }

    public class State : IState
    {
        public State(StateCargo stateCargo)
        {
            Cargo = stateCargo;
            History = new StateHistory();
            Loadout = new StateLoadout();
        }

        public void IncrementCargoWithHistory(string name, int change)
        {
            Cargo.IncrementCargo(name, change);
            History.IncrementCargo(name, change);
        }

        public void IncrementCargo(string name, int change)
        {
            Cargo.IncrementCargo(name, change);
        }

        public void UpdateLoadout(ShipLoadout loadout)
        {
            Loadout.Update(loadout);
        }

        public void SetSystem(string system)
        {
            History.System = system;
        }

        public void ApplyDump(List<MaterialOperation> dumpOperations, HashSet<Kind> resetFilter)
        {
            var dump = dumpOperations
                       .GroupBy(m => m.MaterialName)
                       .ToDictionary(m => m.Key, m => m.First().Size);
            foreach (var item in Cargo.Ingredients.Where(item => resetFilter.Contains(item.Value.Data.Kind)).ToList())
            {
                var currentValue = item.Value.Count;

                if (dump.TryGetValue(item.Key, out var toSetValue))
                {
                    if (currentValue != toSetValue)
                    {
                        Cargo.IncrementCargo(item.Key, toSetValue - currentValue);
                    }
                }
                else if (currentValue != 0)
                {
                    Cargo.IncrementCargo(item.Key, -1 * currentValue);
                }
            }

            var names = Cargo.Ingredients.Keys.ToHashSet();

            foreach (var item in dumpOperations.Where(op => !names.Contains(op.MaterialName)))
            {
                Cargo.IncrementCargo(item.MaterialName, item.Size);
            }
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

        public bool Contains(string name) => Cargo.Ingredients.ContainsKey(name);

        public event EventHandler<EngineerOperation> BlueprintCrafted;
    }
}