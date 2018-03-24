using System.Collections.Generic;

namespace EDEngineer.Models.Operations
{
    public class EngineerOperation : JournalOperation
    {
        public BlueprintCategory Category { get; }
        public string TechnicalName { get; }

        public List<BlueprintIngredient> IngredientsConsumed { get; set; }

        public EngineerOperation(BlueprintCategory category, string technicalName)
        {
            Category = category;
            TechnicalName = technicalName;
        }

        public override void Mutate(State state)
        {
            foreach (var ingredient in IngredientsConsumed)
            {
                state.Cargo.IncrementCargo(ingredient.Entry.Data.Name, -1 * ingredient.Size);
            }

            state.OnBlueprintCrafted(Category, TechnicalName, IngredientsConsumed);
        }
    }
}