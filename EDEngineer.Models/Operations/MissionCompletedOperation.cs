using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class MissionCompletedOperation : JournalOperation
    {
        public List<CargoOperation> CommodityRewards { get; set; }

        public override void Mutate(State state)
        {
            foreach (var reward in CommodityRewards)
            {
                reward.Mutate(state);
            }
        }
    }
}