using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDEngineer.Models.Operations
{
    public class LeaveSettlementOperation : JournalOperation
    {
        public override void Mutate(State.State state)
        {
            state.SetSettlement(null);
        }

        public override Dictionary<string, int> Changes { get; } = new Dictionary<string, int>();
    }
}
