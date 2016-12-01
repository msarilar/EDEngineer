using System.Linq;

namespace EDEngineer.Models.Operations
{
    public class DeathOperation : JournalOperation
    {
        public override void Mutate(State state)
        {
            foreach (var commodity in state.Cargo.Select(kv => kv.Value).Where(e => e.Data.Kind == Kind.Commodity && e.Count > 0).ToList())
            {
                state.IncrementCargo(commodity.Data.Name, commodity.Count * -1);
            }
        }
    }
}