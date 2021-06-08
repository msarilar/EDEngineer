using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class CollectItemOperation : JournalOperation
    {
        public string MaterialName { get; set; }

        public int Size { get; set; }

        public override void Mutate(State.State state)
        {
            state.History.IncrementCargo(MaterialName, Size);
        }

        public override Dictionary<string, int> Changes { get; } = new Dictionary<string, int>();
    }
}