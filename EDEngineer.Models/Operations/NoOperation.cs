namespace EDEngineer.Models.Operations
{
    public class NoOperation : JournalOperation
    {
        public override void Mutate(State state)
        {
            // NOP
        }
    }
}