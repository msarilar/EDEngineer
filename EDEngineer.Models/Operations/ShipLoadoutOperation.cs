using EDEngineer.Models.Loadout;

namespace EDEngineer.Models.Operations
{
    public class ShipLoadoutOperation : JournalOperation
    {
        private readonly ShipLoadout loadout;
        public ShipLoadoutOperation(ShipLoadout loadout)
        {
            this.loadout = loadout;
        }

        public override void Mutate(State.State state)
        {
            state.UpdateLoadout(loadout);
        }
    }
}