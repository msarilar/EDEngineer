using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class MaterialOperation : JournalOperation
    {
        public string MaterialName { get; set; }

        public int Size { get; set; }

        public override void Mutate(State.State state)
        {
            state.IncrementCargo(MaterialName, Size, false);
        }

        public override Dictionary<string, int> Changes => new Dictionary<string, int>
        {
            [MaterialName] = Size
        };
    }
}