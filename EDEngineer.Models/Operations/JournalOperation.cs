namespace EDEngineer.Models.Operations
{
    public abstract class JournalOperation
    {
        public JournalEvent JournalEvent { get; set; }

        public abstract void Mutate(State state);
    }
}