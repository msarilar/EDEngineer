using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class NoOperation : JournalOperation
    {
        public override void Mutate(State.State state)
        {
            // NOP
        }

        public override Dictionary<string, int> Changes { get; } = new Dictionary<string, int>();
    }
}