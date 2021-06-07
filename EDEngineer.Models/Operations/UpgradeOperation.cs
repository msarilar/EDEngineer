using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDEngineer.Models.Operations
{
    public class UpgradeOperation : JournalOperation
    {
        private Dictionary<string, int> _changes = new Dictionary<string, int>();

        public JournalEvent Event { get; set; }

        public string Name { get; set; }

        public int Class { get; set; }

        public override Dictionary<string, int> Changes => _changes;

        public override void Mutate(State.State state)
        {
            var equipement = GetEquipment(state);
            var blueprint = state.Blueprints.FirstOrDefault(x => x.BlueprintName == equipement.Name && x.Grade == Class);
            if (blueprint != null)
            {
                foreach (var item in blueprint.Ingredients)
                {
                    state.IncrementCargo(item.Entry.Data.Name, -item.Size);
                    _changes.Add(item.Entry.Data.Name, -item.Size);
                }
            }
        }

        private Equipment GetEquipment(State.State state)
        {
            Equipment equipment;
            if (Event == JournalEvent.UpgradeWeapon)
            {
                equipment = state.Equipments[Name];
            }
            else
            {
                string suitType = Name.Split("_".ToCharArray())[0];
                equipment = state.Equipments[suitType];
            }
            return equipment;
        }
    }
}