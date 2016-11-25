using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class CargoOperation : JournalOperation
    {
        public string CommodityName { get; set; }

        public int Size { get; set; }

        public override void Mutate(State state)
        {
            state.IncrementCargo(CommodityName, Size);
        }
    }
}