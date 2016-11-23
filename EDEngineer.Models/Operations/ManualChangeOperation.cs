namespace EDEngineer.Models.Operations
{
    public class ManualChangeOperation : JournalOperation
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public override void Mutate(State state)
        {
            state.IncrementCargo(Name, Count);
        }
    }
}