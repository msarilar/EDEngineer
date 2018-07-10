using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class MaterialTradeOperation : JournalOperation
    {
        public string IngredientRemoved { get; set; }
        public int RemovedQuantity { get; set; }

        public string IngredientAdded { get; set; }
        public int AddedQuantity { get; set; }
        
        public override void Mutate(State.State state)
        {
            state.IncrementCargo(IngredientRemoved, -1 * RemovedQuantity);
            state.IncrementCargo(IngredientAdded, AddedQuantity);
        }

        public override Dictionary<string, int> Changes => new Dictionary<string, int>
        {
            [IngredientRemoved] = -1 * RemovedQuantity,
            [IngredientAdded] = AddedQuantity,
        };
    }
}