using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class SynthesisOperation : JournalOperation
    {
        public List<JournalOperation> SynthesisPartOperation { get; set; }

        public override void Mutate(State state)
        {
            foreach (var reward in SynthesisPartOperation)
            {
                reward.Mutate(state);
            }
        }
    }
}