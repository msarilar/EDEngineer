namespace EDEngineer.Models.Operations
{
    public class SystemUpdatedOperation : JournalOperation
    {
        public string System { get; }

        public SystemUpdatedOperation(string system)
        {
            System = system;
        }

        public override void Mutate(IState state)
        {
            state.System = System;
        }
    }
}