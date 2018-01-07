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
            var dump = DumpOperations.ToDictionary(m => m.MaterialName, m => m.Size);
            foreach (var item in state.Cargo.Where(item => ResetFilter.Contains(item.Value.Data.Kind)).ToList())
            {
                var currentValue = item.Value.Count;

                int toSetValue;
                if (dump.TryGetValue(item.Key, out toSetValue))
                {
                    if (currentValue != toSetValue)
                    {
                        state.IncrementCargo(item.Key, toSetValue - currentValue);
                    }
                }
                else if(currentValue != 0)
                {
                    state.IncrementCargo(item.Key, -1 * currentValue);
                }
            }
        }
    }
}