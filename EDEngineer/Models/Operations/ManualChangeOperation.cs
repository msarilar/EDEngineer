namespace EDEngineer.Models.Operations
{
    public class ManualChangeOperation : JournalOperation
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public override void Mutate(State state)
        {
            if (state.Cargo.ContainsKey(Name))
            {
                state.IncrementCargo(Name, Count);
            }
        }
    }
}