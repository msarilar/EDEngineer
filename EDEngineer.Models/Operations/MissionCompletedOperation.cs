using System.Collections.Generic;
using System.Linq;

namespace EDEngineer.Models.Operations
{
    public class MissionCompletedOperation : JournalOperation
    {
        public List<CargoOperation> CommodityRewards { get; set; }

        public override void Mutate(State.State state)
        {
            foreach (var reward in CommodityRewards)
            {
                reward.Mutate(state);
            }
        }

        public override Dictionary<string, int> Changes =>
            CommodityRewards
                .SelectMany(c => c.Changes)
                .GroupBy(c => c.Key)
                .ToDictionary(c => c.Key, c => c.Select(x => x.Value).Sum());
    }
}