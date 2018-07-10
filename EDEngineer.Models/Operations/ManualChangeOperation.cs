using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class ManualChangeOperation : JournalOperation
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public override void Mutate(State.State state)
        {
            if (state.Cargo.Ingredients.ContainsKey(Name))
            {
                state.IncrementCargo(Name, Count);
            }
        }

        public override Dictionary<string, int> Changes => new Dictionary<string, int>
        {
            [Name] = Count
        };
    }
}