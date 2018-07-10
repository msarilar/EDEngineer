using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class SystemUpdatedOperation : JournalOperation
    {
        public string System { get; }

        public SystemUpdatedOperation(string system)
        {
            System = system;
        }

        public override void Mutate(State.State state)
        {
            state.SetSystem(System);
        }

        public override Dictionary<string, int> Changes { get; } = new Dictionary<string, int>();
    }
}