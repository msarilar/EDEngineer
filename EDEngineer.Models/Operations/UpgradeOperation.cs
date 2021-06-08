using System.Collections.Generic;
using System.Linq;

namespace EDEngineer.Models.Operations
{
    public class UpgradeOperation : JournalOperation
    {
        private readonly Dictionary<string, int> changes = new Dictionary<string, int>();

        public string EquipmentName { get; set; }

        public int Class { get; set; }

        public override Dictionary<string, int> Changes => changes;

        public override void Mutate(State.State state)
        {
            var blueprint = state.Blueprints.FirstOrDefault(x => x.BlueprintName == EquipmentName && x.Grade == Class);
            if (blueprint != null)
            {
                foreach (var item in blueprint.Ingredients)
                {
                    state.IncrementCargo(item.Entry.Data.Name, -item.Size);
                    changes.Add(item.Entry.Data.Name, -item.Size);
                }
            }
        }
    }
}