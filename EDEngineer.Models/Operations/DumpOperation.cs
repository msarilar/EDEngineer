using System.Collections.Generic;
using System.Linq;
using EDEngineer.Models.Utils.Json;

namespace EDEngineer.Models.Operations
{
    public class DumpOperation : JournalOperation
    {
        public List<MaterialOperation> DumpOperations { get; set; } 

        public HashSet<Kind> ResetFilter { get; set; }

        public override void Mutate(State.State state)
        {
            var dump = DumpOperations
                .GroupBy(m => m.MaterialName)
                .ToDictionary(m => m.Key, m => m.First().Size);
            foreach (var item in state.Cargo.Ingredients.Where(item => ResetFilter.Contains(item.Value.Data.Kind)).ToList())
            {
                var currentValue = item.Value.Count;

                if (dump.TryGetValue(item.Key, out var toSetValue))
                {
                    if (currentValue != toSetValue)
                    {
                        state.Cargo.IncrementCargo(item.Key, toSetValue - currentValue);
                    }
                }
                else if (currentValue != 0)
                {
                    state.Cargo.IncrementCargo(item.Key, -1 * currentValue);
                }
            }

            var names = state.Cargo.Ingredients.Keys.ToHashSet();

            foreach (var item in DumpOperations.Where(op => !names.Contains(op.MaterialName)))
            {
                state.Cargo.IncrementCargo(item.MaterialName, item.Size);
            }
        }
    }
}