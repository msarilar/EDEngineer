using EDEngineer.Models.State;

namespace EDEngineer.Models.Operations
{
    public class MaterialOperation : JournalOperation
    {
        public string MaterialName { get; set; }

        public int Size { get; set; }

        public override void Mutate(IState state)
        {
            state.IncrementCargoWithHistory(MaterialName, Size);
        }
    }
}