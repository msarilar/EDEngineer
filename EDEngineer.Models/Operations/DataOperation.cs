using EDEngineer.Models.State;

namespace EDEngineer.Models.Operations
{
    public class DataOperation : JournalOperation
    {
        public string DataName { get; set; }

        public int Size { get; set; }

        public override void Mutate(IState state)
        {
            state.IncrementCargoWithHistory(DataName, Size);
        }
    }
}