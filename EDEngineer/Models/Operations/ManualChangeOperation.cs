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
            else if (state.Data.ContainsKey(Name))
            {
                state.IncrementData(Name, Count);
            }
            else if (state.Materials.ContainsKey(Name))
            {
                state.IncrementMaterials(Name, Count);
            }
        }
    }
}