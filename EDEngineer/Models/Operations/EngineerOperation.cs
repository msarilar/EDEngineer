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
                if (state.Cargo.ContainsKey(ingredient.Name))
                {
                    state.IncrementCargo(ingredient.Name, -1*ingredient.Size);
                }
                else if (state.Data.ContainsKey(ingredient.Name))
                {
                    state.IncrementData(ingredient.Name, -1*ingredient.Size);
                }
                else if (state.Materials.ContainsKey(ingredient.Name))
                {
                    state.IncrementMaterials(ingredient.Name, -1*ingredient.Size);
                }
            }
        }
    }
}