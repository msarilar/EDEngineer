namespace EDEngineer.Models.Operations
{
    public class MaterialOperation : JournalOperation
    {
        public string MaterialName { get; set; }

        public int Size { get; set; }

        public override void Mutate(State state)
        {
            state.IncrementCargo(MaterialName, Size);
        }
    }
}