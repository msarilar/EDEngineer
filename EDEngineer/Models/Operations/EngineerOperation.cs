using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class EngineerOperation : JournalOperation
    {
        public List<BlueprintIngredient> IngredientsConsumed { get; set; }

        public override void Mutate(State state)
        {
            foreach (var ingredient in IngredientsConsumed)
            {
                state.IncrementCargo(ingredient.Entry.Data.Name, -1 * ingredient.Size);
            }
        }
    }
}