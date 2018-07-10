using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.State;
using EDEngineer.Models.Utils.Json;

namespace EDEngineer.Models.Operations
{
    public class DumpOperation : JournalOperation
    {
        public List<MaterialOperation> DumpOperations { get; set; } 

        public HashSet<Kind> ResetFilter { get; set; }

        public override void Mutate(IState state)
        {
            state.ApplyDump(DumpOperations, ResetFilter);
        }
    }
}