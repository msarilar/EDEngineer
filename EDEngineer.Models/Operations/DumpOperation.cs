using System.Collections.Generic;
using System.Linq;

namespace EDEngineer.Models.Operations
{
    public class DumpOperation : JournalOperation
    {
        public List<MaterialOperation> DumpOperations { get; set; } 

        public HashSet<Kind> ResetFilter { get; set; }

        public override void Mutate(State state)
        {
            foreach (var item in state.Cargo.Where(item => ResetFilter.Contains(item.Value.Data.Kind)))
            {
                state.IncrementCargo(item.Key, -1 * item.Value.Count);
            }

            foreach (var operation in DumpOperations)
            {
                state.IncrementCargo(operation.MaterialName, operation.Size);
            }
        }
    }
}