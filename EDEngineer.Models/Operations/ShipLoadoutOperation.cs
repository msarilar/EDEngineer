using EDEngineer.Models.Loadout;
using EDEngineer.Models.State;

namespace EDEngineer.Models.Operations
{
    public class ShipLoadoutOperation : JournalOperation
    {
        private readonly ShipLoadout loadout;
        public ShipLoadoutOperation(ShipLoadout loadout)
        {
            this.loadout = loadout;
        }

        public override void Mutate(IState state)
        {
            state.UpdateLoadout(loadout);
        }
    }
}