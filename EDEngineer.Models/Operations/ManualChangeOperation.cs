using EDEngineer.Models.State;

namespace EDEngineer.Models.Operations
{
    public class ManualChangeOperation : JournalOperation
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public override void Mutate(IState state)
        {
            if (state.Contains(Name))
            {
                state.IncrementCargo(Name, Count);
            }
        }
    }
}