using System.Collections.Generic;
using EDEngineer.Models.State;

namespace EDEngineer.Models.Operations
{
    public class MissionCompletedOperation : JournalOperation
    {
        public List<CargoOperation> CommodityRewards { get; set; }

        public override void Mutate(IState state)
        {
            foreach (var reward in CommodityRewards)
            {
                reward.Mutate(state);
            }
        }
    }
}