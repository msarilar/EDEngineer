using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class CargoOperation : JournalOperation
    {
        public string CommodityName { get; set; }

        public int Size { get; set; }

        public override void Mutate(State.State state)
        {
            state.IncrementCargo(CommodityName, Size);
        }

        public override Dictionary<string, int> Changes => new Dictionary<string, int>
        {
            [CommodityName] = Size
        };
    }
}