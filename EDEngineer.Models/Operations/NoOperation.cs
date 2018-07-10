using EDEngineer.Models.State;

namespace EDEngineer.Models.Operations
{
    public class NoOperation : JournalOperation
    {
        public override void Mutate(IState state)
        {
            // NOP
        }
    }
}