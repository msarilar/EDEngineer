using System;
using System.Collections.Generic;
using EDEngineer.Models.Loadout;

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

        public void OnBlueprintCrafted(BlueprintCategory category, string technicalName, List<BlueprintIngredient> ingredientsConsumed)
        {
            BlueprintCrafted?.Invoke(this, Tuple.Create(category, technicalName, ingredientsConsumed));
        }

        public event EventHandler<Tuple<BlueprintCategory, string, List<BlueprintIngredient>>> BlueprintCrafted;
    }
}