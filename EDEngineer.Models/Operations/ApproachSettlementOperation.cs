using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class ApproachSettlementOperation : JournalOperation
    {
        public string SettlementName { get; }

        public ApproachSettlementOperation(string settlement)
        {
            SettlementName = settlement;
        }

        public override void Mutate(State.State state)
        {
            state.SetSettlement(SettlementName);
        }

        public override Dictionary<string, int> Changes { get; } = new Dictionary<string, int>();
    }
}