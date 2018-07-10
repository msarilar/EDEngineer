using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class DataOperation : JournalOperation
    {
        public string DataName { get; set; }

        public int Size { get; set; }

        public override void Mutate(State.State state)
        {
            state.IncrementCargo(DataName, Size);
        }

        public override Dictionary<string, int> Changes => new Dictionary<string, int>
        {
            [DataName] = Size
        };
    }
}