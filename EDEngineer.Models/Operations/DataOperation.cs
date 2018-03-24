namespace EDEngineer.Models.Operations
{
    public class DataOperation : JournalOperation
    {
        public string DataName { get; set; }

        public int Size { get; set; }

        public override void Mutate(State state)
        {
            state.IncrementCargo(DataName, Size);
        }
    }
}